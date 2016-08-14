using UnityEngine;
using System;

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

		public static double[,] Multiply(ref double[,] a, ref double[,] b)
		{
			double[,] target = new double[a.GetLength(0), a.GetLength(1)];

			Multiply(ref a, ref b, ref target);
			return target;
		}

		public static void Multiply(ref double[,] a, ref double[,] b, ref double[,] target)
		{

			for (int color = 0, colors = Mathf.Min(a.GetLength(1), b.GetLength(1), target.GetLength(1)); color < colors; color++)
			{
				for (int i = 0, l = target.GetLength(0); i < l; i++)
				{
					target[i, color] = a[i, color] * b[i, color];
				}
			}
		}


		public static double[,] Pow(ref double[,] a, ref double b)
		{
			double[,] target = new double[a.GetLength(0), a.GetLength(1)];

			Pow(ref a, b, ref target);
			return target;
		}

		public static void Pow(ref double[,] a, double b, ref double[,] target)
		{

			for (int color = 0, colors = Mathf.Min(a.GetLength(1), target.GetLength(1)); color < colors; color++)
			{
				for (int i = 0, l = target.GetLength(0); i < l; i++)
				{
					target [i, color] = System.Math.Pow (a [i, color], b);	
				}	
			}
		}

        public static void Tensor(ref double[,] Ix, ref double[,] Iy, ref double[,,,] A)
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

		public static void Response(ref double[,] Sx2, ref double[,] Sy2, ref double[,] Sxy, double kappa, ref double[,] R)
		{
			for (int color = 0, colors = Mathf.Min (Sx2.GetLength (1), R.GetLength (1)); color < colors; color++) {
				for (int i = 0, l = Mathf.Min (R.GetLength (0), Sx2.GetLength (0)); i < l; i++) {
					R[i, color] = 
						-1 * (Sx2[i, color] * Sy2[i, color] - Sxy[i, color] * Sxy[i, color] ) +
						kappa * System.Math.Pow(Sx2[i, color] + Sy2[i, color], 2.0);
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
						1 * (A[i, color, 0, 0] * A[i, color, 1, 1] - A[i, color, 0, 1] * A[i, color, 1, 0] ) - 
                        1 * kappa * System.Math.Pow(A[i, color, 0, 0] + A[i, color, 1, 1], 2.0);
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

        public static void Convert(ref double[,] I, int fromStride, int fromHeight, int toStride, ref Color[] target)
        {
            bool hasAlpha = I.GetLength(1) == 4;

            int toHeight = target.Length / toStride;

            int fromXMin = Mathf.Max(0, (toStride - fromStride) / 2);
            int fromXMax = fromXMin + fromStride - 1;
            int fromYMin = Mathf.Max(0, (toHeight - fromHeight) / 2);
            int fromYMax = fromYMin + fromHeight - 1;

            int targetPos = 0;
            for (int y = 0; y < toHeight; y++)
            {
                for (int x=0; x<toStride; x++, targetPos++)
                {

                    if (y < fromYMin || y > fromYMax || x < fromXMin || x > fromXMax)
                    {
                        target[targetPos].r = 0.5f;
                        target[targetPos].g = 0.5f;
                        target[targetPos].b = 0.5f;
                        target[targetPos].a = 0;
                    }
                    else {
                        int sourcePos = (x - fromXMin) + (y - fromYMin) * fromStride;
                        target[targetPos].r = (float)I[sourcePos, 0];
                        target[targetPos].g = (float)I[sourcePos, 1];
                        target[targetPos].b = (float)I[sourcePos, 2];
                        if (hasAlpha)
                        {
                            target[targetPos].a = (float)I[sourcePos, 3];
                        }
                        else
                        {
                            target[targetPos].a = 1f;
                        }
                    }
                    
                }

            }

        }

        public static void Apply(ref double[,] I, int stride, Texture2D tex)
        {
            Color[] pixels = new Color[I.GetLength(0)];
            bool hasAlpha = I.GetLength(1) == 4;
            for (int i=0, l=pixels.Length; i< l; i++)
            {
                pixels[i].r = (float)I[i, 0];
                pixels[i].g = (float)I[i, 1];
                pixels[i].b = (float)I[i, 2];
                if (hasAlpha)
                {
                    pixels[i].a = (float)I[i, 3];
                }
                else
                {
                    pixels[i].a = 1f;
                }

            }
            if (pixels.Length != tex.width * tex.height)
            {
                pixels = SubSample(ref pixels, stride, pixels.Length / stride, tex.width, tex.height);
            }
            tex.SetPixels(pixels);
            tex.Apply();
        }

        public static Color[] SubSample(ref Color[] source, int sourceStride, int sourceHeight, int targetStride, int targetHeight)
        {
            Color[] target = new Color[targetHeight * targetStride];

            int sourceY = 0;
            int sourceX = 0;
            float scaleY = (float)sourceHeight / targetHeight;
            float scaleX = (float)sourceStride / targetStride;

            int targetLength = target.Length;
            int sourceLength = source.Length;

            for (int targetY = 0; targetY < targetHeight; targetY++)
            {
                sourceY = Mathf.RoundToInt(targetY * scaleY);

                for (int targetX = 0; targetX < targetHeight; targetX++)
                {
                    sourceX = Mathf.RoundToInt(targetX * scaleX);

                    int sourcePos = sourceY * sourceStride + sourceX;
                    int targetPos = targetY * targetStride + targetX;

                    if (targetPos < targetLength && sourcePos < sourceLength)
                    {
                        target[targetPos] = source[sourcePos];
                    }
                }
            }
            return target;
        }

        public static void SubSample(ref Color[] source, int sourceStride, int sourceHeight, ref double[,] target, int targetStride, int targetHeight)
        {
            bool hasAlpha = target.GetLength(1) == 4;
            int sourceY = 0;
            int sourceX = 0;
            float scaleY = (float) sourceHeight / targetHeight;
            float scaleX = (float) sourceStride / targetStride;
            
            int targetLength = target.GetLength(0);
            int sourceLength = source.Length;

            for (int targetY=0; targetY < targetHeight; targetY++)
            {
                sourceY = Mathf.RoundToInt(targetY * scaleY);

                for (int targetX=0; targetX < targetHeight; targetX++)
                {
                    sourceX = Mathf.RoundToInt(targetX * scaleX);

                    int sourcePos = sourceY * sourceStride + sourceX;
                    int targetPos = targetY * targetStride + targetX;

                    if (targetPos < targetLength && sourcePos < sourceLength)
                    {
                        target[targetPos, 0] = source[sourcePos].r;
                        target[targetPos, 1] = source[sourcePos].g;
                        target[targetPos, 2] = source[sourcePos].b;
                        if (hasAlpha)
                        {
                            target[targetPos, 3] = source[sourcePos].a;
                        }
                    }

                }


            }
        }

        public static double[,] Texture2Double(Texture2D tex, bool useAlpha=false)
        {
            Color[] colors = tex.GetPixels();
            double[,] I = new double[colors.Length, useAlpha ? 4 : 3];
            Color2Double(ref colors, ref I);
            return I;
        }

        public static double[,] Texture2Double(Texture2D tex, int targetWidth, int targetHeight, bool useAlpha)
        {
            Color[] colors = tex.GetPixels();
            double[,] I = new double[targetWidth * targetHeight, useAlpha ? 4 : 3];
            Color2Double(ref colors, ref I, tex.width, targetWidth, (targetHeight - tex.height) / 2);
            return I;
        }

        public static void Color2Double(ref Color[] colors, ref double[,] I, int sourceStride, int targetStride, int targetYOffset)
        {
            int sourceHeight = colors.Length / sourceStride;
            int targetXOffset = (targetStride - sourceStride) / 2;

            bool hasAlpha = I.GetLength(1) == 4;
            for (int y=0; y<sourceHeight; y++)
            {
                for (int x=0; x<sourceStride; x++)
                {
                    int sourcePos = x + y * sourceStride;
                    int targetPos = (x + targetXOffset) + (y + targetYOffset) * targetStride;
                    I[targetPos, 0] = colors[sourcePos].r;
                    I[targetPos, 1] = colors[sourcePos].g;
                    I[targetPos, 2] = colors[sourcePos].b;
                    if (hasAlpha)
                    {
                        I[targetPos, 3] = colors[sourcePos].a;
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

        public static void WebCam2Double(WebCamTexture camTex, ref double[,] target, int targetStride, float digitalZoom=0)
        {            
            ZoomCropConvert(
                camTex.GetPixels,
                camTex.width, 
                camTex.height, 
                ref target, 
                targetStride, 
                digitalZoom);
        }

        public static void Texture2Double(Texture2D source, ref double[,] target, int targetStride, float digitalZoom=0)
        {
            ZoomCropConvert(
                source.GetPixels,
                source.width,
                source.height,
                ref target,
                targetStride,
                digitalZoom);
        }

        public static void ZoomCropConvert(Func<int, int, int, int, Color[]> pixelFunc, int sourceWidth, int sourceHeight, ref double[,] target, int targetStride, float digitalZoom=0)
        {
            int targetHeight = target.GetLength(0) / targetStride;

            Rect zoomRect = GetZoomRect(sourceWidth, sourceHeight, targetStride, targetHeight, digitalZoom);

            Color[] pixels = pixelFunc((int) zoomRect.position.x, (int) zoomRect.position.y, (int) zoomRect.size.x, (int) zoomRect.size.y);
            SubSample(ref pixels, (int) zoomRect.size.x, (int) zoomRect.size.y, ref target, targetStride, targetHeight);

        }

        public static Rect GetZoomRect(int sourceWidth, int sourceHeight, int targetStride, int targetHeight, float digitalZoom=0)
        {
            int sourceStride;
            float aspect = (float)targetStride / targetHeight;
            if (sourceWidth / (float)targetStride > sourceHeight / (float)targetHeight)
            {
                sourceStride = Mathf.FloorToInt(aspect * sourceHeight);
            }
            else
            {
                sourceStride = sourceWidth;

            }

            sourceStride = Mathf.Min(sourceWidth, Mathf.RoundToInt(targetStride / digitalZoom));
            int sourceRecalcHeight = Mathf.Min(sourceHeight, Mathf.FloorToInt(sourceStride / aspect));
            sourceStride = Mathf.FloorToInt(sourceRecalcHeight * aspect);

            return new Rect((sourceWidth - sourceStride) / 2, (sourceHeight - sourceRecalcHeight) / 2, sourceStride, sourceRecalcHeight);
        }

    }

}