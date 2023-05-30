using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using System.Linq;
using System.Net;
using UDPFlood.Ethernet.FloodSettings;

namespace UDPFlood.Ethernet;

public delegate void BomberEventArgs();

public class UdpBomber
{
    public event BomberEventArgs? OnSelectedDeviceChanged;
    public event BomberEventArgs? OnThreadListChanged;
    public event BomberEventArgs? OnSourceIpChanged;

    public string SelectedDeviceName { get => GetDeviceName(_selectedDevice); }
    public IReadOnlyList<string> DeviceNames { get => _devices.Select(GetDeviceName).ToList(); }
    public IReadOnlyList<BomberThreadSettings> Threads { get => _threads; }
    public IPAddress DeviceSourceAddress { get; private set; }
    public Scanner NetworkScanner { get => _networkScanner; }

    private Scanner _networkScanner;
    private LivePacketDevice _selectedDevice;
    private readonly IReadOnlyList<LivePacketDevice> _devices;
    private readonly List<BomberThreadSettings> _threads;

    public UdpBomber()
    {
        _threads = new ();

        _devices = GetDeviceList();

        if (_devices.Count == 0)
            throw new InvalidProgramException("Убедитесь, что у вас установлен WinPcap.");

        var realtekDevice = _devices.Where(x => x.Description.ToLower().Contains("realtek")).FirstOrDefault();
        _selectedDevice = realtekDevice ?? _devices[0];

        DeviceSourceAddress = _selectedDevice.GetNetworkInterface().GetIPProperties().GatewayAddresses[0].Address;
        
        _networkScanner = new(_selectedDevice);
    }

    public void SelectDevice(string deviceName)
    {
        var device = _devices.Where(x => x.Description.Contains(deviceName)).FirstOrDefault();
        
        if (device == null)
            throw new InvalidOperationException("Запрашиваемое устройство не найдено.");

        _selectedDevice = device;
        NetworkScanner.SetDevice(_selectedDevice);
        OnSelectedDeviceChanged?.Invoke();

        DeviceSourceAddress = _selectedDevice.GetNetworkInterface().GetIPProperties().GatewayAddresses[0].Address;
        OnSourceIpChanged?.Invoke();
    }

    public void RemoveThread(BomberThreadSettings thread)
    {
        _threads.Remove(thread);
        OnThreadListChanged?.Invoke();
    }

    public void AddThread(BomberThreadSettings thread)
    {
        _threads.Add(thread);
        OnThreadListChanged?.Invoke();
    }

    public void Start()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    private static IReadOnlyList<LivePacketDevice> GetDeviceList()
    {
        var devices = LivePacketDevice.AllLocalMachine.Where(x => x.GetNetworkInterface() != null);
        var unicastDevices = devices.Where(x => x.GetNetworkInterface().GetIPProperties().UnicastAddresses.Count > 0);

        var result = new List<LivePacketDevice>();

        foreach (var device in unicastDevices)
        {
            if (device.Addresses.Where(x => x.Address.Family == SocketAddressFamily.Internet).Any()
                && device.GetNetworkInterface().GetIPProperties().GatewayAddresses.Count > 0)
                result.Add(device);
        }

        return result;
    }

    private static string GetDeviceName(LivePacketDevice device)
        => device.Description.Split("'")[1];
}
