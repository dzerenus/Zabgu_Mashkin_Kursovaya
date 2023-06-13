using System;
using System.Collections.Generic;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;

namespace UDPFlood.Analyzer.Models;

public class UdpPacket
{
    public DateTimeOffset Time { get; } = DateTimeOffset.Now;
    public MacAddress MacSource { get; }
    public MacAddress MacDestination { get; }
    public IpV4Address IpSource { get; }
    public IpV4Address IpDestination { get; }
    public ushort PortSource { get; }
    public ushort PortDestination { get; }
    public IReadOnlyCollection<byte> Content { get; init; } = Array.Empty<byte>();

    public UdpPacket(MacAddress macSource, MacAddress macDestination, IpV4Address ipSource, IpV4Address ipDestination, ushort portSource, ushort portDestination)
    {
        MacSource = macSource;
        MacDestination = macDestination;
        IpSource = ipSource;
        IpDestination = ipDestination;
        PortSource = portSource;
        PortDestination = portDestination;
    }
}
