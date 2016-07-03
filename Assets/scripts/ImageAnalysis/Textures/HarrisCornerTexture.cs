using UnityEngine;


namespace ImageAnalysis.Textures
{
    public class HarrisCornerTexture : ConvolvableTexture
    {
        Filter sobelX = Filter.Get<Filters.SobelX>();
        Filter sobelY = Filter.Get<Filters.SobelY>();
        Filter gauss3Filter = Filter.Get<Filters.Gaussian5x5S3>();

        double[,] gauss3;
        double[,] edgeX;
        double[,] edgeY;
        double[,,,] A;
        double[,] response;

        int gaussStride;
        int gaussHeight;

        int sobelStride;
        int sobelHeight;
        double kappa = 0.1;
        double threshold = 0.7;

        public double Kappa { get { return kappa; } set { kappa = Mathf.Clamp((float) value, 0.04f, 0.15f);} }

        public HarrisCornerTexture(Texture2D texture, float kappa) : base(texture)
        {
            Kappa = kappa;
            gaussStride = texture.width - gauss3Filter.Kernel.GetLength(1) + 1;
            gaussHeight = texture.height - gauss3Filter.Kernel.GetLength(0) + 1;
            gauss3 = new double[gaussHeight * gaussStride, 3];

            sobelStride = gaussStride - sobelX.Kernel.GetLength(1) + 1;
            sobelHeight = gaussHeight - sobelX.Kernel.GetLength(0) + 1;

            int edgeSizes = gaussHeight * gaussStride;
            edgeX = new double[edgeSizes, 3];
            edgeY = new double[edgeSizes, 3];
            A = new double[edgeSizes, 3, 2, 2];
            response = new double[edgeSizes, 3];
        }


        public HarrisCornerTexture(Texture2D texture) : base(texture)
        {
            gaussStride = texture.width - gauss3Filter.Kernel.GetLength(1) + 1;
            gaussHeight = texture.height - gauss3Filter.Kernel.GetLength(0) + 1;
            gauss3 = new double[gaussHeight * gaussStride, 3];

            sobelStride = gaussStride - sobelX.Kernel.GetLength(1) + 1;
            sobelHeight = gaussHeight - sobelX.Kernel.GetLength(0) + 1;

            int edgeSizes = gaussHeight * gaussStride;
            edgeX = new double[edgeSizes, 3];
            edgeY = new double[edgeSizes, 3];
            A = new double[edgeSizes, 3, 2, 2];
            response = new double[edgeSizes, 3];
        }

        protected override void _Convolve(double[,] data, int stride)
        {

            ImageAnalysis.Convolve.Valid(ref data, stride, ref gauss3, gaussStride, gauss3Filter);

            ImageAnalysis.Convolve.Valid(ref gauss3, gaussStride, ref edgeX, sobelStride, sobelX);
            ImageAnalysis.Convolve.Valid(ref gauss3, gaussStride, ref edgeY, sobelStride, sobelY);
            ImageAnalysis.Convolve.TensorMatrix(ref edgeX, ref edgeY, ref A);

            //int i = 400;
            //Debug.Log("Ix " + edgeX[i]);
            //Debug.Log("Iy " + edgeY[i]);
            //Debug.Log("A0,0 " + A[i, 0, 0]);
            //Debug.Log("A0,1 " + A[i, 0, 1]);
            //Debug.Log("A1,0 " + A[i, 1, 0]);
            //Debug.Log("A1,1 " + A[i, 1, 1]);

            ImageAnalysis.Convolve.Response(ref A, kappa, ref response);

            //Debug.Log("R " + response[200]);

            double[] colorThresholds = ImageAnalysis.Convolve.Max(ref response);
            for (int color=0; color<colorThresholds.Length; color++)
            {
                colorThresholds[color] *= threshold;
            }

            //Debug.Log(colorThresholds);

            ImageAnalysis.Convolve.ThresholdInplace(ref response, colorThresholds);

            ImageAnalysis.Convolve.Convert(ref response, sobelStride, gaussHeight, Texture.width, ref target);

        }
    }
}
