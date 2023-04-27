namespace UDPFlood.Flooder.ViewModels;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

public class MWindowVM : INotifyPropertyChanged
{
    public string AttackButtonText { get => _attackButtonText; }
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
            }
        }
    }
    public ObservableCollection<PortInfo> OpenedPorts { get; set; } = new();

    public ICommand CheckPorts { get; }
    public ICommand StartOrStopAttack { get; }

    private string _attackButtonText = "Начать атаку";
    private string _ipAddress = string.Empty;

    public MWindowVM()
    {
        StartOrStopAttack = new RelayCommand(x =>
        {
            var flooder = new UDPFlooder(IpAddress);
            flooder.PacketSended += (s, port) =>
            {
                var list = OpenedPorts.ToList();
                var portInfo = list.Find(x => x.Port == port);

                if (portInfo == null)
                {
                    OpenedPorts.Add(new PortInfo(port));
                }

                else
                {
                    portInfo.Packets++;
                }
            };

            flooder.Start();
        });
    }

    private static string? ValidateIpAddress(string input)
    {
        var trimmed = input.Trim();
        var groups = trimmed.Split('.');

        if (groups.Length > 4)
            return null;

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
