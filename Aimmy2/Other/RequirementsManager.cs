using Aimmy2.MouseMovementLibraries.RazerSupport;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows;
using Visuality;

namespace Aimmy2.Other
{
    internal class RequirementsManager
    {
        public static void MainWindowInit()
        {
            if (Directory.GetCurrentDirectory().Contains("Temp"))
            {
                MessageBox.Show(
                    "Hi, it is made aware that you are running Aimmy without extracting it from the zip file." +
                    " Please extract Aimmy from the zip file or Aimmy will not be able to run properly." +
                    "\n\nThank you.",
                    "Aimmy V2"
                    );
            }

            CheckForRequirements();
        }

        public static bool CheckForRequirements()
        {
            if (!IsVCRedistInstalled())
            {
                MessageBox.Show("You don't have VCREDIST Installed, please install it to use Aimmy.", "Aimmy");
                return false;
            }

            if (!IsCUDAInstalled())
            {
                MessageBox.Show("You don't have 12.x CUDA Installed (or its improper install), you may not be able to use Aimmy.", "Aimmy");
            }

            if (!IsCUDNNInstalled())
            {
                MessageBox.Show("You don't have 9.x CUDNN Installed (or its improper install), you may not be able to use Aimmy.", "Aimmy");
                return false;
            }

            FileManager.LogInfo("Everything seemed good to RequirementsManager.");
            return true;
        }

        #region General Requirements for Aimmy
        public static bool IsVCRedistInstalled()
        {
            // Visual C++ Redistributable for Visual Studio 2015, 2017, and 2019 check
            string regKeyPath = @"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x64";

            using var key = Registry.LocalMachine.OpenSubKey(regKeyPath);

            if (key != null && key.GetValue("Installed") != null)
            {
                object? installedValue = key.GetValue("Installed");
                return installedValue != null && (int)installedValue == 1;
            }
            return false;
        }
        public static bool IsCUDAInstalled()
        {
            try
            {
                string baseDirectory = @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA";
                string envCudaPath = Environment.GetEnvironmentVariable("CUDA_PATH") ?? "";

                if (Directory.Exists(baseDirectory))
                {
                    var cudaDirectories = Directory.GetDirectories(baseDirectory, "v12.*");

                    foreach (var directory in cudaDirectories)
                    {
                        if (Directory.Exists(Path.Combine(directory, "bin")))
                        {
                            FileManager.LogInfo($"CUDA 12.x found in directory: {directory}");
                            return true;
                        }
                    }
                }

                //maybe they installed it on a different harddrive, or what if they wanna be different and change their local drive from C to D
                if (!string.IsNullOrEmpty(envCudaPath) && envCudaPath.Contains("CUDA\\v12."))
                {
                    FileManager.LogInfo("CUDA 12.x found via CUDA_PATH environment variable.");
                    return true;
                }

                string registryBasePath = @"SOFTWARE\NVIDIA Corporation\GPU Computing Toolkit\CUDA";
                using var baseKey = Registry.LocalMachine.OpenSubKey(registryBasePath);

                if (baseKey != null)
                {
                    var versionKeys = baseKey.GetSubKeyNames()
                                             .Where(name => name.StartsWith("v12."));
                    foreach (var versionKey in versionKeys)
                    {
                        using var key = baseKey.OpenSubKey(versionKey);
                        object? installedValue = key?.GetValue("64BitInstalled");

                        if (installedValue != null && (int)installedValue == 1)
                        {
                            FileManager.LogInfo($"CUDA {versionKey} is installed as per registry.");
                            return true;
                        }
                    }
                }

                string[] dlls =
                    [
                    "cublasLt64_12.dll",
                    "cublas64_12.dll",
                    "cufft64_11.dll",
                    "cudart64_12.dll",
                    ];

                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

                bool dllExists = false;
                foreach (string dll in dlls)
                {
                    if (File.Exists(Path.Combine(exeDirectory, dll)))
                    {
                        FileManager.LogInfo($"Found CUDA DLLs {dll} in executable directory");
                        dllExists = true;
                    }
                }

                if (dllExists)
                {
                    FileManager.LogInfo("Found all CUDA DLLS in Aimmy Directory");
                    return true;
                }

                FileManager.LogError("CUDA 12.6 is not installed");
                return false;
            }
            catch (Exception ex)
            {
                FileManager.LogError($"Error while checking for CUDA 12.6: {ex}");
                return false;
            }
        }

