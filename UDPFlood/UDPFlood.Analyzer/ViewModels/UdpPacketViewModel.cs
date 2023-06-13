using System.Linq;
using System.Text;
using UDPFlood.Analyzer.Models;

namespace UDPFlood.Analyzer.ViewModels;

public class UdpPacketViewModel
{
    public string Time { get; }
    public string MacSrc { get; }
    public string MacDst { get; }
    public string IpSrc { get; }
    public string IpDst { get; }
    public int PortSrc { get; }
    public int PortDst { get; }
    public string Content { get; }

    public UdpPacketViewModel(UdpPacket packet)
    {
        Time = packet.Time.ToString("HH:mm:ss");
        MacSrc = packet.MacSource.ToString();
        MacDst = packet.MacDestination.ToString();
        IpSrc = packet.IpSource.ToString();
        IpDst = packet.IpDestination.ToString();
        PortSrc = packet.PortSource;
        PortDst = packet.PortDestination;
        Content = Encoding.UTF8.GetString(packet.Content.ToArray());
    }
}
