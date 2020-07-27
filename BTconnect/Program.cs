using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace BTconnect
{
	class Program
	{
		static void check_home()
		{
			BluetoothClient client = new BluetoothClient();

			var AllDevices = client.PairedDevices;
			//var AllDevices = client.DiscoverDevices(10, true, true, false);

			foreach (BluetoothDeviceInfo Device in AllDevices)
			{
				Console.WriteLine(Device.DeviceName);

				if (Device.DeviceName == "home")
				{
					if (Device.Connected)
					{
						Console.WriteLine("Alread connected " + Device.DeviceName);
					}
					else
					{
						Console.WriteLine("reconnect " + Device.DeviceName + " : " + Device.DeviceAddress);
						//client.BeginConnect(Device.DeviceAddress, Device.InstalledServices[0], null, client);
						//{0000110b-0000-1000-8000-00805f9b34fb}

						var services = Device.InstalledServices.ToList();
						services.Reverse();

						foreach (var id in services)
						{
							try
							{
								Console.WriteLine("Connecting to: " + id);
								client.Connect(Device.DeviceAddress, id);
							}
							catch
							{
								Console.WriteLine("Failed to: " + id);
							}
						}

						Thread.Sleep(5 * 1000); //Extra waiting
					}
				}
			}
		}

		static void Main(string[] args)
		{
			while (true)
			{
				check_home();

				Thread.Sleep(5 * 1000);
			}

			//Console.ReadKey();
		}
	}
}
