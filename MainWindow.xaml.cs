using Microsoft.VisualBasic;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
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

namespace NetworkScanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public List<NetworkItem> Items { get; set; }
        string IP;

        public class NetworkItem
        {
            public string IP { get; set; }
            public string Name { get; set; }
            public bool Online { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Items = new List<NetworkItem>();
            Data.ItemsSource = Items;
            Data.Columns[0].Header = "IP Address";
            Data.Columns[1].Header = "Hostname";
            Data.Columns[2].Header = "Online";
        }

        ProgressDialog progressDialog = new ProgressDialog();

        private void MBTSNS_Click(object sender, RoutedEventArgs e)
        {
            string IP = Interaction.InputBox("Please provide an IP address (public or private) of the network to scan.", "NetworkScanner", "0.0.0.0", 10, 10);
            this.IP = IP;
            progressDialog.DoWork += ProgressDialog_DoWork;
            progressDialog.WindowTitle = "NetworkScanner";
            progressDialog.Text = $"Scanning network of {IP}";
            progressDialog.ShowTimeRemaining = true;
            progressDialog.ShowDialog();
        }

        private void ProgressDialog_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            string[] IPS = IP.Split('.');
            string new_IP = $"{IPS[0]}.{IPS[1]}.{IPS[2]}";

            int found = 0;

            for (int i = 0; i < 255; i++)
            {
                if (progressDialog.CancellationPending)
                    return;

                //Thread.Sleep(50);
                progressDialog.ReportProgress((int)(i / 255.0 * 100), null, string.Format(System.Globalization.CultureInfo.CurrentCulture, "Scanning network: {0}%", Math.Round(i / 255.0 * 100)) + $" ({new_IP + "." + i})" +
                    $" Found {found} alive IPs");

                try
                {
                    Ping ping = new Ping();
                    PingReply reply = ping.Send(new_IP + "." + i, 1000);
                    if (reply.Status == IPStatus.Success)
                    {
                        found++;
                        NetworkItem networkItem = new NetworkItem();
                        networkItem.IP = new_IP + "." + i;
                        IPAddress[] localIPs = Dns.GetHostAddresses(new_IP + "." + i);
                        IPHostEntry entry = Dns.GetHostByAddress(localIPs[0]);
                        networkItem.Name = entry.HostName;
                        MessageBox.Show(Thread.CurrentThread.ManagedThreadId.ToString());
                        Dispatcher.Invoke(() =>
                        {
                            Items.Add(networkItem);
                            Data.ItemsSource = Items;
                            //MessageBox.Show(Items.Count.ToString());
                            MessageBox.Show(Thread.CurrentThread.ManagedThreadId.ToString());
                        }, System.Windows.Threading.DispatcherPriority.Render);
                        Application.Current.Dispatcher.Invoke(() => {
                            Items.Add(networkItem);
                            //MessageBox.Show(Items.Count.ToString());
                            MessageBox.Show(Thread.CurrentThread.ManagedThreadId.ToString());
                            Data.ItemsSource = Items;
                        });
                    }
                } catch { }
            }
        }
    }
}
