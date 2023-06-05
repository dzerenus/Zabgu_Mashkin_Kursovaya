using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UDPFlood.Ethernet;

namespace UDPFlood.Flooder.ViewModels;

public class ScanWindowVM : INotifyPropertyChanged
{
    public bool IsScanButtonEnabled { get; private set; } = false;

    public int ScanProgress { get; private set; } = 0;
    public string SourceIP { get => Bomber?.DeviceSourceAddress.ToString() ?? string.Empty; }

    public string SelectedInterface
    {
        get => Bomber?.SelectedDeviceName ?? string.Empty;
        set
        {
            Bomber?.SelectDevice(value);
            OnPropertyChanged(nameof(SelectedInterface));
        }
    }
    
    public UdpBomber? Bomber { get; private set; }
    public Scanner? NetworkScanner { get; private set; }

    public ObservableCollection<ArpRow> ArpTable { get; set; } = new ();
    public ICommand ScanCommand { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ScanWindowVM()
    {
        ScanCommand = new RelayCommand(_ => DoScan());
    }

    private void DoScan()
    {
        try
        {
            IsScanButtonEnabled = !IsScanButtonEnabled;
            OnPropertyChanged(nameof(IsScanButtonEnabled));

            if (Bomber == null)
                return;

            if (Bomber.NetworkScanner.ScanInProgress)
                Bomber.NetworkScanner.StopScan();

            else
                Bomber.NetworkScanner.StartScan();
        }

        catch (InvalidOperationException)
        {
            MessageBox.Show("Ошибка! Устройство отключено или недоступно!");
        }
    }

    public void SetBomber(UdpBomber bomber)
    {
        Bomber = bomber;
        ArpTable = new(Bomber.NetworkScanner.ArpTable);
        Bomber.OnSourceIpChanged += () => OnPropertyChanged(nameof(SourceIP));

        IsScanButtonEnabled = !Bomber.NetworkScanner.ScanInProgress;

        Bomber.NetworkScanner.OnArpAdded += () =>
        {
            ArpTable = new(Bomber.NetworkScanner.ArpTable);
            OnPropertyChanged(nameof(ArpTable));
        };

        Bomber.NetworkScanner.OnArpProgressChanged += progress =>
        {
            ScanProgress = Convert.ToInt32(Convert.ToDouble(progress) / 256 * 100);
            OnPropertyChanged(nameof(ScanProgress));
        };

        Bomber.NetworkScanner.OnScanEnded += () =>
        {
            IsScanButtonEnabled = true;
            ScanProgress = 0;
            OnPropertyChanged(nameof(IsScanButtonEnabled));
            OnPropertyChanged(nameof(ScanProgress));
        };

        OnPropertyChanged("");
    }

    public void OnPropertyChanged(string prop)
        => PropertyChanged?.Invoke(this, new(prop));
}
