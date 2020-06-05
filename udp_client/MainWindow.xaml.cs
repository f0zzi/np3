using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using System.Drawing;
using System.IO;
using System.Threading;

namespace udp_client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static bool is_take_shot = false;
        private static UdpClient udpClient = new UdpClient(11012);
        private static IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11011);
        private static ObservableCollection<BitmapImage> screenshots = new ObservableCollection<BitmapImage>();
        private static ImageConverter imageConverter = new ImageConverter();
        const int SIZE = 8;

        public MainWindow()
        {
            InitializeComponent();
            lbScreenshots.ItemsSource = screenshots;
        }

        private async void Start_click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(tbFrequency.Text) && Convert.ToInt32(tbFrequency.Text) > 0)
            {
                (sender as Button).IsEnabled = false;
                is_take_shot = true;
                btTakeShot.IsEnabled = false;
                btStop.IsEnabled = true;
                int delay = Convert.ToInt32(tbFrequency.Text);
                await Task.Run(() =>
                {
                    while (is_take_shot)
                    {
                        TakeShot();
                        Thread.Sleep(delay * 1000);
                    }
                });
            }
        }
        private void Stop_click(object sender, RoutedEventArgs e)
        {
            is_take_shot = false;
            (sender as Button).IsEnabled = false;
            btTakeShot.IsEnabled = true;
            btStart.IsEnabled = true;
        }

        private async void Take_screen_click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => TakeShot());
        }
        public static void TakeShot()
        {
            try
            {
                byte[] send_command = Encoding.UTF8.GetBytes("SEND");
                udpClient.Send(send_command, send_command.Length, RemoteIpEndPoint);
                int image_size = BitConverter.ToInt32(udpClient.Receive(ref RemoteIpEndPoint), 0);
                using (MemoryStream ms = new MemoryStream())
                {
                    while (image_size > 0)
                    {
                        byte[] receive_data = udpClient.Receive(ref RemoteIpEndPoint);
                        image_size -= receive_data.Length;
                        ms.Write(receive_data, 0, receive_data.Length);
                    }
                    byte[] tmp = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(tmp, 0, tmp.Length);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        screenshots.Add(BitmapToImageSource((Bitmap)imageConverter.ConvertFrom(tmp)));
                    });
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}
