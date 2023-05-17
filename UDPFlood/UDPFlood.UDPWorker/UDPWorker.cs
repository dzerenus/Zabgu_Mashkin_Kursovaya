using PcapDotNet.Core;
using System.Diagnostics;

namespace UDPFlood.UDPWorker;

public class UDPWorker
{
    public static void Test()
    {
        // Retrieve the device list from the local machine
        IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

        if (allDevices.Count == 0)
        {
            Debug.WriteLine("No interfaces found! Make sure WinPcap is installed.");
            return;
        }

        // Print the list
        for (int i = 0; i != allDevices.Count; ++i)
        {
            LivePacketDevice device = allDevices[i];
            Debug.Write((i + 1) + ". " + device.Name);
            if (device.Description != null)
                Debug.WriteLine(" (" + device.Description + ")");
            else
                Debug.WriteLine(" (No description available)");
        }
    }
}