using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Application = System.Windows.Application;
using System.Threading;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using Notification.Wpf;

namespace BTconnectGUI
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly NotificationManager notificationManager = new NotificationManager();

		void log(string msg)
		{
			Application.Current.Dispatcher.Invoke(new Action(() => {
				log_window.Text += msg + "\n";

				log_window.ScrollToEnd();
			}));
		}

		void check_home()
		{
			BluetoothClient client = new BluetoothClient();

			while (true)
			{
				var AllDevices = client.PairedDevices;

				foreach (BluetoothDeviceInfo Device in AllDevices)
				{
					log(Device.DeviceName);

					if (Device.DeviceName == "home")
					{
						if (Device.Connected)
						{
							log("Alread connected " + Device.DeviceName);
						}
						else
						{
							notificationManager.Show("Reconnecting to Home", "BTconnect", new System.TimeSpan(2));

							log("reconnect " + Device.DeviceName + " : " + Device.DeviceAddress);

							var services = Device.InstalledServices.ToList();
							services.Reverse();

							foreach (var id in services)
							{
								try
								{
									log("Connecting to: " + id);
									client.Connect(Device.DeviceAddress, id);
								}
								catch
								{
									log("Failed to: " + id);
								}
							}

							Thread.Sleep(5 * 1000); //Extra waiting
						}
					}
				}

				Thread.Sleep(5 * 1000);
			}
		}

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Stream iconstream = Application.GetResourceStream(new Uri("pack://application:,,,/BTconnectGUI;component/hgz.ico")).Stream;

			var TaskBarIcon = new NotifyIcon
			{
				Icon = new Icon(iconstream, SystemInformation.IconSize),
				Text = @"BTconnect",
				Visible = true
			};

			iconstream.Close();

			TaskBarIcon.MouseClick += quickIcon_MouseClick;

			CheckLoop();

			notificationManager.Show(title: "BTconnect", message: "Reconnecting to Home",
				NotificationType.Notification, "", expirationTime: new TimeSpan(0, 0, 2),
				onClick: () => log("act"), null);
		}

		private void CheckLoop()
		{
			Task.Run(delegate
			{
				check_home();
			});
		}

		private void quickIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (Visibility == Visibility.Hidden)
				{
					Visibility = Visibility.Visible;
					Activate();
				}
				else
				{
					Visibility = Visibility.Hidden;
				}
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			Application.Current.Shutdown();
		}
	}
}