        public static bool IsCUDNNInstalled()
        {
            try
            {
                string cudnnDirectoryBase = @"C:\Program Files\NVIDIA\CUDNN";
                string aimmyDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Check if there's a folder starting with "v9." in the NVIDIA CUDNN directory
                if (Directory.Exists(cudnnDirectoryBase))
                {
                    var cudnnDirectories = Directory.GetDirectories(cudnnDirectoryBase, "v9.*");

                    foreach (var directory in cudnnDirectories)
                    {
                        if (Directory.Exists(Path.Combine(directory, "bin")))
                        {
                            FileManager.LogInfo($"cuDNN 9.x found in directory: {directory}");
                            return true;
                        }
                    }
                }

                if (File.Exists(Path.Combine(aimmyDirectory, "cudnn64_9.dll")))
                {
                    FileManager.LogInfo("cuDNN 9.x was found in the Aimmy directory.");
                    return true;
                }

                // maybe they installed it on a different harddrive, or what if they wanna be different and change their local drive from C to D
                string path = Environment.GetEnvironmentVariable("PATH") ?? "";

                if (!string.IsNullOrEmpty(path) && path.Contains("CUDNN\\v9."))
                {
                    FileManager.LogInfo("cuDNN 9.x may be installed and referenced in PATH.");
                    return true;
                }

                FileManager.LogError("CUDNN 9.x is not installed");
                return false;
            }
            catch (Exception ex)
            {
                FileManager.LogError($"Error while checking for CUDNN: {ex}");
                return false;
            }
        }

        public static bool IsTensorRTInstalled()
        {
            //Installation varies, this may be wrong... Program will not exit based on findings.
            try
            {
                string[] dlls =
                    [
                    "nvinfer_10.dll",
                    "nvinfer_plugin_10.dll",
                    "nvonnxparser_10.dll",
                    ];
                string baseDirectory = @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA";
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

                bool dllExists = false;

                foreach (string dll in dlls)
                {
                    // Search through CUDA directories for any version containing the DLLs
                    if (Directory.Exists(baseDirectory))
                    {
                        var cudaDirectories = Directory.GetDirectories(baseDirectory, "v*");
                        foreach (var directory in cudaDirectories)
                        {
                            if (File.Exists(Path.Combine(directory, "bin", dll)))
                            {
                                FileManager.LogInfo($"Found TensorRT DLL {dll} in {directory}\\bin");
                                dllExists = true;
                            }
                        }
                    }

                    // Check if the DLL is in the application's executable directory
                    if (File.Exists(Path.Combine(exeDirectory, dll)))
                    {
                        FileManager.LogInfo($"Found TensorRT DLL {dll} in executable directory");
                        dllExists = true;
                    }
                }

                if (dllExists)
                {
                    FileManager.LogInfo("Found all TensorRT DLLS in CUDA Path");
                    return true;
                }

                // maybe they installed it on a different harddrive, or what if they wanna be different and change their local drive from C to D

                string path = Environment.GetEnvironmentVariable("PATH") ?? "";

                if (!string.IsNullOrEmpty(path) && path.Contains("TensorRT"))
                {
                    FileManager.LogInfo("TensorRT may be installed based on PATH environment variable.");
                    return true;
                }


                FileManager.LogError("TensorRT not found.");
                return false;
            }
            catch (Exception ex)
            {
                FileManager.LogError($"Error while checking for TensorRT: {ex}");
                return false;
            }
        }
        #endregion
        #region LGHUB
        public static bool IsMemoryIntegrityEnabled() // false if enabled true if disabled, you want it disabled
        {
            //credits to Themida
            string keyPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforceCodeIntegrity";
            string valueName = "Enabled";
            object? value = Registry.GetValue(keyPath, valueName, null);
            if (value != null && Convert.ToInt32(value) == 1)
            {
                new NoticeBar("You have Memory Integrity enabled, please disable it to use Logitech Driver", 7000).Show();
                return false;
            }
            else return true;
        }

