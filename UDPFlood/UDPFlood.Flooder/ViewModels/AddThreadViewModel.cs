using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using UDPFlood.Ethernet.FloodSettings;

namespace UDPFlood.Flooder.ViewModels;

public class AddThreadViewModel : INotifyPropertyChanged
{
    public delegate void AddThreadEventArgs(ThreadSettings thread);

    public event AddThreadEventArgs? AddThread;

    public string IPSourceAddressesTextboxText
    {
        get => _iPSourceAddressesTextboxText;
        set
        {
            _iPSourceAddressesTextboxText = value;
            OnPropertyChanged(nameof(IPSourceAddressesTextboxText));
            ValidateInput();
        }
    }
    public bool IPSourceAddressesTextboxEnabled
    {
        get => _iPSourceAddressesTextboxEnabled;
        set
        {
            _iPSourceAddressesTextboxEnabled = value;
            OnPropertyChanged(nameof(IPSourceAddressesTextboxEnabled));
        }
    }
    public bool IPSourceIsFromTextBox
    {
        get => _iPSourceIsFromTextBox;
        set
        {
            IPSourceAddressesTextboxEnabled = value;
            _iPSourceIsFromTextBox = value;
            OnPropertyChanged(nameof(IPSourceIsFromTextBox));
            ValidateInput();
        }
    }
    public bool IPSourceIsRandom
    {
        get => _iPSourceIsRandom;
        set
        {
            _iPSourceIsRandom = value;
            OnPropertyChanged(nameof(IPSourceIsRandom));
        }
    }
    public bool IPSourceIsMine
    {
        get => _iPSourceIsMine;
        set
        {
            _iPSourceIsMine = value;
            OnPropertyChanged(nameof(IPSourceIsMine));
        }
    }

    public string MACSourceAddressesTextboxText
    {
        get => _macSourceAddressesTextboxText;
        set
        {
            _macSourceAddressesTextboxText = value;
            OnPropertyChanged(nameof(MACSourceAddressesTextboxText));
            ValidateInput();
        }
    }
    public bool MACSourceAddressesTextboxEnabled
    {
        get => _macSourceAddressesTextboxEnabled;
        set
        {
            _macSourceAddressesTextboxEnabled = value;
            OnPropertyChanged(nameof(MACSourceAddressesTextboxEnabled));
        }
    }
    public bool MACSourceIsFromTextBox
    {
        get => _macSourceIsFromTextBox;
        set
        {
            MACSourceAddressesTextboxEnabled = value;
            _macSourceIsFromTextBox = value;
            OnPropertyChanged(nameof(MACSourceIsFromTextBox));
            ValidateInput();
        }
    }
    public bool MACSourceIsRandom
    {
        get => _macSourceIsRandom;
        set
        {
            _macSourceIsRandom = value;
            OnPropertyChanged(nameof(MACSourceIsRandom));
        }
    }
    public bool MACSourceIsMine
    {
        get => _macSourceIsMine;
        set
        {
            _macSourceIsMine = value;
            OnPropertyChanged(nameof(MACSourceIsMine));
        }
    }

    public string PortsDistinationTextboxText
    {
        get => _portsDistinationTextboxText;
        set
        {
            _portsDistinationTextboxText = value;
            OnPropertyChanged(nameof(PortsDistinationTextboxText));
            ValidateInput();
        }
    }
    public bool PortsDistinationTextboxEnabled
    {
        get => _portsDistinationTextboxEnabled;
        set
        {
            _portsDistinationTextboxEnabled = value;
            OnPropertyChanged(nameof(PortsDistinationTextboxEnabled));
        }
    }
    public bool PortsDistinationIsFromTextBox
    {
        get => _portDistinationIsFromTextBox;
        set
        {
            PortsDistinationTextboxEnabled = value;
            _portDistinationIsFromTextBox = value;
            OnPropertyChanged(nameof(PortsDistinationIsFromTextBox));
            ValidateInput();
        }
    }
    public bool PortsDistinationIsRandom
    {
        get => _portDistinationIsRandom;
        set
        {
            _portDistinationIsRandom = value;
            OnPropertyChanged(nameof(PortsDistinationIsRandom));
        }
    }

    public bool TimeIsDelayEnabled
    {
        get => _timeIsDelayEnabled;
        set
        {
            _timeIsDelayEnabled = value;
            OnPropertyChanged(nameof(TimeIsDelayEnabled));
            ValidateInput();
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
            ValidateInput();
        }
    }
    public string TimeLengthTextboxText
    {
        get => _timeLengthTextboxText;
        set
        {
            _timeLengthTextboxText = value;
            OnPropertyChanged(nameof(TimeLengthTextboxText));
            ValidateInput();
        }
    }
    public string TimeDelayTextboxText
    {
        get => _timeDelayTextboxText;
        set
        {
            _timeDelayTextboxText = value;
            OnPropertyChanged(nameof(TimeDelayTextboxText));
            ValidateInput();
        }
    }

