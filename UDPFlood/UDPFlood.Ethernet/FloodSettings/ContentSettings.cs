namespace UDPFlood.Ethernet.FloodSettings;

public enum ContentMode
{
    Empty,
    Random,
    FromContent
}

public class ContentSettings
{
    public ContentMode Mode { get; }
    public string? Content { get; }

    public ContentSettings(ContentMode mode, string? content = null)
    {
        Mode = mode;

        if (Mode == ContentMode.FromContent && content == null)
            throw new NullReferenceException();

        Content = content;
    }
}
