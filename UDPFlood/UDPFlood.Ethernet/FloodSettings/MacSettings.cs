namespace UDPFlood.Ethernet.FloodSettings;

public enum MacMode
{
    Random,
    MySelf,
    FromList
}

public class MacSettings
{
    public MacMode Mode { get; }
    public IEnumerable<string>? Addresses { get; }

    public MacSettings(MacMode mode, IEnumerable<string>? addresses = null)
    {
        Mode = mode;

        if (mode == MacMode.FromList && addresses == null)
            throw new NullReferenceException();

        Addresses = addresses;
    }
}
