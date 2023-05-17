namespace UDPFlood.Ethernet.FloodSettings;

public enum PortMode
{
    Random,
    FromList
}

public class PortSettings
{
    public PortMode Mode { get; }
    public IEnumerable<int>? Ports { get; }

    public PortSettings(PortMode mode, IEnumerable<int>? ports = null)
    {
        Mode = mode;

        if (mode == PortMode.FromList && ports == null)
            throw new NullReferenceException();

        Ports = ports;
    }
}
