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

            for (int sourceY = 0, sourceOffset = 0, targetLength = target.Length, targetPos = 0, sourceLength = source.Length; sourceOffset < targetLength; sourceY++)
            {

                for (int sourceX = 0; sourceX < targetStride; sourceX++, sourceOffset++, targetPos++)
                {
                    r = 0;
                    b = 0;
                    g = 0;

                    for (int filtY = 0; filtY < filtHeight; filtY++)
                    {
                        for (int filtX = 0; filtX < filtWidth; filtX++)
                        {

                            samplePos = filtY * sourceWidth + filtX + sourceOffset;
                            if (samplePos >= sourceLength)
                                return;
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

        public static void TensorMatrix(Color[] Ix, Color[] Iy, ref Color[,,] A)
        {
            for (int i=0, l=Ix.Length; i< l; i++)
            {
                A[i, 0, 0].r = Mathf.Pow(Ix[i].r, 2);
                A[i, 0, 0].g = Mathf.Pow(Ix[i].g, 2);
                A[i, 0, 0].b = Mathf.Pow(Ix[i].b, 2);

                A[i, 1, 1].r = Mathf.Pow(Iy[i].r, 2);
                A[i, 1, 1].g = Mathf.Pow(Iy[i].g, 2);
                A[i, 1, 1].b = Mathf.Pow(Iy[i].b, 2);

                A[i, 0, 1].r = A[i, 1, 0].r = Ix[i].r * Iy[i].r;
                A[i, 0, 1].g = A[i, 1, 0].g = Ix[i].g * Iy[i].g;
                A[i, 0, 1].b = A[i, 1, 0].b = Ix[i].b * Iy[i].b;

            }
        }

        public static void Response(Color[,,] A, float kappa, ref Color[] R)
        {
            for (int i=0, l=R.Length; i< l; i++)
            {
                R[i].r = A[i, 0, 0].r * A[i, 1, 1].r - A[i, 0, 1].r * A[i, 1, 0].r - kappa * Mathf.Pow(A[i, 0, 0].r + A[i, 1, 1].r, 2);
                R[i].g = A[i, 0, 0].g * A[i, 1, 1].g - A[i, 0, 1].g * A[i, 1, 0].g - kappa * Mathf.Pow(A[i, 0, 0].g + A[i, 1, 1].g, 2);
                R[i].b = A[i, 0, 0].b * A[i, 1, 1].b - A[i, 0, 1].b * A[i, 1, 0].b - kappa * Mathf.Pow(A[i, 0, 0].b + A[i, 1, 1].b, 2);
                R[i].a = 1;
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

        public static Color Max(Color[] I)
        {
            Color max = new Color(0, 0, 0);
            for (int i=0, l=I.Length; i< l; i++)
            {
                if (I[i].r > max.r)
                {
                    max.r = I[i].r;
                }
                if (I[i].g > max.g)
                {
                    max.g = I[i].g;
                }
                if (I[i].b > max.b)
                {
                    max.b = I[i].b;
                }
            }
            return max;
        }

        public static void Threshold(Color[] I, Color threshold, ref Color[] T)
        {
            float r = threshold.r;
            float g = threshold.g;
            float b = threshold.b;
            for (int i = 0, l = I.Length; i < l; i++)
            {
                T[i].r = I[i].r > r ? 1 : 0;
                T[i].g = I[i].g > g ? 1 : 0;
                T[i].b = I[i].b > b ? 1 : 0;
            }
        }

        public static void Threshold(ref Color[] I, Color threshold)
        {
            float r = threshold.r;
            float g = threshold.g;
            float b = threshold.b;
            for (int i = 0, l = I.Length; i < l; i++)
            {
                I[i].r = I[i].r > r ? 1 : 0;
                I[i].g = I[i].g > g ? 1 : 0;
                I[i].b = I[i].b > b ? 1 : 0;
            }
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