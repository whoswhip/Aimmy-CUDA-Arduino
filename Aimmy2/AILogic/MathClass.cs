using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Aimmy2.AILogic
{
    class MathClass
    {
        public static Rectangle ClampRectangle(Rectangle rect, int screenWidth, int screenHeight)
        {
            int x = Math.Max(0, Math.Min(rect.X, screenWidth - rect.Width));
            int y = Math.Max(0, Math.Min(rect.Y, screenHeight - rect.Height));
            int width = Math.Min(rect.Width, screenWidth - x);
            int height = Math.Min(rect.Height, screenHeight - y);

            return new Rectangle(x, y, width, height);
        }

        public static Func<double[], double[], double> L2Norm_Squared_Double = (x, y) =>
        {
            double dist = 0f;
            for (int i = 0; i < x.Length; i++)
            {
                dist += (x[i] - y[i]) * (x[i] - y[i]);
            }

            return dist;
        };

        public static unsafe float[] BitmapToFloatArray(Bitmap image)
        {
            int height = image.Height;
            int width = image.Width;
            int totalPixels = height * width;
            float[] result = new float[3 * totalPixels];
            float multiplier = 1.0f / 255.0f;

            Rectangle rect = new(0, 0, width, height);
            BitmapData bmpData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int stride = bmpData.Stride;
            int offset = stride - (width * 3);

            byte* ptr = (byte*)bmpData.Scan0.ToPointer();
            float* resultPtr = (float*)Marshal.UnsafeAddrOfPinnedArrayElement(result, 0);

            try
            {
                for (int i = 0; i < height; i++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        resultPtr[0] = ptr[2] * multiplier;                // R
                        resultPtr[totalPixels] = ptr[1] * multiplier;      // G
                        resultPtr[2 * totalPixels] = ptr[0] * multiplier;  // B
                        resultPtr++;
                        ptr += 3;
                    }
                    ptr += offset;
                }
            }
            finally
            {
                image.UnlockBits(bmpData);
            }

            return result;
        }
    }
}
