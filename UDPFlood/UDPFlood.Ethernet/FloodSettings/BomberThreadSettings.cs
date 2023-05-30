namespace UDPFlood.Ethernet.FloodSettings;

public sealed class BomberThreadSettings
{
    public ContentSettings Content { get; }
    public DelaySettings Delay { get; }
    public IpSettings Ip { get; }
    public PortSettings DestPort { get; }
    public PortSettings SrcPort { get; }
    public MacSettings Mac { get; }

    public BomberThreadSettings(ContentSettings content, DelaySettings delay, IpSettings ip, PortSettings port, PortSettings srcPort, MacSettings mac)
    {
        Content = content;
        Delay = delay;
        Ip = ip;
        DestPort = port;
        SrcPort = srcPort;
        Mac = mac;
    }
}