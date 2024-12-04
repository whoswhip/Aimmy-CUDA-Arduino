using Aimmy2.Other;
using SharpGen.Runtime;
using System.Drawing.Imaging;
using System.Drawing;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Aimmy2.Class;

namespace Aimmy2.AILogic
{
    internal class CaptureManager
    {
        #region variables
        public Bitmap? _captureBitmap { get; private set; }

        private ID3D11Device? _device;
        private ID3D11DeviceContext? _context;
        private IDXGIOutputDuplication? _outputDuplication;
        private ID3D11Texture2D? _desktopImage;
        #endregion
        #region DirectX
        public void InitializeDirectX()
        {
            try
            {
                DisposeD3D11();

                // Initialize Direct3D11 device and context
                FeatureLevel[] featureLevels = new[]
                   {
                        FeatureLevel.Level_12_1,
                        FeatureLevel.Level_12_0,
                        FeatureLevel.Level_11_1,
                        FeatureLevel.Level_11_0,
                        FeatureLevel.Level_10_1,
                        FeatureLevel.Level_10_0,
                        FeatureLevel.Level_9_3,
                        FeatureLevel.Level_9_2,
                        FeatureLevel.Level_9_1
                    };
                var result = D3D11.D3D11CreateDevice(
                    null,
                    DriverType.Hardware,
                    DeviceCreationFlags.BgraSupport,
                    featureLevels,
                    out _device,
                    out FeatureLevel featureLevel, // DEBUG
                    out _context
                );
                FileManager.LogInfo($"Direct3D11 Feature Level Selected: {featureLevel}");
                if (result != Result.Ok || _device == null || _context == null)
                {
                    throw new InvalidOperationException($"Failed to create Direct3D11 device or context. HRESULT: {result}");
                }

                using var dxgiDevice = _device.QueryInterface<IDXGIDevice>();
                using var adapterForOutput = dxgiDevice.GetAdapter();
                var resultEnum = adapterForOutput.EnumOutputs(0, out var outputTemp);
                if (resultEnum != Result.Ok || outputTemp == null)
                {
                    throw new InvalidOperationException("Failed to enumerate outputs.");
                }


                using var output = outputTemp.QueryInterface<IDXGIOutput1>() ?? throw new InvalidOperationException("Failed to acquire IDXGIOutput1.");

                // Duplicate the output
                _outputDuplication = output.DuplicateOutput(_device);

                FileManager.LogInfo("Direct3D11 device, context, and output duplication initialized.");
            }
            catch (Exception ex)
            {
                FileManager.LogError("Error initializing Direct3D11: " + ex);
            }
        }
        public void ReinitializeD3D11()
        {
            try
            {
                InitializeDirectX();
                FileManager.LogError("Reinitializing D3D11, timing out for 500ms");
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                FileManager.LogError("Error during D3D11 reinitialization: " + ex);
            }
        }
        public static Rectangle ClampRectangle(Rectangle rect, int screenWidth, int screenHeight)
        {
            int x = Math.Max(0, Math.Min(rect.X, screenWidth - rect.Width));
            int y = Math.Max(0, Math.Min(rect.Y, screenHeight - rect.Height));
            int width = Math.Min(rect.Width, screenWidth - x);
            int height = Math.Min(rect.Height, screenHeight - y);

            return new Rectangle(x, y, width, height);
        }