    public bool ContentIsEmptyMessage
    {
        get => _contentIsEmptyMessage;
        set
        {
            if (value)
            {
                ContentIsRandomMessage = false;
                ContentIsTextBoxMessage = false;
            }

            _contentIsEmptyMessage = value;
            OnPropertyChanged(nameof(ContentIsEmptyMessage));
        }
    }
    public bool ContentIsRandomMessage
    {
        get => _contentIsRandomMessage;
        set
        {
            if (value)
            {
                ContentIsEmptyMessage = false;
                ContentIsTextBoxMessage = false;
            }

            _contentIsRandomMessage = value;
            OnPropertyChanged(nameof(ContentIsRandomMessage));
        }
    }
    public bool ContentIsTextBoxMessage
    {
        get => _contentIsTextboxMessage;
        set
        {
            if (value)
            {
                ContentIsRandomMessage = false;
                ContentIsEmptyMessage = false;
            }

            _contentIsTextboxMessage = value;
            OnPropertyChanged(nameof(ContentIsTextBoxMessage));
            ValidateInput();
        }
    }
    public string ContentTextBoxText
    {
        get => _contentTextboxText;
        set
        {
            _contentTextboxText = value;
            OnPropertyChanged(nameof(ContentTextBoxText));
            ValidateInput();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            ErrorMessageVisibility = value == string.Empty ? Visibility.Hidden : Visibility.Visible;
            IsInputValid = value == string.Empty;

            OnPropertyChanged(nameof(ErrorMessage));
            OnPropertyChanged(nameof(ErrorMessageVisibility));
            OnPropertyChanged(nameof(IsInputValid));
        }
    }
    public Visibility ErrorMessageVisibility { get; set; } = Visibility.Hidden;
    public bool IsInputValid { get; set; }

    public ICommand AddThreadCommand { get; private set; }

    private string _errorMessage = string.Empty;

    private bool _contentIsEmptyMessage = true;
    private bool _contentIsRandomMessage = true;
    private bool _contentIsTextboxMessage = true;
    private string _contentTextboxText = string.Empty;

    private bool _timeIsDelayEnabled = false;
    private string _timeEverySecondsTextboxText = "60";
    private string _timeLengthTextboxText = "15";
    private string _timeDelayTextboxText = "5";

    private string _portsDistinationTextboxText = string.Empty;
    private bool _portsDistinationTextboxEnabled = false;
    private bool _portDistinationIsFromTextBox = false;
    private bool _portDistinationIsRandom = true;

    private string _macSourceAddressesTextboxText = string.Empty;
    private bool _macSourceAddressesTextboxEnabled = false;
    private bool _macSourceIsFromTextBox = true;
    private bool _macSourceIsRandom = true;
    private bool _macSourceIsMine = true;

    private string _iPSourceAddressesTextboxText = string.Empty;
    private bool _iPSourceAddressesTextboxEnabled = false;
    private bool _iPSourceIsFromTextBox = true;
    private bool _iPSourceIsRandom = true;
    private bool _iPSourceIsMine = true;

    public AddThreadViewModel()
    {
        AddThreadCommand = new RelayCommand(x => DoAddThread());
    }

    private void DoAddThread()
    {
        if (!IsInputValid)
            throw new InvalidOperationException();

        var ipSettings = IPSourceIsMine ? new IpSettings(IpMode.MySelf)
            : IPSourceIsRandom ? new IpSettings(IpMode.Random)
            : IPSourceIsFromTextBox ? new IpSettings(IpMode.FromList, IPSourceAddressesTextboxText.Split(";"))
            : throw new InvalidOperationException();


        var macSettings = MACSourceIsMine ? new MacSettings(MacMode.MySelf)
            : MACSourceIsRandom ? new MacSettings(MacMode.Random)
            : MACSourceIsFromTextBox ? new MacSettings(MacMode.FromList, MACSourceAddressesTextboxText.Split(";"))
            : throw new InvalidOperationException();

        var portSettings = PortsDistinationIsRandom ? new PortSettings(PortMode.Random)
            : PortsDistinationIsFromTextBox ? new PortSettings(PortMode.FromList, PortsDistinationTextboxText.Split(";").Select(int.Parse))
            : throw new InvalidOperationException();

        var delaySettings = TimeIsDelayEnabled 
            ? new DelaySettings(true, int.Parse(TimeEverySecondsTextboxText), int.Parse(TimeLengthTextboxText), int.Parse(TimeDelayTextboxText)) 
            : new DelaySettings(false, 0, 0, 0);

        var contentSettings = ContentIsEmptyMessage ? new ContentSettings(ContentMode.Empty)
            : ContentIsRandomMessage ? new ContentSettings(ContentMode.Random)
            : ContentIsTextBoxMessage ? new ContentSettings(ContentMode.FromContent, ContentTextBoxText)
            : throw new InvalidOperationException();

        AddThread?.Invoke(new(contentSettings, delaySettings, ipSettings, portSettings, macSettings));
    }

    private void ValidateInput()
    {
        if (IPSourceIsFromTextBox)
        {
            var parts = IPSourceAddressesTextboxText.Split(";");

            foreach (var part in parts)
                if (!Ethernet.Validate.IsValidIP(part))
                {
                    ErrorMessage = "Неверно заданы IP источника!";
                    return;
                }
        }

        if (MACSourceIsFromTextBox)
        {
            var parts = MACSourceAddressesTextboxText.Split(";");

            foreach (var part in parts)
                if (!Ethernet.Validate.IsValidMAC(part))
                {
                    ErrorMessage = "Неверно заданы MAC источника!";
                    return;
                }
        }

        if (PortsDistinationIsFromTextBox)
        {
            var parts = PortsDistinationTextboxText.Split(";");

            foreach (var part in parts)
                if (!Ethernet.Validate.IsValidPort(part))
                {
                    ErrorMessage = "Неверно заданы порты назначения!";
                    return;
                }
        }

        if (TimeIsDelayEnabled)
        {
            if (!int.TryParse(TimeEverySecondsTextboxText, out var everySecond) || everySecond < 0
                || !int.TryParse(TimeLengthTextboxText, out var timeLength) || timeLength < 0 || timeLength > everySecond
                || !int.TryParse(TimeDelayTextboxText, out var timeDelay) || timeDelay < 0 || timeDelay >= timeLength)
            {
                ErrorMessage = "Время задано неверно!";
                return;
            }
        }

        ErrorMessage = string.Empty;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string prop)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
