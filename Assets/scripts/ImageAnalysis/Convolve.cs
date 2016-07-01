using UnityEngine;
using System.Collections;

namespace ImageAnalysis
{
    public static class Convolve
    {
        public static Color[] Valid<T>(Color[] source, int sourceWidth) where T : Filter
        {
            T filter = Filter.Get<T>();

            int sourceHeight = source.Length / sourceWidth;

            double[,] kernel = filter.Kernel;
            int filtWidth = kernel.GetLength(1);
            int filtHeight = kernel.GetLength(0);

            int targetWidth = sourceWidth - filtWidth + 1;
            int targetHeight = sourceHeight - filtHeight + 1;

            Color[] target = new Color[targetHeight * targetWidth];
            Valid(source, sourceWidth, ref target, targetWidth, filter);
            return target;
        }

        public static void Valid(Color[] source, int sourceWidth, ref Color[] target, int targetStride, Filter filter)
        {

            int samplePos = 0;
            double[,] kernel = filter.Kernel;

            int filtWidth = kernel.GetLength(1);
            int filtHeight = kernel.GetLength(0);

            double r = 0;
            double g = 0;
            double b = 0;

            for (int sourceX = 0, sourceOffset = 0, targetLength = target.Length, targetPos = 0; sourceOffset < targetLength; sourceX++)
            {

                for (int sourceY = 0; sourceY < targetStride; sourceY++, sourceOffset++, targetPos++)
                {
                    r = 0;
                    b = 0;
                    g = 0;

                    for (int filtY = 0; filtY < filtHeight; filtY++)
                    {
                        for (int filtX = 0; filtX < filtWidth; filtX++)
                        {

                            samplePos = filtY * sourceWidth + filtX + sourceOffset;
                            r += source[samplePos].r * filter.Kernel[filtY, filtX];
                            g += source[samplePos].g * filter.Kernel[filtY, filtX];
                            b += source[samplePos].b * filter.Kernel[filtY, filtX];
                        }
                    }

                    target[targetPos].r = Mathf.Clamp01((float)(r * filter.Scale + filter.Bias));
                    target[targetPos].g = Mathf.Clamp01((float)(g * filter.Scale + filter.Bias));
                    target[targetPos].b = Mathf.Clamp01((float)(b * filter.Scale + filter.Bias));
                    target[targetPos].a = 1;

                }

                sourceOffset += filtWidth - 1;
            }
        }

        public static Color[] Add(Color[] a, Color[] b)
        {
            Color[] target = new Color[a.Length];
            Add(a, b, ref target);
            return target;
        }

        public static void Add(Color[] a, Color[] b, ref Color[] target)
        {
            for (int i = 0, l = target.Length; i < l; i++)
            {
                target[i].r = Mathf.Clamp01(a[i].r + b[i].r);
                target[i].g = Mathf.Clamp01(a[i].g + b[i].g);
                target[i].b = Mathf.Clamp01(a[i].b + b[i].b);
                target[i].a = 1f;
            }
        }


        public static Color[] Subtract(Color[] a, Color[] b)
        {
            Color[] target = new Color[a.Length];

            Subtract(a, b, ref target);
            return target;
        }

        public static void Subtract(Color[] a, Color[] b, ref Color[] target)
        {
            for (int i = 0, l = target.Length; i < l; i++)
            {
                target[i].r = Mathf.Clamp01(a[i].r - b[i].r);
                target[i].g = Mathf.Clamp01(a[i].g - b[i].g);
                target[i].b = Mathf.Clamp01(a[i].b - b[i].b);
                target[i].a = 1f;
            }
        }

        public static Color[] OriginalSize<T>(Color[] im, int toStride) where T : Filter
        {
            double[,] kernel = Filter.Get<T>().Kernel;
            int kernelWidth = kernel.GetLength(1);
            int kernelHeight = kernel.GetLength(0);
            int height = im.Length / (toStride - kernelWidth + 1);
            return Resize(im, toStride - kernelWidth + 1, toStride, height + kernelHeight - 1);
        }

        public static Color[] Resize(Color[] im, int fromStride, int toStride, int toHeight)
        {
            int height = im.Length / fromStride;
            Color[] target = new Color[toStride * toHeight];
            Resize(im, fromStride, height, toStride, ref target);
            return target;
        }

        public static void Resize(Color[] im, int fromStride, int fromHeight, int toStride, ref Color[] target)
        {
            for (int i = 0; i < fromHeight; i++)
            {
                System.Array.Copy(im, i * fromStride, target, i * toStride, fromStride);
            }

        }
    }

}