        public Bitmap? D3D11Screen(Rectangle detectionBox)
        {
            try
            {
                if (_device == null || _context == null | _outputDuplication == null)
                {
                    FileManager.LogError("Device, context, or textures are null, attempting to reinitialize");
                    ReinitializeD3D11();

                    if (_device == null || _context == null || _outputDuplication == null)
                    {
                        throw new InvalidOperationException("Device, context, or textures are still null after reinitialization.");
                    }
                }

                if (_captureBitmap != null)
                {
                    FileManager.LogInfo("Bitmap was not null, disposing.", true, 1500);
                    _captureBitmap?.Dispose();
                    _captureBitmap = null;
                }

                var result = _outputDuplication!.AcquireNextFrame(500, out var frameInfo, out var desktopResource);

                if (result != Result.Ok)
                {
                    if (result == Vortice.DXGI.ResultCode.DeviceRemoved)
                    {
                        FileManager.LogError("Device removed, reinitializing D3D11.", true, 1000);
                        ReinitializeD3D11();
                        return null;
                    }

                    FileManager.LogError("Failed to acquire next frame: " + result + ". Reinitializing...");
                    ReinitializeD3D11();
                    return null;
                }

                using var screenTexture = desktopResource.QueryInterface<ID3D11Texture2D>();

                bool requiresNewResources = _desktopImage == null || _desktopImage.Description.Width != detectionBox.Width || _desktopImage.Description.Height != detectionBox.Height;

                if (requiresNewResources)
                {
                    _desktopImage?.Dispose();

                    var desc = new Texture2DDescription
                    {
                        Width = detectionBox.Width,
                        Height = detectionBox.Height,
                        MipLevels = 1,
                        ArraySize = 1,
                        Format = screenTexture.Description.Format,
                        SampleDescription = new SampleDescription(1, 0),
                        Usage = ResourceUsage.Staging,
                        CPUAccessFlags = CpuAccessFlags.Read,
                        BindFlags = BindFlags.None
                    };

                    _desktopImage = _device.CreateTexture2D(desc);
                }
                var box = new Box
                {
                    Left = detectionBox.Left,
                    Top = detectionBox.Top,
                    Front = 0,
                    Right = detectionBox.Right,
                    Bottom = detectionBox.Bottom,
                    Back = 1
                };

                _context!.CopySubresourceRegion(_desktopImage, 0, 0, 0, 0, screenTexture, 0, box);

                if (_desktopImage == null) return null;
                var map = _context.Map(_desktopImage, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);

                var bitmap = new Bitmap(detectionBox.Width, detectionBox.Height, PixelFormat.Format32bppArgb);
                var boundsRect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);

                unsafe
                {
                    Buffer.MemoryCopy((void*)map.DataPointer, (void*)mapDest.Scan0, mapDest.Stride * mapDest.Height, map.RowPitch * detectionBox.Height);
                    //    var sourcePtr = (byte*)map.DataPointer;
                    //    var destPtr = (byte*)mapDest.Scan0;
                    //    int rowPitch = map.RowPitch;
                    //    int destStride = mapDest.Stride;
                    //    int widthInBytes = detectionBox.Width * 4;

                    //    Buffer.MemoryCopy(sourcePtr, destPtr, widthInBytes * detectionBox.Height, widthInBytes * detectionBox.Height);
                }
                bitmap.UnlockBits(mapDest);
                _context.Unmap(_desktopImage, 0);
                _outputDuplication.ReleaseFrame();

                //FileManager.LogError($"Successfully captured screen with D3D11, width: {detectionBox.Width}, height: {detectionBox.Height}.");
                return bitmap;
            }

            catch (SharpGenException ex)
            {
                FileManager.LogError("SharpGenException: " + ex);
                ReinitializeD3D11();
                return null;
            }
            catch (Exception e)
            {
                FileManager.LogError("Error capturing screen:" + e);
                return null;
            }
        }
        public void DisposeD3D11()
        {
            if (_desktopImage != null)
            {
                _desktopImage?.Dispose();
                _desktopImage = null;
            }

            if (_outputDuplication != null)
            {
                _outputDuplication?.Dispose();
                _outputDuplication = null;
            }

            if (_context != null)
            {
                _context?.Dispose();
                _context = null;
            }

            if (_device != null)
            {
                _device?.Dispose();
                _device = null;
            }
        }

        #endregion
        #region GDI
        public Bitmap GDIScreen(Rectangle detectionBox)
        {
            if (detectionBox.Width <= 0 || detectionBox.Height <= 0)
            {
                throw new ArgumentException("Detection box dimensions must be greater than zero. (The enemy is too small)");
            }

            if (_device != null || _context != null || _outputDuplication != null)
            {
                FileManager.LogWarning("D3D11 was not properly disposed, disposing now...", true, 1500);
                DisposeD3D11();
            }

            if (_captureBitmap == null || _captureBitmap.Width != detectionBox.Width || _captureBitmap.Height != detectionBox.Height)
            {
                _captureBitmap?.Dispose();
                _captureBitmap = new Bitmap(detectionBox.Width, detectionBox.Height, PixelFormat.Format32bppArgb);
            }

            try
            {
                using Graphics g = Graphics.FromImage(_captureBitmap);
                g.CopyFromScreen(detectionBox.Left, detectionBox.Top, 0, 0, detectionBox.Size);
            }
            catch (Exception ex)
            {
                FileManager.LogError($"Failed to capture screen: {ex.Message}");
                throw;
            }

            //FileManager.LogError($"Successfully captured screen with GDI, width: {detectionBox.Width}, height: {detectionBox.Height}.");
            return _captureBitmap;
        }
        #endregion
        public void DisplaySettingsChanged()
        {
            if (Dictionary.dropdownState["Screen Capture Method"] == "DirectX")
            {
                ReinitializeD3D11();
            }
            else
            {
                _captureBitmap?.Dispose();
                _captureBitmap = null;
            }
        }
        public Bitmap? ScreenGrab(Rectangle detectionBox)
        {
            try
            {
                if (Dictionary.dropdownState["Screen Capture Method"] == "DirectX")
                {
                    Bitmap? frame = D3D11Screen(detectionBox);
                    return frame;
                }
                else
                {
                    Bitmap? frame = GDIScreen(detectionBox);
                    return frame;
                }
            }
            catch (Exception e)
            {
                FileManager.LogError("Error capturing screen:" + e);
                return null;
            }
        }

    }
}
