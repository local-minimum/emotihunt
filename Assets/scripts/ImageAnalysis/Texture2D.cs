using UnityEngine;

namespace ImageAnalysis
{
    public static class Texture2DConvolve {
        public static Texture2D Convolve<T>(this Texture2D sourceTex) where T : Filter
        {
            Color[] data = sourceTex.GetPixels();

            Texture2D outTex = new Texture2D(sourceTex.width, sourceTex.height);
            
            outTex.SetPixels(ImageAnalysis.Convolve.Valid<T>(data, sourceTex.width));
            outTex.Apply();
            return outTex;


        }
        
        public static Texture2D Edges(this Texture2D sourceTex)
        {
            Color[] data = sourceTex.GetPixels();

            Texture2D outTex = new Texture2D(sourceTex.width, sourceTex.height);
            Color[] Xedges = ImageAnalysis.Convolve.Valid<Filters.SobelX>(data, sourceTex.width);
            Color[] Yedges = ImageAnalysis.Convolve.Valid<Filters.SobelY>(data, sourceTex.width);

            outTex.SetPixels(ImageAnalysis.Convolve.Add(Xedges, Yedges));
            outTex.Apply();
            return outTex;

        }

        public static void SetEdges(this Texture2D sourceTex, Color[] data)
        {
            Color[] Xedges = ImageAnalysis.Convolve.Valid<Filters.SobelX>(data, sourceTex.width);
            Color[] Yedges = ImageAnalysis.Convolve.Valid<Filters.SobelY>(data, sourceTex.width);
            Color[] edges = ImageAnalysis.Convolve.Add(Xedges, Yedges);
            Color[] resized = ImageAnalysis.Convolve.OriginalSize<Filters.SobelX>(edges, sourceTex.width);
            
            sourceTex.SetPixels(resized);
            sourceTex.Apply();
        }

        public static void SetDifferenceOfGaussians(this Texture2D sourceTex, Color[] data)
        {
            Color[] gauss1 = ImageAnalysis.Convolve.Valid<Filters.Gaussian5x5S1>(data, sourceTex.width);
            Color[] gauss2 = ImageAnalysis.Convolve.Valid<Filters.Gaussian5x5S3>(data, sourceTex.width);
            Color[] DoG = ImageAnalysis.Convolve.Subtract(gauss1, gauss2);
            Color[] resized = ImageAnalysis.Convolve.OriginalSize<Filters.Gaussian5x5S1>(DoG, sourceTex.width);

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

        abstract protected void _Convolve(Color[] data, int stride);

        public void Convolve(Color[] data, int stride)
        {
            _Convolve(data, stride);
            texture.SetPixels(target);
            texture.Apply();
        }
    }
}