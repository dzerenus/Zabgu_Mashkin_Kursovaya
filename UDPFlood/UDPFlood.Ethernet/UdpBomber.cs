using PcapDotNet.Core;

namespace UDPFlood.Ethernet;

public class UdpBomber
{
    public string SelectedDevice { get; private set; }
    public IReadOnlyList<string> DeviceNames { get; }

    private LivePacketDevice _selectedDevice;
    private readonly IReadOnlyList<LivePacketDevice> _devices;

    public UdpBomber()
    {
        _devices = LivePacketDevice.AllLocalMachine;
        DeviceNames = _devices.Select(GetDeviceName).ToList();

        if (_devices.Count == 0)
            throw new InvalidProgramException("Убедитесь, что у вас установлен WinPcap.");

        var realtekDevice = _devices.Where(x => x.Description.ToLower().Contains("realtek")).FirstOrDefault();
        _selectedDevice = realtekDevice ?? _devices[0];

        SelectedDevice = GetDeviceName(_selectedDevice);
    }

    public void SelectDevice(string deviceName)
    {
        var device = _devices.Where(x => x.Description.Contains(deviceName)).FirstOrDefault();
        
        if (device == null)
            throw new InvalidOperationException("Запрашиваемое устройство не найдено.");

        _selectedDevice = device;
        SelectedDevice = GetDeviceName(_selectedDevice);
    }

    private string GetDeviceName(LivePacketDevice device)
        => device.Description.Split("'")[1];
}
