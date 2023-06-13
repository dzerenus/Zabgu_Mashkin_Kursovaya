namespace UDPFlood.Ethernet.FloodSettings;

public class DelaySettings
{
    public bool IsDelay { get; }

    /// <summary>
    /// Каждые {Every} секунд;
    /// </summary>
    public int Every { get; }
    
    /// <summary>
    /// Количество пакетов в одной партии
    /// </summary>
    public int PacketCount { get; }
    
    /// <summary>
    /// С задержкой {Delay} между пакетами;
    /// </summary>
    public int Delay { get; }

    public DelaySettings(bool isDelay, int every, int count, int delay)
    {
        IsDelay = isDelay;

        if (IsDelay)
        {
            // Проверки
        }

        Every = every;
        PacketCount = count;
        Delay = delay;
    }
}
