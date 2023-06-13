namespace UDPFlood.Flooder.ViewModels;

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using UDPFlood.Ethernet.FloodSettings;

public class ThreadSettingsViewModel
{
    public string WorkStatus { get; }
    public string SrcIpMode { get; }
    public string SrcMacMode { get; }
    public string DstPortsMode { get; }
    public string PacketContentMode { get; }

    public ThreadSettingsViewModel(BomberThreadSettings thread)
    {
        WorkStatus = "Флудер не запущен";
        
        SrcIpMode = thread.SourceIp.Mode switch 
        {
            IpMode.Random => "Случайно",
            IpMode.MySelf => "Используется свой",
            IpMode.FromList => $"Задано {thread.SourceIp.Addresses?.Count()}",
            _ => throw new InvalidOperationException()
        };

        SrcMacMode = thread.SourceMac.Mode switch
        {
            MacMode.Random => "Случайно",
            MacMode.MySelf => "Используется свой",
            MacMode.FromList => $"Задано {thread.SourceMac.Addresses?.Count()}",
            _ => throw new InvalidOperationException()
        };

        DstPortsMode = thread.DestinationPort.Mode switch 
        {
            PortMode.Random => "Случайно",
            PortMode.FromList => $"Задано {thread.DestinationPort.Ports?.Count()}",
            _ => throw new InvalidOperationException()
        };

        PacketContentMode = thread.Content.Mode switch
        {
            ContentMode.Empty => "Пусто",
            ContentMode.Random => "Случайное",
            ContentMode.FromContent => "Установлено",
            _ => throw new InvalidOperationException()
        };
    }
}
