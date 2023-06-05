using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UDPFlood.Ethernet;

public delegate void NetworkScannerEventArgs();
public delegate void NetworkScannerProgressEventArgs(int current);

public class Scanner
{
    public bool ScanInProgress { get => _inProcess; }
    public List<ArpRow> ArpTable { get; } = new();
    
    private bool _inProcess = false;
    private LivePacketDevice _device;
    
    public event NetworkScannerEventArgs? OnArpAdded;
    public event NetworkScannerEventArgs? OnScanEnded;
    public event NetworkScannerProgressEventArgs? OnArpProgressChanged;

    public Scanner(LivePacketDevice device)
    {
        _device = device;
    }

    public void SetDevice(LivePacketDevice device)
    {
        _device = device;
    }

    public void StopScan()
    {
        _inProcess = false;
    }

    public void StartScan()
    {
        ArpTable.Clear();
        _inProcess = true;

        var addresses = _device.GetNetworkInterface().GetIPProperties();
        var ip = addresses.UnicastAddresses.Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault()
            ?? throw new NullReferenceException();

        var sendThread = new Thread(() => SendArp(ip.Address.ToString()));
        var recvThread = new Thread(() => RecieveArp(ip.Address.ToString()));

        recvThread.Start();
        sendThread.Start();
    }

    private void SendArp(string srcIp)
    {
        var parts = srcIp.Split('.');

        for (int i = 0; i < 256 && _inProcess; i++)
        {
            SendArpRequest(parts[0] + '.' + parts[1] + '.' + parts[2] + '.' + i);
            OnArpProgressChanged?.Invoke(i);
        }

        _inProcess = false;
        OnScanEnded?.Invoke();
    }

    private void RecieveArp(string srcIp)
    {
        using var communicator = _device.Open(65535, PacketDeviceOpenAttributes.Promiscuous, 1000);
        communicator.SetFilter("arp");

        while (_inProcess)
        {
            communicator.ReceivePacket(out var packet);
            var arpLayer = packet?.Ethernet.Arp;

            if (arpLayer == null || arpLayer.TargetProtocolIpV4Address.ToString() != srcIp)
                continue;

            var ip = arpLayer.SenderProtocolIpV4Address;
            var mac = new MacAddress(BitConverter.ToString(arpLayer.SenderHardwareAddress.ToArray()).Replace("-", ":"));

            ArpTable.Add(new ArpRow(ip, mac));
            OnArpAdded?.Invoke();
        }
    }

    private void SendArpRequest(string targetIp)
    {
        using var communicator = _device.Open();

        var ethernetLayer = new EthernetLayer()
        {
            Source = _device.GetMacAddress(),
            Destination = new MacAddress("ff:ff:ff:ff:ff:ff"),
            EtherType = EthernetType.None
        };

        var addresses = _device.GetNetworkInterface().GetIPProperties();
        var ip = addresses.UnicastAddresses.Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault()
            ?? throw new NullReferenceException();

        var arpLayer = new ArpLayer()
        {
            ProtocolType = EthernetType.IpV4,
            Operation = ArpOperation.Request,
            TargetHardwareAddress = new byte[] { 0, 0, 0, 0, 0, 0 }.AsReadOnly(),
            SenderHardwareAddress = GetMacBytes().AsReadOnly(),
            TargetProtocolAddress = GetIpBytes(targetIp).AsReadOnly(),
            SenderProtocolAddress = GetIpBytes(ip.Address.ToString()).AsReadOnly(),
        };

        var builder = new PacketBuilder(ethernetLayer, arpLayer);
        communicator.SendPacket(builder.Build(DateTime.Now));
    }

    private byte[] GetIpBytes(string ip)
    {
        var bytes = new byte[4];
        var parts = ip.Split('.');

        for (int i = 0; i < parts.Length; i++)
            bytes[i] = Convert.ToByte(parts[i], 10);

        return bytes;
    }

    private byte[] GetMacBytes()
    {
        var bytes = new byte[6];

        var mac = _device.GetMacAddress();
        var parts = mac.ToString().Split(':');

        for (int i = 0; i < parts.Length; i++)
            bytes[i] = Convert.ToByte("0x" + parts[i], 16);

        return bytes;
    }
}

public class ArpRow
{
    public string IpString { get; }
    public string MacString { get; }

    internal IpV4Address Ip { get; }
    internal MacAddress Mac { get; }

    public ArpRow(IpV4Address ip, MacAddress mac)
    {
        Ip = ip;
        Mac = mac;

        IpString = ip.ToString();
        MacString = mac.ToString();
    }
}