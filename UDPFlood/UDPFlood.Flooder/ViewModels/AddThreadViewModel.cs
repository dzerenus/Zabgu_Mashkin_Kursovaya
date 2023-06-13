using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using UDPFlood.Ethernet.FloodSettings;

namespace UDPFlood.Flooder.ViewModels;

public class AddThreadViewModel : INotifyPropertyChanged
{
    public delegate void AddThreadEventArgs(BomberThreadSettings thread);

    public event AddThreadEventArgs? AddThread;

    public AddThreadInput DestinationIpField { get; }
    public AddThreadInput SourceIpField { get; }
    public AddThreadInput DestinationMacField { get; }
    public AddThreadInput SourceMacField { get; }
    public AddThreadInput DestinationPortField { get; }
    public AddThreadInput SourcePortField { get; }
    public AddThreadInput ContentField { get; }

    public bool TimeIsDelayEnabled
    {
        get => _timeIsDelayEnabled;
        set
        {
            _timeIsDelayEnabled = value;
            OnPropertyChanged(nameof(TimeIsDelayEnabled));
        }
    }
    public bool TimeIsDelayDisabled
    {
        get => !_timeIsDelayEnabled;
        set
        {
            _timeIsDelayEnabled = !value;
            OnPropertyChanged(nameof(TimeIsDelayDisabled));
        }
    }
    public string TimeEverySecondsTextboxText
    {
        get => _timeEverySecondsTextboxText;
        set
        {
            _timeEverySecondsTextboxText = value;
            OnPropertyChanged(nameof(TimeEverySecondsTextboxText));
        }
    }
    public string TimeLengthTextboxText
    {
        get => _timeLengthTextboxText;
        set
        {
            _timeLengthTextboxText = value;
            OnPropertyChanged(nameof(TimeLengthTextboxText));
        }
    }
    public string TimeDelayTextboxText
    {
        get => _timeDelayTextboxText;
        set
        {
            _timeDelayTextboxText = value;
            OnPropertyChanged(nameof(TimeDelayTextboxText));
        }
    }
    public string Ttl
    {
        get => _ttl.ToString();
        set
        {
            if (!int.TryParse(value, out var ttl) || ttl < 0 || ttl > 255)
                return;
            
            _ttl = ttl;
            OnPropertyChanged(nameof(Ttl));
        }
    }

    public ICommand AddThreadCommand { get; private set; }

    private bool _timeIsDelayEnabled = false;
    private string _timeEverySecondsTextboxText = "60";
    private string _timeLengthTextboxText = "15";
    private string _timeDelayTextboxText = "5";
    private int _ttl = 100;

    public AddThreadViewModel()
    {
        var isIpInputValid = new Predicate<string>(x => x.Split(';').All(s => Ethernet.Validate.IsValidIP(s) && s.Length > 0));
        var isMacInputValid = new Predicate<string>(x => x.Split(';').All(s => Ethernet.Validate.IsValidMAC(s) && s.Length > 0));
        var isPortInputValid = new Predicate<string>(x => x.Split(';').All(x => ushort.TryParse(x, out _)));
        var isContentValid = new Predicate<string>(x => Encoding.UTF8.GetBytes(x).Length <= 508);

        var destIpComboBoxItems = new List<ComboBoxItem>() 
        {
            new("Случайный для каждого пакета", (int)IpMode.Random),
            new("Случайный из списка", (int)IpMode.FromList)
        };

        var srcIpComboBoxItems = new List<ComboBoxItem>()
        {
            new("Случайный для каждого пакета", (int)IpMode.Random),
            new("Использовать адрес сетевой карты", (int)IpMode.MySelf),
            new("Случайный из списка", (int)IpMode.FromList)
        };

        var destMacComboBoxItems = new List<ComboBoxItem>()
        {
            new("Случайный для каждого пакета", (int)MacMode.Random),
            new("Случайный из списка", (int)MacMode.FromList)
        };

        var srcMacComboBoxItems = new List<ComboBoxItem>()
        {
            new("Случайный для каждого пакета", (int)MacMode.Random),
            new("Использовать адрес сетевой карты", (int)MacMode.MySelf),
            new("Случайный из списка", (int)MacMode.FromList)
        };

        var portComboBoxItems = new List<ComboBoxItem>()
        {
            new("Случайный для каждого пакета", (int)PortMode.Random),
            new("Случайный из списка", (int)PortMode.FromList)
        };

        var contentComboBoxItems = new List<ComboBoxItem>()
        {
            new("Без данных", (int)ContentMode.Empty),
            new("Случайные данные", (int)ContentMode.Random),
            new("Ввести данные (UTF-8)", (int)ContentMode.FromContent)
        };

        DestinationIpField = new(destIpComboBoxItems, destIpComboBoxItems[^1], isIpInputValid);
        SourceIpField = new(srcIpComboBoxItems, srcIpComboBoxItems[^1], isIpInputValid);
        DestinationMacField = new(destMacComboBoxItems, destIpComboBoxItems[^1], isMacInputValid);
        SourceMacField = new(srcMacComboBoxItems, destIpComboBoxItems[^1], isMacInputValid);
        DestinationPortField = new(portComboBoxItems, portComboBoxItems[^1], isPortInputValid);
        SourcePortField = new(portComboBoxItems, portComboBoxItems[^1], isPortInputValid);
        ContentField = new(contentComboBoxItems, contentComboBoxItems[^1], isContentValid);

        AddThreadCommand = new RelayCommand(x => DoAddThread());
    }

