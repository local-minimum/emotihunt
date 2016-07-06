using UnityEngine;


namespace ImageAnalysis.Textures
{
    public class HarrisCornerTexture : ConvolvableTexture
    {
        Filter sobelX = Filter.Get<Filters.SobelX>();
        Filter sobelY = Filter.Get<Filters.SobelY>();
        Filter gauss3Filter = Filter.Get<Filters.Gaussian5x5S3>();

        double[,] gaussX;
        double[,] gaussY;
        double[,] edgeX;
        double[,] edgeY;
        double[,,,] A;
        double[,] response;

        int filt1stride;
        int filt1height;

        int filt2stride;
        int filt2height;
        double kappa = 0.1;
        double threshold = 0.7;
        
        public double Kappa { get { return kappa; } set { kappa = Mathf.Clamp((float) value, 0.04f, 0.15f);} }
        public double Threshold { get { return threshold;} set { threshold = Mathf.Clamp01((float) value); } }

        public double[,] Response
        {
            get
            {
                return response;
            }
        }

        public int ResponseStride
        {
            get
            {
                return filt2stride;
            }
        }

        public HarrisCornerTexture(Texture2D texture, float kappa) : base(texture)
        {
            Kappa = kappa;
            ConstructHelper(texture);
        }


        public HarrisCornerTexture(Texture2D texture) : base(texture)
        {
            ConstructHelper(texture);
        }

        void ConstructHelper(Texture2D texture) { 
            filt1stride = texture.width - sobelX.Kernel.GetLength(1) + 1;
            filt1height = texture.height - sobelX.Kernel.GetLength(0) + 1;
            int filt1size = filt1height * filt1stride;

            edgeX = new double[filt1size, 3];
            edgeY = new double[filt1size, 3];


            filt2stride = filt1stride - gauss3Filter.Kernel.GetLength(1) + 1;
            filt2height = filt1height - gauss3Filter.Kernel.GetLength(0) + 1;

            int filt2size = filt2height * filt2stride;
            gaussX = new double[filt2size, 3];
            gaussY = new double[filt2size, 3];

            A = new double[filt2size, 3, 2, 2];
            response = new double[filt2size, 3];
        }

        public override void Convolve(double[,] data, int stride)
        {

            ImageAnalysis.Convolve.Valid(ref data, stride, ref edgeX, filt1stride, sobelX);
            ImageAnalysis.Convolve.Valid(ref data, stride, ref edgeY, filt1stride, sobelY);

            ImageAnalysis.Convolve.Valid(ref edgeX, filt1stride, ref gaussX, filt2stride, gauss3Filter);
            ImageAnalysis.Convolve.Valid(ref edgeY, filt1stride, ref gaussY, filt2stride, gauss3Filter);

            ImageAnalysis.Convolve.TensorMatrix(ref gaussX, ref gaussY, ref A);

            /*
            int i = 400;
            int c = 1;
            Debug.Log("Ix " + edgeX[i, c]);
            Debug.Log("Iy " + edgeY[i, c]);
            Debug.Log("A0,0 " + A[i, c, 0, 0]);
            Debug.Log("A0,1 " + A[i, c, 0, 1]);
            Debug.Log("A1,0 " + A[i, c, 1, 0]);
            Debug.Log("A1,1 " + A[i, c, 1, 1]);
            */
            ImageAnalysis.Convolve.Response(ref A, kappa, ref response);

            /*
            Debug.Log("R " + response[i, c] + " kappa " + kappa);

            double[] colorThresholds = ImageAnalysis.Convolve.Max(ref response);
            for (int color=0; color<colorThresholds.Length; color++)
            {
                colorThresholds[color] *= threshold;
            }

            Debug.Log(colorThresholds);

            ImageAnalysis.Convolve.ThresholdInplace(ref response, colorThresholds);
            */
            Math.ValueScale01(ref response);
            ImageAnalysis.Convolve.Convert(ref response, filt2stride, filt2height, Texture.width, ref target);

        }
    }
}
