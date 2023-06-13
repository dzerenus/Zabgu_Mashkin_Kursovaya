namespace UDPFlood.Flooder.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using UDPFlood.Ethernet;
using UDPFlood.Ethernet.FloodSettings;

public class MWindowVM : INotifyPropertyChanged
{
    public bool IsInputEnabled
    {
        get => _isInputEnabled;
        set
        {
            _isInputEnabled = value;
            OnPropertyChanged(nameof(IsInputEnabled));
        }
    }
    public bool IsAttackButtonEnabled 
    { 
        get => _isAttackButtonEnabled; 
        set
        {
            _isAttackButtonEnabled = value;
            OnPropertyChanged(nameof(IsAttackButtonEnabled));
        }
    }
    public string AttackButtonText 
    { 
        get => _attackButtonText;
        set
        {
            _attackButtonText = value;
            OnPropertyChanged(nameof(AttackButtonText));
        }
    }
    public int DataGridItemSelectedIndex 
    { 
        get => _dataGridItemSelectedIndex;
        set
        {
            _dataGridItemSelectedIndex = -1;
            OnPropertyChanged(nameof(DataGridItemSelectedIndex));
        }
    }
    
    public ICommand ClearThreadsCommand { get; }
    public ICommand AddNewThreadCommand { get; }
    public ICommand StartOrStopAttack { get; }
    public ICommand OpenScanWindowCommand { get; }

    public ObservableCollection<ThreadInfoViewModel> ThreadsCollection { get; set; } = new ();

    public string BomberSelectedDevice
    {
        get => Bomber.SelectedDeviceName;
        set
        {
            Bomber.SelectDevice(value);
            OnPropertyChanged(BomberSelectedDevice);
        }
    }
    public UdpBomber Bomber { get; }

    private string _attackStatus = "ОТКЛЮЧЕНА";
    private string _attackButtonText = "💣 Начать атаку";
    private bool _isAttackButtonEnabled = true;
    private bool _isInputEnabled = true;
    private int _threadCount = 9;
    private string _ipAddress = "192.168.0.90";

    private int _prevSecondPacketCount = 0;
    private int _secondPacketCount = 0;
    private long _currentSecond = 0;

    private int _dataGridItemSelectedIndex = -1;

    private readonly List<BomberThreadSettings> _threads = new List<BomberThreadSettings>();

    public MWindowVM()
    {
        Bomber = new();
        Bomber.OnSelectedDeviceChanged += () => OnPropertyChanged(nameof(BomberSelectedDevice));

        Bomber.OnThreadsChanged += threadsInfo =>
        {
            var threadInfoViewModels = threadsInfo.Select(x => new ThreadInfoViewModel(x)).ToList();
            ThreadsCollection = new ObservableCollection<ThreadInfoViewModel>(threadInfoViewModels);
            OnPropertyChanged(nameof(ThreadsCollection));
        };

        OpenScanWindowCommand = new RelayCommand(_ =>
        {
            var scanWindow = new NetworkScan();
            var scanVm = scanWindow.DataContext as ScanWindowVM;
            scanVm?.SetBomber(Bomber);
            scanWindow.ShowDialog();
        });

        AddNewThreadCommand = new RelayCommand(_ =>
        {
            var addWindow = new AddThreadWindow();

            if (addWindow.DataContext is not AddThreadViewModel context)
                throw new NullReferenceException("Invalid AddThreadWindow DataContext");

            context.AddThread += (BomberThreadSettings threadSettings) =>
            {
                Bomber.AddThread(threadSettings);
                addWindow.Close();
            };

            addWindow.ShowDialog();
        });

        ClearThreadsCommand = new RelayCommand(_ =>
        {
            _threads.Clear();
            ThreadsCollection.Clear();
        });

        StartOrStopAttack = new RelayCommand(x =>
        {
            if (Bomber.IsBomberWork)
            {
                Bomber.Stop();

                _currentSecond = 0;
                _secondPacketCount = 0;

                IsInputEnabled = true;
                AttackButtonText = "💣 Начать атаку";
            }

            else
            {
                IsInputEnabled = false;
                AttackButtonText = "🚫 Остановить атаку";

                Bomber.Start();
            }
        });
    }

    private static bool IsValidIpv4(string input)
    {
        var groups = input.Split('.');

        if (groups.Length != 4)
            return false;

        foreach (var group in groups)
        {
            if (!int.TryParse(group, out var groupInteger))
                return false;

            if (groupInteger < 0 || groupInteger > 255)
                return false;
        }

        return true;
    }

    private static string? ValidateIpAddress(string input)
    {
        var trimmed = input.Trim();
        var groups = trimmed.Split('.');

        if (groups.Length > 4)
            return null;

        if (groups.Length == 1 && groups[0].Length == 0)
            return "";

       if (trimmed[^1] == '.')
            groups[^1] = "0";

        foreach (var group in groups)
        {
            if (!int.TryParse(group, out var groupInteger))
                return null;

            if (groupInteger < 0 || groupInteger > 255) 
                return null;
        }

        return trimmed;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
