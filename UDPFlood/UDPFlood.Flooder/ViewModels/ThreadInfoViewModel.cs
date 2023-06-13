using System;
using System.Text;
using System.Collections.Generic;
using UDPFlood.Ethernet;
using UDPFlood.Ethernet.FloodSettings;

namespace UDPFlood.Flooder.ViewModels;

public class ThreadInfoViewModel
{
    public string WorkStatus { get; }
    public string DstIpMode { get; }
    public string DstMacMode { get; }
    public string SrcPortsMode { get; }
    public string SrcIpMode { get; }
    public string SrcMacMode { get; }
    public string DstPortsMode { get; }
    public string PacketContentMode { get; }
    public int TTL { get; }
    public ulong PacketCount { get; }
    public string LastPacketSendedAt { get; }

    public ThreadInfoViewModel(BomberThreadInfo threadInfo)
    {
        TTL = threadInfo.Settings.Ttl;
        PacketCount = threadInfo.PacketCount;

        if (threadInfo.LastPacketSendedAt == DateTimeOffset.MinValue)
            LastPacketSendedAt = "Никогда";
        else 
            LastPacketSendedAt = threadInfo.LastPacketSendedAt.ToString("HH:mm:ss");

        WorkStatus = threadInfo.ThreadStatus switch
        {
            ThreadStatus.Stop => "Не запущен",
            ThreadStatus.Awaited => "Ждёт задержку",
            ThreadStatus.InProgress => "В процессе",
            _ => throw new InvalidOperationException()
        };

        DstIpMode = threadInfo.Settings.DestinationIp.Mode switch
        {
            IpMode.Random => "Случайно",
            IpMode.MySelf => "Используется свой",
            IpMode.FromList => GetStringFromList(threadInfo.Settings.DestinationIp.Addresses),
            _ => throw new InvalidOperationException()
        };

        SrcIpMode = threadInfo.Settings.SourceIp.Mode switch
        {
            IpMode.Random => "Случайно",
            IpMode.MySelf => "Используется свой",
            IpMode.FromList => GetStringFromList(threadInfo.Settings.SourceIp.Addresses),
            _ => throw new InvalidOperationException()
        };

        DstMacMode = threadInfo.Settings.DestinationMac.Mode switch
        {
            MacMode.Random => "Случайно",
            MacMode.MySelf => "Используется свой",
            MacMode.FromList => GetStringFromList(threadInfo.Settings.DestinationMac.Addresses),
            _ => throw new InvalidOperationException()
        };

        SrcMacMode = threadInfo.Settings.SourceMac.Mode switch
        {
            MacMode.Random => "Случайно",
            MacMode.MySelf => "Используется свой",
            MacMode.FromList => GetStringFromList(threadInfo.Settings.SourceMac.Addresses),
            _ => throw new InvalidOperationException()
        };

        SrcPortsMode = threadInfo.Settings.SourcePort.Mode switch
        {
            PortMode.Random => "Случайно",
            PortMode.FromList => GetStringFromList(threadInfo.Settings.SourcePort.Ports),
            _ => throw new InvalidOperationException()
        };

        DstPortsMode = threadInfo.Settings.DestinationPort.Mode switch
        {
            PortMode.Random => "Случайно",
            PortMode.FromList => GetStringFromList(threadInfo.Settings.DestinationPort.Ports),
            _ => throw new InvalidOperationException()
        };

        PacketContentMode = threadInfo.Settings.Content.Mode switch
        {
            ContentMode.Empty => "Пусто",
            ContentMode.Random => "Случайное",
            ContentMode.FromContent => threadInfo.Settings.Content.Content ?? string.Empty,
            _ => throw new InvalidOperationException()
        };
    }

    private static string GetStringFromList(IEnumerable<int>? input)
    {
        if (input == null)
            return string.Empty;

        var sb = new StringBuilder();

        foreach (var inputItem in input)
        {
            sb.Append(inputItem);
            sb.Append(Environment.NewLine);
        }

        return sb.ToString();
    }

    private static string GetStringFromList(IEnumerable<string>? input)
    {
        if (input == null)
            return string.Empty;

        var sb = new StringBuilder();
        
        foreach (var inputItem in input)
        {
            sb.Append(inputItem);
            sb.Append(Environment.NewLine);
        }

        return sb.ToString();
    }
}
