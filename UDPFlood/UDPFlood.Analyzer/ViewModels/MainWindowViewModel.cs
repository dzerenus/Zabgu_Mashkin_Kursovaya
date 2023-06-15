using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;
using UDPFlood.Analyzer.Models;

namespace UDPFlood.Analyzer.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public int TotalCount { get; set; } = 0;
    public List<IpPacketCountRow> IpPackets { get; set; } = new();
    public List<IpPacketCountRow> TopSenders { get; set; } = new();

    public string SelectedDeviceName
    {
        get => _analyzer.SelectedDeviceName;
        set
        {
            _analyzer.SelectDevice(value);
            OnPropertyChanged(nameof(SelectedDeviceName));
        }
    }
    public IEnumerable<string> DeviceNames => _analyzer.DeviceNames;
    public ObservableCollection<UdpPacketViewModel> Packets { get; set; } = new();

    public string StartStopScanningButtonText { get; set; } = "Начать сканирование";
    public ICommand StartOrStopScanning { get; }

    private readonly UdpAnalyzer _analyzer = new();

    public MainWindowViewModel()
    {
        _analyzer.OnEveryMinute += () =>
        {
            TotalCount = _analyzer.TotalPacketCount;

            IpPackets = new();
            foreach (var pair in _analyzer.PacketCountByIp)
                IpPackets.Add(new(pair.Key, pair.Value, TotalCount));

            IpPackets = IpPackets.OrderByDescending(x => x.PacketCount).Take(5).ToList();

            OnPropertyChanged(nameof(IpPackets));
            OnPropertyChanged(nameof(TotalCount));
        };


        StartOrStopScanning = new RelayCommand(_ =>
        {
            if (_analyzer.IsRunning)
            {
                _analyzer.Stop();
                StartStopScanningButtonText = "Начать сканирование";
                OnPropertyChanged(nameof(StartStopScanningButtonText));
            } 
            
            else
            {
                _analyzer.Start();

                var retakeThread = new Thread(RetakePacketsThreadStart);
                retakeThread.Start();

                StartStopScanningButtonText = "Остановить сканирование";
                OnPropertyChanged(nameof(StartStopScanningButtonText));
            }
        });

    }

    public void UpdateTopSenders(IpPacketCountRow row)
    {
        var dict = _analyzer.PacketCountByIp;
        var senders = _analyzer.MinutePackets.Where(x => x.IpDestination.ToString() == row.Ip);

        var result = new Dictionary<IPAddress, int>();
        var total = 0;

        foreach (var sender in senders)
        {
            var ip = IPAddress.Parse(sender.IpSource.ToString());
            
            if (result.ContainsKey(ip))
                result[ip]++;

            else result[ip] = 1;
            total++;
        }

        TopSenders = new();

        foreach (var sender in result)
            TopSenders.Add(new (sender.Key, sender.Value, total));

        TopSenders = TopSenders.OrderByDescending(x => x.PacketCount).Take(5).ToList();
        OnPropertyChanged(nameof(TopSenders));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged(string prop)
        => PropertyChanged?.Invoke(this, new(prop));

    private void RetakePacketsThreadStart()
    {
        while (_analyzer.IsRunning)
        {
            var packets = _analyzer.Packets.TakeLast(20);
            Packets = new(packets.Select(x => new UdpPacketViewModel(x)));
            OnPropertyChanged(nameof(Packets));

            Thread.Sleep(1000);
        }
    }
}

public class IpPacketCountRow
{
    public string Ip { get; }
    public int PacketCount { get; }
    public int Percents { get; }

    public IpPacketCountRow(IPAddress ip, int ipCount, int totalCount)
    {
        Ip = ip.ToString();
        PacketCount = ipCount;
        Percents = Convert.ToInt32((double)ipCount / totalCount * 100);
    }

    public override string ToString()
    {
        return $"{Ip} ({PacketCount}) - {Percents}%";
    }
}