    private void DoAddThread()
    {
        if (SourceIpField.IsError)
        {
            MessageBox.Show("IP источника задан неверно", "Ошибка!");
            return;
        }

        if (DestinationIpField.IsError)
        {
            MessageBox.Show("IP назначения задан неверно", "Ошибка!");
            return;
        }

        if (SourceMacField.IsError)
        {
            MessageBox.Show("MAC источника задан неверно", "Ошибка!");
            return;
        }

        if (DestinationMacField.IsError)
        {
            MessageBox.Show("MAC назначения задан неверно", "Ошибка!");
            return;
        }

        if (SourcePortField.IsError)
        {
            MessageBox.Show("Порт источника задан неверно", "Ошибка!");
            return;
        }

        if (DestinationPortField.IsError)
        {
            MessageBox.Show("Порт назначения задан неверно", "Ошибка!");
            return;
        }

        if (ContentField.IsError)
        {
            MessageBox.Show("Содержимое пакета задано неверно", "Ошибка!");
            return;
        }

        if (TimeIsDelayEnabled
            && (!int.TryParse(TimeDelayTextboxText, out int delay)
            || !int.TryParse(TimeEverySecondsTextboxText, out int every)
            || !int.TryParse(TimeLengthTextboxText, out var length)))
        {
            MessageBox.Show("Время задано неверно", "Ошибка!");
            return;
        }

        var srcIpMode = (IpMode)SourceIpField.SelectedItem.Value;
        var dstIpMode = (IpMode)DestinationIpField.SelectedItem.Value;

        var srcMacMode = (MacMode)SourceMacField.SelectedItem.Value;
        var dstMacMode = (MacMode)DestinationMacField.SelectedItem.Value;

        var srcPortMode = (PortMode)SourcePortField.SelectedItem.Value;
        var dstPortMode = (PortMode)DestinationPortField.SelectedItem.Value;

        var contentMode = (ContentMode)ContentField.SelectedItem.Value;

        var srcIp = new IpSettings(srcIpMode, srcIpMode == IpMode.FromList ? SourceIpField.TextBoxValue.Split(';') : null);
        var dstIp = new IpSettings(dstIpMode, dstIpMode == IpMode.FromList ? DestinationIpField.TextBoxValue.Split(';') : null);

        var srcMac = new MacSettings(srcMacMode, srcMacMode == MacMode.FromList ? SourceMacField.TextBoxValue.Split(';') : null);
        var dstMac = new MacSettings(dstMacMode, dstMacMode == MacMode.FromList ? DestinationMacField.TextBoxValue.Split(';') : null);

        var srcPort = new PortSettings(srcPortMode, srcPortMode == PortMode.FromList ? SourcePortField.TextBoxValue.Split(';').Select(int.Parse) : null);
        var dstPort = new PortSettings(dstPortMode, dstPortMode == PortMode.FromList ? DestinationPortField.TextBoxValue.Split(';').Select(int.Parse) : null);

        var delaySettings = TimeIsDelayEnabled 
            ? new DelaySettings(true, int.Parse(TimeEverySecondsTextboxText), int.Parse(TimeLengthTextboxText), int.Parse(TimeDelayTextboxText)) 
            : new DelaySettings(false, 0, 0, 0);

        var contentSettings = new ContentSettings(contentMode, contentMode == ContentMode.FromContent ? ContentField.TextBoxValue : null);

        AddThread?.Invoke(new(contentSettings, delaySettings, srcIp, dstIp, srcMac, dstMac, srcPort, dstPort, (byte)_ttl));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string prop)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}

public class AddThreadInput : INotifyPropertyChanged
{
    public string TextBoxValue
    {
        get => _textboxValue;
        set
        {
            _textboxValue = value;
            IsError = !IsValidInput();
            OnPropertyChanged(nameof(TextBoxValue));
        }
    }
    public bool IsTextBoxEnabled
    {
        get => _isTextBoxEnabled;
        set
        {
            IsError = !IsValidInput();
            _isTextBoxEnabled = value;
            OnPropertyChanged(nameof(IsTextBoxEnabled));
        }
    }
    public bool IsError
    {
        get => _isError;
        private set
        {
            _isError = value;
            OnPropertyChanged(nameof(IsError));
        }
    }
    public ComboBoxItem SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            IsTextBoxEnabled = TextBoxEnabledWhen.Value == value.Value;
            OnPropertyChanged(nameof(SelectedItem));
            IsError = !IsValidInput();
        }
    }
    public ComboBoxItem TextBoxEnabledWhen { get; }
    public IEnumerable<ComboBoxItem> ComboboxItems { get; }

    private Predicate<string> _isInputValid;
    private ComboBoxItem _selectedItem;
    private string _textboxValue = string.Empty;
    private bool _isTextBoxEnabled;
    private bool _isError;

    public AddThreadInput(IEnumerable<ComboBoxItem> comboBoxItems, ComboBoxItem textBoxEnabledItem, Predicate<string> validator)
    {
        if (!comboBoxItems.Any())
            throw new NullReferenceException();

        ComboboxItems = comboBoxItems;
        TextBoxEnabledWhen = textBoxEnabledItem;
        _selectedItem = comboBoxItems.First();
        _isInputValid = validator;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged(string prop)
    {
        PropertyChanged?.Invoke(this, new(prop));
    }

    private bool IsValidInput()
    {
        if (!IsTextBoxEnabled)
            return true;

        return _isInputValid(TextBoxValue);
    }
}

public class ComboBoxItem
{
    public string Text { get; }
    public int Value;

    public ComboBoxItem(string text, int value)
    {
        Text = text;
        Value = value;
    }

    public override string ToString() => Text;
}
