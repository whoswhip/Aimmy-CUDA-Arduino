using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using Visuality;
using System.Windows;


namespace Aimmy2.MouseMovementLibraries.ArduinoSupport
{
    public static class StartArduino
    {
        public static void StartArduinoMouse()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string exePath = FindMouseMovementExe();
            new NoticeBar($"Arduino Mouse is starting from {exePath}", 5000).Show();

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true,
                CreateNoWindow = false,
            };

            Process process = Process.Start(start);
            Thread.Sleep(5000);
            if (process.HasExited)
            {
                MessageBox.Show("Arduino Movement has unexpectedly closed. (Make sure your Arduino is connected)", "Aimmy");
            }
        }
        static string FindMouseMovementExe()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string exePath = Path.Combine(currentDirectory, "mousemovement.exe");

            if (File.Exists(exePath))
            {
                string guid = Guid.NewGuid().ToString();
                File.Move(exePath, $"{guid}.exe");
                string filepath = Path.Combine(currentDirectory, $"{guid}.exe");
                return filepath;
            }
            else
            {
                foreach (string file in Directory.GetFiles(currentDirectory))
                {
                    if (!file.Contains("-"))
                        continue;
                    using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        using (SHA256 sha = SHA256.Create())
                        {
                            byte[] hash = sha.ComputeHash(stream);
                            StringBuilder hashString = new StringBuilder(2 * hash.Length);
                            foreach (byte b in hash)
                            {
                                hashString.AppendFormat("{0:X2}", b);
                            }

                            if (hashString.ToString().Equals("A871EC5FEF87ABA50C97E0198A065F2F130AA8155E0A20E9449BE2DDE3DA2447")) // protected mousemovement.exe
                            {
                                stream.Close();
                                string guid = Guid.NewGuid().ToString();
                                File.Move(file, $"{guid}.exe");
                                string filepath = Path.Combine(currentDirectory, $"{guid}.exe");
                                return filepath;
                            }
                            else if (hashString.ToString().Equals("8BEB14B3C04398B50E524054DF81AAC5BE5A17053E5E232945EE9DCDE1BE9B4E")) // default mousemovement.exe
                            {
                                stream.Close();
                                string guid = Guid.NewGuid().ToString();
                                File.Move(file, $"{guid}.exe");
                                string filepath = Path.Combine(currentDirectory, $"{guid}.exe");
                                return filepath;
                            }
                            else
                            {
                                stream.Close();
                                continue;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }

    public class SocketArduinoMouse
    {
        public SocketArduinoMouse() { }

        public void SendMouseCoordinates(int x, int y)
        {
            string ipAddress = "127.0.0.1";
            int port = 9999;

            using (var client = new TcpClient())
            {
                client.Connect(ipAddress, port);

                if (x != 0 || y != 0)
                {
                    string message = $"{x},{y}";
                    byte[] buffer = Encoding.ASCII.GetBytes(message);
                    client.GetStream().Write(buffer, 0, buffer.Length);
                }
            }
        }

        public void SendMouseClick(int click)
        {
            string ipAddress = "127.0.0.1";
            int port = 9999;

            using (var client = new TcpClient())
            {
                client.Connect(ipAddress, port);

                if (click.Equals(0) || click.Equals(1))
                {
                    string message = $"{click}";
                    byte[] buffer = Encoding.ASCII.GetBytes(message);
                    client.GetStream().Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}