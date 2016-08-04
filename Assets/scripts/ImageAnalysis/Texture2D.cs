using UnityEngine;
using System.Collections.Generic;

namespace ImageAnalysis
{
    public static class Texture2DConvolve {
        public static Texture2D Convolve<T>(this Texture2D sourceTex) where T : Filter
        {
            Color[] data = sourceTex.GetPixels();
            double[,] I = new double[data.Length, 3];

            ImageAnalysis.Convolve.Color2Double(ref data, ref I);

            Texture2D outTex = new Texture2D(sourceTex.width, sourceTex.height);
            double[,] convolution = ImageAnalysis.Convolve.Valid<T>(ref I, sourceTex.width);

            outTex.SetPixels(ImageAnalysis.Convolve.OriginalSize<T>(ref convolution, sourceTex.width));
            outTex.Apply();
            return outTex;


        }
        
        public static Texture2D Edges(this Texture2D sourceTex)
        {
            Color[] data = sourceTex.GetPixels();
            double[,] I = new double[data.Length, 3];
            ImageAnalysis.Convolve.Color2Double(ref data, ref I);

            Texture2D outTex = new Texture2D(sourceTex.width, sourceTex.height);
            double[,] Xedges = ImageAnalysis.Convolve.Valid<Filters.SobelX>(ref I, sourceTex.width);
            double[,] Yedges = ImageAnalysis.Convolve.Valid<Filters.SobelY>(ref I, sourceTex.width);
            double[,] convolution = ImageAnalysis.Convolve.Add(ref Xedges, ref Yedges);

            outTex.SetPixels(ImageAnalysis.Convolve.OriginalSize<Filters.SobelX>(ref convolution, sourceTex.width));
            outTex.Apply();
            return outTex;

        }

        public static void SetEdges(this Texture2D sourceTex, ref Color[] data)
        {
            double[,] I = new double[data.Length, 3];
            ImageAnalysis.Convolve.Color2Double(ref data, ref I);
            double[,] Xedges = ImageAnalysis.Convolve.Valid<Filters.SobelX>(ref I, sourceTex.width);
            double[,] Yedges = ImageAnalysis.Convolve.Valid<Filters.SobelY>(ref I, sourceTex.width);
            double[,] edges = ImageAnalysis.Convolve.Add(ref Xedges, ref Yedges);

            sourceTex.SetPixels(ImageAnalysis.Convolve.OriginalSize<Filters.SobelX>(ref edges, sourceTex.width));

            sourceTex.Apply();
        }

        public static void SetDifferenceOfGaussians(this Texture2D sourceTex, Color[] data)
        {
            double[,] I = new double[data.Length, 3];
            ImageAnalysis.Convolve.Color2Double(ref data, ref I);
            double[,] gauss1 = ImageAnalysis.Convolve.Valid<Filters.Gaussian5x5S1>(ref I, sourceTex.width);
            double[,] gauss2 = ImageAnalysis.Convolve.Valid<Filters.Gaussian5x5S3>(ref I, sourceTex.width);
            double[,] DoG = ImageAnalysis.Convolve.Subtract(ref gauss1, ref gauss2);
            Color[] resized = ImageAnalysis.Convolve.OriginalSize<Filters.Gaussian5x5S1>(ref DoG, sourceTex.width);

            sourceTex.SetPixels(resized);
            sourceTex.Apply();
        }

    }

    [System.Serializable]
    public abstract class ConvolvableTexture
    {
        Texture2D texture;
        protected Color[] target;

        public ConvolvableTexture(Texture2D texture)
        {
            this.texture = texture;
            target = new Color[texture.width * texture.height];
        }

        public Texture2D Texture
        {
            get
            {
                return texture;
            }

        }

        abstract public IEnumerable<float> Convolve(double[,] data, int stride);

        public void ConvolveAndApply(double[,] data, int stride)
        {
            Convolve(data, stride);
            ApplyTargetToTexture(texture);
        }

        public void ApplyTargetToTexture(Texture2D tex)
        {
            tex.SetPixels(target);
            tex.Apply();
        }

        public void SetPixelsVisible()
        {
            for (int i=0; i<target.Length; i++)
            {
                if (target[i].r > 0 || target[i].g > 0 || target[i].b > 0 || target[i].a > 0)
                {
                    target[i].a = 1f;
                }
            }
        }
    }
}