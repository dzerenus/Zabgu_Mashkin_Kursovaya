namespace UDPFlood.Ethernet.FloodSettings;

public enum IpMode
{
    MySelf,
    Random,
    FromList
}

public class IpSettings
{
    public IpMode Mode { get; }
    public IEnumerable<string>? Addresses { get; }

    public IpSettings(IpMode mode, IEnumerable<string>? addresses = null)
    {
        Mode = mode;

        if (mode == IpMode.FromList && addresses == null)
            throw new NullReferenceException();

        Addresses = addresses;
    }
}
