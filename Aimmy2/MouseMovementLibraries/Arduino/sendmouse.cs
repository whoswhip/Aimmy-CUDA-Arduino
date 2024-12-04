using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using Visuality;
using System.Windows;
using System.Reflection.Metadata;


namespace Aimmy2.MouseMovementLibraries.ArduinoSupport
{
    public static class StartArduino
    {
        public static Process MovementProcess { get; private set; }
        public static string[] hashes = new string[]
        {
            "8BEB14B3C04398B50E524054DF81AAC5BE5A17053E5E232945EE9DCDE1BE9B4E", // normal
            "71F595B74BC97AD96E4627E73D78B1EB5325B12569B9AB6D54F4DB6722013355" // protected 
        };
        public static void StartArduinoMouse()
        {
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string exePath = FindMouseMovementExe();
                new NoticeBar($"Arduino Mouse is starting from {exePath}", 5000).Show();

                ProcessStartInfo start = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                };

                MovementProcess = new Process();
                MovementProcess.StartInfo = start;
                MovementProcess.Start();

                Thread.Sleep(5000);
                if (MovementProcess.HasExited)
                {
                    MessageBox.Show("Arduino Movement has unexpectedly closed. (Make sure your Arduino is connected)", "Aimmy");
                }
            }
            catch
            {
                MessageBox.Show("Arduino Movement has failed to start.", "Aimmy");
            }
        }
        static string FindMouseMovementExe()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string exePath = Path.Combine(currentDirectory, "mousemovement.exe");

            if (File.Exists(exePath))
            {
                return ShuffleFileName(exePath);
            }
            else if (File.Exists(Path.Combine(currentDirectory, "mousemovement_protected.exe")))
            {
                return ShuffleFileName(Path.Combine(currentDirectory, "mousemovement_protected.exe"));
            }
            else
            {
                foreach (string file in Directory.GetFiles(currentDirectory))
                {
                    string hash = FileHash(file);
                    if (hashes[0].Equals(hash))
                        return ShuffleFileName(file);
                    else if (hashes[1].Equals(hash))
                        return ShuffleFileName(file);
                }
            }
            return null;
        }
        static string FileHash(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(stream);
                    StringBuilder hashString = new StringBuilder(2 * hash.Length);
                    foreach (byte b in hash)
                    {
                        hashString.AppendFormat("{0:X2}", b);
                    }
                    return hashString.ToString();
                }
            }
        }
        static string ShuffleFileName(string path)
        {
            string guid = Guid.NewGuid().ToString();
            File.Move(path, $"{guid}.exe");
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{guid}.exe");
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