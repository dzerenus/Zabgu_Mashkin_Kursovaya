namespace UDPFlood.Flooder.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using UDPFlood.Ethernet.FloodSettings;

public class MWindowVM : INotifyPropertyChanged
{
    public string AttackStatus
    {
        get => _attackStatus;
        set
        {
            _attackStatus = value;
            OnPropertyChanged(nameof(AttackStatus));
        }
    }
    public string ThreadCount 
    {
        get => _threadCount.ToString();
        set 
        {
            var input = value;

            if (input.Length > 1)
                input = input[^1].ToString();

            if (int.TryParse(input, out var threadCount) && threadCount > 0 && threadCount < 10)
            {
                _threadCount = threadCount;
                OnPropertyChanged(nameof(ThreadCount));
            }    
        }
    }
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
    public int PrevSecondPacketCount
    {
        get => _prevSecondPacketCount;
        set
        {
            _prevSecondPacketCount = value;
            OnPropertyChanged(nameof(PrevSecondPacketCount));
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
    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            var validated = ValidateIpAddress(value);

            if (validated != null)
            {
                _ipAddress = validated;
                OnPropertyChanged(nameof(IpAddress));

                if (IsValidIpv4(value))
                {
                    IsAttackButtonEnabled = true;
                }

                else
                {
                    IsAttackButtonEnabled = false;
                }
            }
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

    public ICommand AddNewThreadCommand { get; }
    public ICommand StartOrStopAttack { get; }

    public ObservableCollection<ThreadSettingsViewModel> ThreadsCollection { get; } = new ();

    private string _attackStatus = "ОТКЛЮЧЕНА";
    private string _attackButtonText = "💣 Начать атаку";
    private bool _isAttackButtonEnabled = false;
    private bool _isInputEnabled = true;
    private int _threadCount = 9;
    private string _ipAddress = "192.168.0.90";

    private int _prevSecondPacketCount = 0;
    private int _secondPacketCount = 0;
    private long _currentSecond = 0;

    private int _dataGridItemSelectedIndex = -1;

    private readonly List<ThreadSettings> _threads = new List<ThreadSettings>();

    private UDPFlooder? _flooder;

    public MWindowVM()
    {
        AddNewThreadCommand = new RelayCommand(_ =>
        {
            var addWindow = new AddThreadWindow();
            var context = addWindow.DataContext as AddThreadViewModel;

            if (context == null)
                throw new NullReferenceException("Invalid AddThreadWindow DataContext");

            context.AddThread += (ThreadSettings thread) =>
            {
                _threads.Add(thread);
                
                ThreadsCollection.Clear();
                _threads.ForEach(x => ThreadsCollection.Add(new(x)));

                addWindow.Close();
            };

            addWindow.ShowDialog();
        });

        StartOrStopAttack = new RelayCommand(x =>
        {
            if (_flooder != null)
            {
                _flooder.Stop();
                _flooder = null;

                PrevSecondPacketCount = 0;
                _currentSecond = 0;
                _secondPacketCount = 0;

                IsInputEnabled = true;
                AttackButtonText = "💣 Начать атаку";
                AttackStatus = "ОТКЛЮЧЕНА";
            }

            else
            {
                IsInputEnabled = false;
                AttackButtonText = "🚫 Остановить атаку";
                AttackStatus = "ВКЛЮЧЕНА";

                _flooder = new UDPFlooder(_ipAddress, _threadCount);

                _flooder.PacketSended += (s, port) =>
                {
                    var second = DateTimeOffset.Now.ToUnixTimeSeconds();

                    if (_currentSecond == second)
                        _secondPacketCount++;

                    else
                    {
                        _currentSecond = second;
                        PrevSecondPacketCount = _secondPacketCount;
                        _secondPacketCount = 0;
                    }
                };

                _flooder.Start();
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
