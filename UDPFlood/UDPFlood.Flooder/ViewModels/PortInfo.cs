namespace UDPFlood.Flooder.ViewModels;

using System.ComponentModel;

public class PortInfo : INotifyPropertyChanged
{
    public int Port 
    {
        get => _port;
        set 
        {
            _port = value;
            OnPropertyChanged(nameof(Port));
        }
    }

    public int Packets
    {
        get => _packets;
        set
        {
            _packets = value;
            OnPropertyChanged(nameof(Packets));
        }
    }

    private int _port;
    private int _packets;

    public PortInfo(int port)
    {
        Port = port;
        Packets = 0;
    }

    public override string ToString() => $"{Port} [{Packets}]";

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
