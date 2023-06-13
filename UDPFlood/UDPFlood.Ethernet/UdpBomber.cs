using System.Net;
using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using UDPFlood.Ethernet.FloodSettings;

namespace UDPFlood.Ethernet;

public delegate void BomberEventArgs();
public delegate void BomberThreadsEventArgs(IEnumerable<BomberThreadInfo> threads);

public class UdpBomber
{
    public event BomberEventArgs? OnSourceIpChanged;
    public event BomberEventArgs? OnSelectedDeviceChanged;
    public event BomberThreadsEventArgs? OnThreadsChanged;

    public bool IsBomberWork => _threads.All(x => x.IsFlooderWork);

    public string SelectedDeviceName { get => GetDeviceName(_selectedDevice); }
    public IReadOnlyList<string> DeviceNames { get => _devices.Select(GetDeviceName).ToList(); }
    public IPAddress DeviceSourceAddress { get; private set; }
    public Scanner NetworkScanner { get => _networkScanner; }

    private Scanner _networkScanner;
    private LivePacketDevice _selectedDevice;
    private readonly IReadOnlyList<LivePacketDevice> _devices;
    private readonly List<BomberThread> _threads;

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

    public void RemoveThread(Guid threadId)
    {
        var thread = _threads.Where(x => x.ThreadId == threadId).FirstOrDefault();

        if (thread == null)
            throw new InvalidOperationException();

        if (thread.IsFlooderWork)
            thread.Stop();

        _threads.Remove(thread);

        OnThreadsChanged?.Invoke(GenerateThreadInfo());
    }

    public void AddThread(BomberThreadSettings settings)
    {
        var thread = new BomberThread(settings);

        thread.OnThreadChanged += () => OnThreadsChanged?.Invoke(GenerateThreadInfo());

        _threads.Add(thread);
        OnThreadsChanged?.Invoke(GenerateThreadInfo());
    }

    public void Start()
    {
        _threads.ForEach(x => new Thread(() => x.Start(_selectedDevice)).Start());
    }

    public void Stop()
    {
        _threads.ForEach(x => x.Stop());
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

    private IEnumerable<BomberThreadInfo> GenerateThreadInfo()
    {
        var threadsInfo = new List<BomberThreadInfo>();

        foreach (var thread in _threads)
        {
            var threadInfo = new BomberThreadInfo(thread.ThreadId, thread.Status, thread.LastPacketSendedAt, thread.Settings, thread.PacketSendedCount);
            threadsInfo.Add(threadInfo);
        }

        return threadsInfo;
    }
}
