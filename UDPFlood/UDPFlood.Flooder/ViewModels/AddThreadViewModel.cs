using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UDPFlood.Flooder.ViewModels;

public class AddThreadViewModel : INotifyPropertyChanged
{
    public string IPSourceAddressesTextboxText 
    {
        get => _iPSourceAddressesTextboxText; 
        set
        {
            _iPSourceAddressesTextboxText = value;
            OnPropertyChanged(nameof(IPSourceAddressesTextboxText));
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

    public bool ContentIsEmptyMessage
    {
        get => _contentIsEmptyMessage;
        set
        {
            _contentIsEmptyMessage = value;
            OnPropertyChanged(nameof(ContentIsEmptyMessage));
        }
    }

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

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
