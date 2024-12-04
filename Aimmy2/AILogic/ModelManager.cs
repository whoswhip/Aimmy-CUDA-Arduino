using Aimmy2.Class;
using Aimmy2.Other;
using Microsoft.ML.OnnxRuntime;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace Aimmy2.AILogic
{
    internal class ModelManager
    {
        private const int NUM_DETECTIONS = 8400; // Standard for OnnxV8 model (Shape: 1x5x8400)

        public InferenceSession? _onnxModel { get; private set; }
        public List<string>? _outputNames { get; private set; }
        public RunOptions? _modeloptions { get; private set; }


        private SessionOptions sessionOptions = new()
        {
            EnableCpuMemArena = true,
            EnableMemoryPattern = true,
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
            ExecutionMode = ExecutionMode.ORT_PARALLEL

        };

        #region initialization
        public async Task InitializeModel(string modelPath)
        {
            string executionProvider = Dictionary.dropdownState["Execution Provider Type"];
            try
            {
                await LoadModelAsync(modelPath, executionProvider);
            }
            catch (Exception ex)
            {
                await FallbackModelLoad(modelPath, executionProvider, ex);
            }
            finally
            {
                FileManager.CurrentlyLoadingModel = false;
            }
        }

        public async Task FallbackModelLoad(string modelPath, string executionProvider, Exception ex)
        {
            string fallbackProvider = executionProvider == "CUDA" ? "TensorRT" : "CPU";
            try
            {
                await LoadModelAsync(modelPath, fallbackProvider);
            }
            catch (Exception fallbackEx)
            {
                throw new InvalidOperationException($"Failed to load model with fallback provider {fallbackProvider}", fallbackEx);
            }
        }
        #endregion

        public Task LoadModelAsync(string modelPath, string executionProvider)
        {
            try
            {
                switch (executionProvider)
                {
                    case "CUDA":
                        FileManager.LogInfo("Loading model with CUDA");
                        sessionOptions.AppendExecutionProvider_CUDA();
                        break;
                    case "TensorRT":
                        var tensorrtOptions = new OrtTensorRTProviderOptions();
                        tensorrtOptions.UpdateOptions(new Dictionary<string, string>
                        {
                            { "device_id", "0" },
                            { "trt_fp16_enable", "1" },
                            { "trt_engine_cache_enable", "1" },
                            { "trt_engine_cache_path", "bin/models" }
                        });
                        
                        FileManager.LogInfo($"{modelPath} {Path.ChangeExtension(modelPath, ".engine")}");
                        FileManager.LogInfo("Loading model with TensorRT, expect long model load time.", true);

                        sessionOptions.AppendExecutionProvider_Tensorrt(tensorrtOptions);
                        break;
                    case "CPU":
                        FileManager.LogInfo("Loading model with CPU, expect low performance.", true);
                        sessionOptions.AppendExecutionProvider_CPU();
                        break;
                    default:
                        FileManager.LogInfo("Loading model with CUDA");
                        sessionOptions.AppendExecutionProvider_CUDA();
                        break;

                }

                _onnxModel = new InferenceSession(modelPath, sessionOptions);
                _outputNames = new List<string>(_onnxModel.OutputMetadata.Keys);

                FileManager.LogInfo("successfully loaded model");

                // Validate the onnx model output shape (ensure model is OnnxV8)
                ValidateOnnxShape();
            }
            catch (OnnxRuntimeException ex)
            {
                FileManager.LogError($"ONNXRuntime had an error: {ex}");

                string? message = null;
                string? title = null;

                // just in case
                if (ex.Message.Contains("TensorRT execution provider is not enabled in this build") ||
                    (ex.Message.Contains("LoadLibrary failed with error 126") && ex.Message.Contains("onnxruntime_providers_tensorrt.dll")))
                {
                    if (RequirementsManager.IsTensorRTInstalled())
                    { // TensorRT should be preinstalled in all aimmy cuda versions, so this should be rare unless user has personally deleted the files.
                        message = "TensorRT has been found by Aimmy, but not by ONNX. Please check your configuration.\nHint: Check CUDNN and your CUDA, and install dependencies to PATH correctly.";
                        title = "Configuration Error";
                    }
                    else
                    {
                        message = "TensorRT execution provider has not been found on your build. Please check your configuration.\nHint: Download TensorRT 10.3.x and install the LIB folder to path.";
                        title = "TensorRT Error";
                    }
                }
                else if (ex.Message.Contains("CUDA execution provider is not enabled in this build") ||
                         (ex.Message.Contains("LoadLibrary failed with error 126") && ex.Message.Contains("onnxruntime_providers_cuda.dll")))
                {
                    if (RequirementsManager.IsCUDAInstalled() && RequirementsManager.IsCUDNNInstalled())
                    {
                        message = "CUDA & CUDNN have been found by Aimmy, but not by ONNX. Please check your configuration.\nHint: Check CUDNN and your CUDA installations, path, etc. PATH directories should point directly towards the DLLS.";
                        title = "Configuration Error";
                    }
                    else
                    {
                        message = "CUDA execution provider has not been found on your build. Please check your configuration.\nHint: Download CUDA 12.x. Then install CUDNN 9.x to your PATH (or install the DLL included aimmy)";
                        title = "CUDA Error";
                    }
                }

                if (message != null && title != null)
                {
                    System.Windows.MessageBox.Show(message, title, (MessageBoxButton)MessageBoxButton.OK, (MessageBoxImage)MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                FileManager.LogError($"Error starting the model: {ex}");
                _onnxModel?.Dispose();
            }

            


            return Task.CompletedTask;
        }

        public void ValidateOnnxShape()
        {
            var expectedShape = new int[] { 1, 5, NUM_DETECTIONS };
            if (_onnxModel != null)
            {
                var outputMetadata = _onnxModel.OutputMetadata;
                if (!outputMetadata.Values.All(metadata => metadata.Dimensions.SequenceEqual(expectedShape)))
                {
                    FileManager.LogError($"Output shape does not match the expected shape of {string.Join("x", expectedShape)}.\n\nThis model will not work with Aimmy, please use an YOLOv8 model converted to ONNXv8.", true);
                }
            }
        }
    }
}
