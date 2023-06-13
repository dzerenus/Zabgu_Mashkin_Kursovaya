using UDPFlood.Ethernet.FloodSettings;

namespace UDPFlood.Ethernet;

public class BomberThreadInfo
{
    public Guid ThreadId { get; }
    public ThreadStatus ThreadStatus { get; }
    public DateTimeOffset LastPacketSendedAt { get; }
    public ulong PacketCount { get; }
    public BomberThreadSettings Settings { get; }

    public BomberThreadInfo(Guid threadId, ThreadStatus threadStatus, DateTimeOffset lastPacketSendedAt, BomberThreadSettings settings, ulong packetCount)
    {
        ThreadId = threadId;
        ThreadStatus = threadStatus;
        LastPacketSendedAt = lastPacketSendedAt;
        Settings = settings;
        PacketCount = packetCount;
    }
}
