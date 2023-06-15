using System;
using System.Linq;
using System.Collections.Generic;
using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using System.Net;

namespace UDPFlood.Analyzer.Models;

public delegate void UdpAnalyzerEventArgs();

public class UdpAnalyzer
{
    public bool IsRunning => _thread != null;
    public string SelectedDeviceName => GetDeviceName(_selectedDevice);
    public IReadOnlyList<UdpPacket> Packets => _packets;
    public IReadOnlyList<string> DeviceNames { get; }

    public int TotalPacketCount { get; private set; }
    public Dictionary<IPAddress, int> PacketCountByIp { get; private set; } = new();
    public List<UdpPacket> MinutePackets { get; private set; } = new();

    private int _currentMinute = 0;
    private Dictionary<IPAddress, int> _currentMinutePackets = new();
    private int _currentTotalCount = 0;
    private List<UdpPacket> _minutePackets = new();

    private readonly IReadOnlyList<LivePacketDevice> _devices;
    private readonly List<UdpPacket> _packets = new();
    private LivePacketDevice _selectedDevice;
    private UdpAnalyzerThread? _thread;

    public UdpAnalyzer()
    {
        _devices = GetDeviceList();
        DeviceNames = _devices.Select(GetDeviceName).ToList();

        var realtekDevice = _devices.Where(x => x.Description.ToLower().Contains("realtek")).FirstOrDefault();
        _selectedDevice = realtekDevice ?? _devices[0];
    }

    public event UdpAnalyzerEventArgs? OnPacketsChanged;
    public event UdpAnalyzerEventArgs? OnEveryMinute;

    public void Start()
    {
        Clear();

        _thread = new(_selectedDevice);

        _thread.OnNewPacket += packet =>
        {
            _packets.Add(packet);
            OnPacketsChanged?.Invoke();

            if (_currentMinute != DateTimeOffset.Now.Minute)
            {
                _currentMinute = DateTimeOffset.Now.Minute;
                PacketCountByIp = _currentMinutePackets;
                TotalPacketCount = _currentTotalCount;
                _currentMinutePackets = new();
                _currentTotalCount = 0;
                MinutePackets = _minutePackets;
                _minutePackets = new();

                OnEveryMinute?.Invoke();
            }

            _minutePackets.Add(packet);
            _currentTotalCount++;

            var ip = IPAddress.Parse(packet.IpDestination.ToString());

            if (_currentMinutePackets.ContainsKey(ip))
                _currentMinutePackets[ip]++;
            else
                _currentMinutePackets.Add(ip, 1);
        };

        _thread.Start();
    }

    public void Stop()
    {
        _thread?.Stop();
        _thread = null;
    }

    public void Clear()
    {
        _packets.Clear();
        OnPacketsChanged?.Invoke();
    }

    public void SelectDevice(string deviceName)
    {
        var device = _devices.Where(x => x.Description.Contains(deviceName)).FirstOrDefault() 
            ?? throw new InvalidOperationException();

        _selectedDevice = device;
    }

    private static string GetDeviceName(LivePacketDevice device)
        => device.Description.Split("'")[1];

    private static IReadOnlyList<LivePacketDevice> GetDeviceList()
    {
        var result = new List<LivePacketDevice>();
        var devices = LivePacketDevice.AllLocalMachine.Where(x => x.GetNetworkInterface() != null);
        var unicastDevices = devices.Where(x => x.GetNetworkInterface().GetIPProperties().UnicastAddresses.Count > 0);

        foreach (var device in unicastDevices)
        {
            if (device.Addresses.Where(x => x.Address.Family == SocketAddressFamily.Internet).Any()
                && device.GetNetworkInterface().GetIPProperties().GatewayAddresses.Count > 0)
                result.Add(device);
        }

        return result;
    }
}
