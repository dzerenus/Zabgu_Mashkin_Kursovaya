using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using UDPFlood.Analyzer.Models;

namespace UDPFlood.Analyzer.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
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
