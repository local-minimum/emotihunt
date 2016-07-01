using UnityEngine;


namespace ImageAnalysis.Textures
{
    public class DoGTexture : ConvolvableTexture
    {
        Filter gauss1Filter = Filter.Get<Filters.Gaussian5x5S1>();
        Filter gauss3Filter = Filter.Get<Filters.Gaussian5x5S3>();
        Color[] gauss1;
        Color[] gauss3;
        Color[] DoG;
        int gaussStride;
        int gaussHeight;

        public DoGTexture(Texture2D texture) : base(texture)
        {
            gaussStride = texture.width - gauss1Filter.Kernel.GetLength(1) + 1;
            gaussHeight = texture.height - gauss1Filter.Kernel.GetLength(0) + 1;
            int gaussSizes = gaussHeight * gaussStride;
            gauss1 = new Color[gaussSizes];
            gauss3 = new Color[gaussSizes];
            DoG = new Color[gaussSizes];
        }

        protected override void _Convolve(Color[] data, int stride)
        {

            ImageAnalysis.Convolve.Valid(data, stride, ref this.gauss1, gaussStride, this.gauss1Filter);
            ImageAnalysis.Convolve.Valid(data, stride, ref gauss3, gaussStride, gauss3Filter);
            ImageAnalysis.Convolve.Subtract(this.gauss1, gauss3, ref this.DoG);
            ImageAnalysis.Convolve.Resize(this.DoG, gaussStride, gaussHeight, Texture.width, ref target);

        }
    }
}