using UnityEngine;
using System.Collections;

namespace ImageAnalysis
{
    public static class Convolve
    {
        public static double[,] Valid<T>(ref double[,] source, int sourceWidth) where T : Filter
        {
            T filter = Filter.Get<T>();

            int sourceHeight = source.Length / sourceWidth;

            double[,] kernel = filter.Kernel;
            int filtWidth = kernel.GetLength(1);
            int filtHeight = kernel.GetLength(0);

            int targetWidth = sourceWidth - filtWidth + 1;
            int targetHeight = sourceHeight - filtHeight + 1;

            double[,] target = new double[targetHeight * targetWidth, source.GetLength(1)];
            Valid(ref source, sourceWidth, ref target, targetWidth, filter);
            return target;
        }

        public static void Valid(ref double[,] source, int sourceStride, ref double[,] target, int targetStride, Filter filter)
        {

            int samplePos = 0;
            double[,] kernel = filter.Kernel;

            int filtWidth = kernel.GetLength(1);
            int filtHeight = kernel.GetLength(0);
            int sourceLength = source.GetLength(0);
            int sourceHeight = sourceLength / sourceStride;
            int targetLength = target.GetLength(0);
            int targetHeight = targetLength / targetStride;

            double v = 0.0;
            for (int color = 0, colors = Mathf.Min(source.GetLength(1), target.GetLength(1)); color < colors; color++)
            {

                for (int sourceY = 0, sourceOffset = 0, targetPos = 0; sourceY < sourceHeight - filtHeight + 1; sourceY++)
                {

                    for (int targetX = 0; targetX < targetStride; targetX++, sourceOffset++, targetPos++)
                    {

                        v = 0.0;

                        for (int filtY = 0; filtY < filtHeight; filtY++)
                        {
                            for (int filtX = 0; filtX < filtWidth; filtX++)
                            {

                                samplePos = filtY * sourceStride + filtX + sourceOffset;
                                v += source[samplePos, color] * filter.Kernel[filtY, filtX];
                            }
                        }

                        target[targetPos, color] = (v * filter.Scale + filter.Bias);
                    }

                    sourceOffset += filtWidth - 1;
                }
            }
        }

        public static double[,] Add(ref double[,] a, ref double[,] b)
        {
            double[,] target = new double[a.GetLength(0), a.GetLength(1)];
            Add(ref a, ref b, ref target);
            return target;
        }

        public static void Add(ref double[,] a, ref double[,] b, ref double[,] target)
        {
            for (int color = 0, colors = Mathf.Min(a.GetLength(1), b.GetLength(1), target.GetLength(1)); color < colors; color++)
            {
                for (int i = 0, l = target.GetLength(0); i < l; i++)
                {
                    target[i, color] = a[i, color] + b[i, color];
                }
            }
        }


        public static double[,] Subtract(ref double[,] a, ref double[,] b)
        {
            double[,] target = new double[a.GetLength(0), a.GetLength(1)];

            Subtract(ref a, ref b, ref target);
            return target;
        }

        public static void Subtract(ref double[,] a, ref double[,] b, ref double[,] target)
        {

            for (int color = 0, colors = Mathf.Min(a.GetLength(1), b.GetLength(1), target.GetLength(1)); color < colors; color++)
            {
                for (int i = 0, l = target.GetLength(0); i < l; i++)
                {
                    target[i, color] = a[i, color] - b[i, color];
                }
            }
        }

        public static void TensorMatrix(ref double[,] Ix, ref double[,] Iy, ref double[,,,] A)
        {
            for (int color = 0, colors = Mathf.Min(Ix.GetLength(1), Iy.GetLength(1), A.GetLength(1)); color < colors; color++)
            {
                for (int i = 0, l = Ix.GetLength(0); i < l; i++)
                {

                    A[i, color, 0, 0] = System.Math.Pow(Ix[i, color], 2.0);
                    A[i, color, 1, 1] = System.Math.Pow(Iy[i, color], 2.0);
                    A[i, color, 0, 1] = A[i, color, 1, 0] = Ix[i, color] * Iy[i, color];

                }
            }
        }

        public static void Response(ref double[,,,] A, double kappa, ref double[,] R)
        {
            for (int color = 0, colors = Mathf.Min(A.GetLength(1), R.GetLength(1)); color < colors; color++)
            {
                for (int i = 0, l = Mathf.Min(R.GetLength(0), A.GetLength(0)); i < l; i++)
                {
                    R[i, color] = 
                        A[i, color, 0, 0] * A[i, color, 1, 1] - A[i, color, 0, 1] * A[i, color, 1, 0] - 
                        kappa * System.Math.Pow(A[i, color, 0, 0] + A[i, color, 1, 1], 2.0);
                }
            }
        }

