using UnityEngine;


namespace ImageAnalysis.Textures
{
    public class HarrisCornerTexture : ConvolvableTexture
    {
        Filter sobelX = Filter.Get<Filters.SobelX>();
        Filter sobelY = Filter.Get<Filters.SobelY>();
        Filter gauss3Filter = Filter.Get<Filters.Gaussian5x5S3>();

        Color[] gauss3;
        Color[] edgeX;
        Color[] edgeY;
        Color[,,] A;
        Color[] response;

        int gaussStride;
        int gaussHeight;

        int sobelStride;
        int sobelHeight;
        float kappa = 0.1f;
        float threshold = 0.7f;

        public float Kappa { get { return kappa; } set { kappa = Mathf.Clamp(value, 0.04f, 0.15f);} }


        public HarrisCornerTexture(Texture2D texture, float kappa) : base(texture)
        {
            Kappa = kappa;
            gaussStride = texture.width - gauss3Filter.Kernel.GetLength(1) + 1;
            gaussHeight = texture.height - gauss3Filter.Kernel.GetLength(0) + 1;
            gauss3 = new Color[gaussHeight * gaussStride];

            sobelStride = gaussStride - sobelX.Kernel.GetLength(1) + 1;
            sobelHeight = gaussHeight - sobelX.Kernel.GetLength(0) + 1;

            int edgeSizes = gaussHeight * gaussStride;
            edgeX = new Color[edgeSizes];
            edgeY = new Color[edgeSizes];
            A = new Color[edgeSizes, 2, 2];
            response = new Color[edgeSizes];
        }


        public HarrisCornerTexture(Texture2D texture) : base(texture)
        {
            gaussStride = texture.width - gauss3Filter.Kernel.GetLength(1) + 1;
            gaussHeight = texture.height - gauss3Filter.Kernel.GetLength(0) + 1;
            gauss3 = new Color[gaussHeight * gaussStride];

            sobelStride = gaussStride - sobelX.Kernel.GetLength(1) + 1;
            sobelHeight = gaussHeight - sobelX.Kernel.GetLength(0) + 1;

            int edgeSizes = gaussHeight * gaussStride;
            edgeX = new Color[edgeSizes];
            edgeY = new Color[edgeSizes];
            A = new Color[edgeSizes, 2, 2];
            response = new Color[edgeSizes];
        }

        protected override void _Convolve(Color[] data, int stride)
        {

            ImageAnalysis.Convolve.Valid(data, stride, ref gauss3, gaussStride, gauss3Filter);

            ImageAnalysis.Convolve.Valid(gauss3, gaussStride, ref edgeX, sobelStride, sobelX);
            ImageAnalysis.Convolve.Valid(gauss3, gaussStride, ref edgeY, sobelStride, sobelY);
            ImageAnalysis.Convolve.TensorMatrix(edgeX, edgeY, ref A);
            ImageAnalysis.Convolve.Response(A, kappa, ref response);

            Color tColor = ImageAnalysis.Convolve.Max(response) * threshold;

            Debug.Log(tColor);

            ImageAnalysis.Convolve.Threshold(ref response, tColor);

            ImageAnalysis.Convolve.Resize(response, gaussStride, gaussHeight, Texture.width, ref target);

        }
    }
}
