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
		private NotifyIcon TaskBarIcon;

		void log(string msg)
		{
			Application.Current.Dispatcher.Invoke(new Action(() => {
				log_window.Text += msg + "\n";

				log_window.ScrollToEnd();
			}));
		}

		void toast(string title, string msg)
		{
			notificationManager.Show(title: title, message: msg,
				NotificationType.Notification, "", expirationTime: new TimeSpan(0, 0, 2),
				onClick: () => log("act"), null);
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
							toast("BTconnect", "Reconnecting to Home");

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

			TaskBarIcon = new NotifyIcon
			{
				Icon = new Icon(iconstream, SystemInformation.IconSize),
				Text = @"BTconnect",
				Visible = true
			};

			iconstream.Close();

			TaskBarIcon.MouseClick += quickIcon_MouseClick;
			TaskBarIcon.MouseDoubleClick += quickIcon_MouseDoubleClick;

			Task.Run(delegate
			{
				check_home();
			});
		}

		private void quickIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				TaskBarIcon.Dispose();
				Application.Current.Shutdown();
			}
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

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Hide();

			e.Cancel = true;
		}
	}
}
