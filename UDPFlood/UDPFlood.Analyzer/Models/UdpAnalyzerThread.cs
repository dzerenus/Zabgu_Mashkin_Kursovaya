using System.Threading;
using System.Threading.Tasks;
using PcapDotNet.Core;

namespace UDPFlood.Analyzer.Models;

public delegate void OnUdpThreadChange(UdpPacket packet);

public class UdpAnalyzerThread
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly LivePacketDevice _device;

    public UdpAnalyzerThread(LivePacketDevice device)
    {
        _device = device;
    }

    ~UdpAnalyzerThread() => Stop();

    public event OnUdpThreadChange? OnNewPacket;

    public void Start()
    {
        var cancellationToken = _cancellationTokenSource.Token;

        var thread = new Thread(async () =>
        {
            using var communicator = _device.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 100);
            communicator.SetFilter(communicator.CreateFilter("udp"));

            while (!cancellationToken.IsCancellationRequested)
            {
                var packet = await RecievePacket(communicator, cancellationToken);

                if (packet != null)
                    OnNewPacket?.Invoke(packet);
            }
        });

        thread.Start();
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }

    private async Task<UdpPacket?> RecievePacket(PacketCommunicator communicator, CancellationToken cancellationToken)
    {
        var result = communicator.ReceivePacket(out var packet);

        if (result != PacketCommunicatorReceiveResult.Ok
            || packet.Ethernet.IpV4?.Udp == null
            || packet.Ethernet.EtherType != PcapDotNet.Packets.Ethernet.EthernetType.IpV4)
            return null;

        var macSrc = packet.Ethernet.Source;
        var macDst = packet.Ethernet.Destination;

        var ipSrc = packet.Ethernet.IpV4.Source;
        var ipDst = packet.Ethernet.IpV4.Destination;

        var udpPortSrc = packet.Ethernet.IpV4.Udp.SourcePort;
        var udpPortDst = packet.Ethernet.IpV4.Udp.DestinationPort;

        var buffer = new byte[packet.Ethernet.IpV4.Udp.Payload.Length];
        var contentStream = packet.Ethernet.IpV4.Udp.Payload.ToMemoryStream();
        await contentStream.ReadAsync(buffer, cancellationToken);

        var udpPacket = new UdpPacket(macSrc, macDst, ipSrc, ipDst, udpPortSrc, udpPortDst)
        {
            Content = buffer
        };

        return udpPacket;
    }
}