        public static bool CheckForGhub()
        {
            try
            {
                Process? process = Process.GetProcessesByName("lghub").FirstOrDefault(); //gets the first process named "lghub"
                if (process == null)
                {
                    ShowLGHubNotRunningMessage();
                    return false;
                }

                string ghubfilepath = process.MainModule.FileName;
                if (ghubfilepath == null)
                {
                    FileManager.LogError($"An error occurred. Run as admin and try again.", true, 6000);
                    return false;
                }

                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(ghubfilepath);

                if (!versionInfo.ProductVersion.Contains("2021"))
                {
                    ShowLGHubImproperInstallMessage();
                    return false;
                }

                return true;
            }
            catch (AccessViolationException ex)
            {
                FileManager.LogError($"An error occured: {ex.Message}\nRun as admin and try again.", true, 6000);
                return false;
            }
        }

        private static void ShowLGHubNotRunningMessage()
        {
            if (MessageBox.Show("LG HUB is not running, is it installed?", "Aimmy - LG HUB Mouse Movement", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No)
            {
                if (MessageBox.Show("Would you like to install it?", "Aimmy - LG HUB Mouse Movement", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    new LGDownloader().Show();
                }
            }
        }

        private static void ShowLGHubImproperInstallMessage()
        {
            if (MessageBox.Show("LG HUB install is improper, would you like to install it?", "Aimmy - LG HUB Mouse Movement", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                new LGDownloader().Show();
            }
        }
        #endregion
        #region RAZER
        public static bool CheckForRazerDevices(List<string> Razer_HID)
        {
            Razer_HID.Clear();
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Manufacturer LIKE 'Razer%'");
            var razerDevices = searcher.Get().Cast<ManagementBaseObject>();

            Razer_HID.AddRange(razerDevices.Select(device => device["DeviceID"]?.ToString() ?? string.Empty));

            return Razer_HID.Count != 0;
        }

        private static readonly string[] RazerSynapseProcesses =
        {
            "RazerAppEngine",
            "Razer Synapse",
            "Razer Synapse Beta",
            "Razer Synapse 3",
            "Razer Synapse 3 Beta"
        };
        public static async Task<bool> CheckRazerSynapseInstall() // returns true if running/installed and false if not installed/running
        {
            if (RazerSynapseProcesses.Any(processName => Process.GetProcessesByName(processName).Length != 0)) return true; // If any of them are running , return true

            var result = MessageBox.Show("Razer Synapse is not running (Or we cannot find the process), do you have it installed?",
                                         "Aimmy - Razer Synapse", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                await RZMouse.InstallRazerSynapse();
                return false;
            }

            bool isSynapseInstalled = Directory.Exists(@"C:\Program Files\Razer") ||
                                      Directory.Exists(@"C:\Program Files (x86)\Razer") ||
                                      CheckRazerRegistryKey();

            if (!isSynapseInstalled)
            {
                var installConfirmation = MessageBox.Show("Razer Synapse is not installed, would you like to install it?",
                                                          "Aimmy - Razer Synapse", MessageBoxButton.YesNo);

                if (installConfirmation == MessageBoxResult.Yes)
                {
                    await RZMouse.InstallRazerSynapse();
                    return false;
                }
            }

            return isSynapseInstalled;
        }

        private static bool CheckRazerRegistryKey()
        {
            using RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Razer");
            return key != null;
        }
        #endregion
    }
}