        public static Color[] OriginalSize<T>(ref double[,] im, int toStride) where T : Filter
        {
            double[,] kernel = Filter.Get<T>().Kernel;
            int kernelWidth = kernel.GetLength(1);
            int kernelHeight = kernel.GetLength(0);
            int height = im.Length / (toStride - kernelWidth + 1);
            return Resize(ref im, toStride - kernelWidth + 1, toStride, height + kernelHeight - 1);
        }

        public static Color[] Resize(ref double[,] im, int fromStride, int toStride, int toHeight)
        {
            int height = im.Length / fromStride;
            Color[] target = new Color[toStride * toHeight];
            Convert(ref im, fromStride, height, toStride, ref target);
            return target;
        }

        public static double[] Max(ref double[,] I)
        {
            int colors = I.GetLength(1);
            double[] max = new double[colors];

            for (int color = 0; color < colors; color++)
            {
                for (int i = 0, l = I.GetLength(0); i < l; i++)
                {
                    if (max[color] > I[i, color] || i ==0)
                    {
                        max[color] = I[i, color];
                    }
                }
            }
            return max;
        }

        public static double[] Min(ref double[,] I)
        {
            int colors = I.GetLength(1);
            double[] min = new double[colors];

            for (int color = 0; color < colors; color++)
            {
                for (int i = 0, l = I.GetLength(0); i < l; i++)
                {
                    if (min[color] < I[i, color] || i == 0)
                    {
                        min[color] = I[i, color];
                    }
                }
            }
            return min;

        }

        public static void Threshold(ref double[,] I, ref double[] threshold, ref double[,] T)
        {

            for (int color = 0, colors = Mathf.Min(I.GetLength(1), T.GetLength(1), threshold.Length); color < colors; color++)
            {
                for (int i = 0, l = I.Length; i < l; i++)
                {
                    T[i, color] = I[i, color] > threshold[color] ? 1 : 0;
                }
            }
        }

        public static void ThresholdInplace(ref double[,] I, double[] threshold)
        {

            for (int color = 0, colors = Mathf.Min(I.GetLength(1), threshold.Length); color < colors; color++)
            {

                for (int i = 0, l = I.GetLength(0); i < l; i++)
                {
                    I[i, color] = I[i, color] > threshold[color] ? 0 : 1;
                }
            }
        }

        public static void ValueScale01(ref double[,] I)
        {
            double[] min = Min(ref I);
            double[] max = Max(ref I);
            ValueScale01(ref I, min, max);
        }

        public static void ValueScale01(ref double[,] I, double[] min, double[] max)
        {
            double[] span = new double[Mathf.Min(min.Length, max.Length)];
            for (int i=0; i<span.Length; i++)
            {
                span[i] = max[i] - min[i];
            }

            for (int color = 0, colors = Mathf.Min(I.GetLength(1), span.Length); color < colors; color++)
            {

                for (int i = 0, l = I.GetLength(0); i < l; i++)
                {
                    I[i, color] = (I[i, color] - min[color]) / span[color];
                }
            }
        }

        public static void Convert(ref double[,] I, int fromStride, int fromHeight, int toStride, ref Color[] target)
        {
            bool hasAlpha = I.GetLength(1) == 4;

            for (int y = 0; y < fromHeight; y++)
            {
                for (int x=0, sourcePos=y*fromStride, targetPos=y*toStride; x < fromStride && x < toStride; x++, sourcePos++, targetPos++)
                {

                    target[targetPos].r = (float)I[sourcePos, 0];
                    target[targetPos].g = (float)I[sourcePos, 1];
                    target[targetPos].b = (float)I[sourcePos, 2];
                    if (hasAlpha)
                    {
                        target[targetPos].r = (float) I[sourcePos, 3];
                    } else
                    {
                        target[targetPos].a = 1f;
                    }
                    
                }

            }

        }

        public static void Color2Double(ref Color[] colors, ref double[,] I)
        {
            bool hasAlpha = I.GetLength(1) == 4;
            for (int i=0, l=colors.Length; i< l; i++)
            {
                I[i, 0] = colors[i].r;
                I[i, 1] = colors[i].g;
                I[i, 2] = colors[i].b;
                if (hasAlpha)
                {
                    I[i, 3] = colors[i].a;
                }
            }
        }

    }

}