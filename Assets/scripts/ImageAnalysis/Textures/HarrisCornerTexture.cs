using UnityEngine;
using System.Collections.Generic;

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

            ImageAnalysis.Convolve.Response(ref A, kappa, ref response);

            Math.ValueScale01(ref response);
            ImageAnalysis.Convolve.Convert(ref response, filt2stride, filt2height, Texture.width, ref target);

        }

        public int[,] LocateCorners(int nCorners, double aheadCost, int minDistance)
        {

            int[,] sortOrder = Math.ArgSort(ref response);
            int[,] corners = Math.FlexibleTake(ref response, ref sortOrder, nCorners, aheadCost, filt2stride, minDistance);
            return corners;
        }

        public Coordinate[] LocateCornersAsCoordinates(int nCorners, double aheadCost, int minDistance, int offset=0)
        {
            int[,] corners = LocateCorners(nCorners, aheadCost, minDistance);
            return Math.ConvertCoordinate(corners, filt2stride, offset);
        }
    }
}
