namespace UDPFlood.Ethernet.FloodSettings;

public sealed class BomberThreadSettings
{
    public ContentSettings Content { get; }
    public DelaySettings Delay { get; }
    public IpSettings SourceIp { get; }
    public IpSettings DestinationIp { get; }
    public MacSettings SourceMac { get; }
    public MacSettings DestinationMac { get; }
    public PortSettings SourcePort { get; }
    public PortSettings DestinationPort { get; }
    public byte Ttl { get; }

    public BomberThreadSettings(
        ContentSettings content, 
        DelaySettings delay, 
        IpSettings sourceIp, 
        IpSettings destinationIp, 
        MacSettings sourceMac, 
        MacSettings destinationMac, 
        PortSettings sourcePort, 
        PortSettings destinationPort,
        byte ttl)
    {
        Content = content;
        Delay = delay;
        SourceIp = sourceIp;
        DestinationIp = destinationIp;
        SourceMac = sourceMac;
        DestinationMac = destinationMac;
        SourcePort = sourcePort;
        DestinationPort = destinationPort;
        Ttl = ttl;
    }
}