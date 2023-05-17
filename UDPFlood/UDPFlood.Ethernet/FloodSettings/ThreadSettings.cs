namespace UDPFlood.Ethernet.FloodSettings;

public sealed class ThreadSettings
{
    public ContentSettings Content { get; }
    public DelaySettings Delay { get; }
    public IpSettings Ip { get; }
    public PortSettings Port { get; }
    public MacSettings Mac { get; }

    public ThreadSettings(ContentSettings content, DelaySettings delay, IpSettings ip, PortSettings port, MacSettings mac)
    {
        Content = content;
        Delay = delay;
        Ip = ip;
        Port = port;
        Mac = mac;
    }
}
