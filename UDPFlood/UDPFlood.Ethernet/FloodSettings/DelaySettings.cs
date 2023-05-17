namespace UDPFlood.Ethernet.FloodSettings;

public class DelaySettings
{
    public bool IsDelay { get; }
    public int Every { get; }
    public int Length { get; }
    public int Delay { get; }

    public DelaySettings(bool isDelay, int every, int length, int delay)
    {
        IsDelay = isDelay;

        if (IsDelay)
        {
            // Проверки
        }

        Every = every;
        Length = length;
        Delay = delay;
    }
}
