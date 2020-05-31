using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace udpserver
{
    class Server
    {
        private static UdpClient server = new UdpClient(11011);
        private static IPEndPoint RemoteIpEndPoint = null;
        private static ImageConverter imageConverter = new ImageConverter();
        const int SIZE = 8;
        static void Main(string[] args)
        {
            SendImg();
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for command...");
                    byte[] command = server.Receive(ref RemoteIpEndPoint);
                    string received_command = Encoding.UTF8.GetString(command);
                    Console.WriteLine($"Command received: {received_command}");
                    switch (Encoding.UTF8.GetString(command))
                    {
                        case "SEND":
                            Console.WriteLine($"Sending screenshot to: {RemoteIpEndPoint}");
                            SendImg();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void SendImg()
        {
            //if (RemoteIpEndPoint != null)
            {
                byte[] data = (byte[])imageConverter.ConvertTo(TakeScreenshot(), typeof(byte[]));
                byte[] image_size = BitConverter.GetBytes(data.Length);
                server.Send(image_size, image_size.Length, RemoteIpEndPoint);
                //Thread.Sleep(200);
                byte[] buff = new byte[SIZE];
                int len = 0;
                using (MemoryStream ms = new MemoryStream(data))
                {
                    ms.Position = 0;
                    while (true)
                    {
                        len = ms.Read(buff, 0, SIZE);
                        if (len == 0)
                            break;
                        server.Send(buff, len, RemoteIpEndPoint);
                    }
                }
            }
        }

        static Bitmap TakeScreenshot()
        {
            Bitmap tmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graph = Graphics.FromImage(tmp);
            graph.CopyFromScreen(0, 0, 0, 0, tmp.Size);
            return tmp;
        }
    }
}
