using UnityEngine;
using System.Collections.Generic;

namespace ImageAnalysis.Textures
{
    public class HarrisCornerTexture : ConvolvableTexture
    {
        Filter sobelX = Filter.Get<Filters.SobelX>();
        Filter sobelY = Filter.Get<Filters.SobelY>();
        Filter gauss3Filter = Filter.Get<Filters.Gaussian9x9S3>();

		double[,] Gx2;
		double[,] Gy2;
		double[,] Gxy;
		double[,] Sx2;
		double[,] Sy2;
		double[,] Sxy;

        double[,] edgeX;
        double[,] edgeY;
        double[,] response;

        int filt1stride;
        int filt1height;

        public int width;
        public int height;

        int filt2stride;
        int filt2height;
        double kappa = 0.1;
        double threshold = 0.3;
        
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

        public HarrisCornerTexture(Texture2D texture, float kappa, bool useAlpha=false, bool pad=false) : base(texture)
        {
            Kappa = kappa;
            ConstructHelper(texture, useAlpha, pad);
        }


        public HarrisCornerTexture(Texture2D texture, bool useAlpha=false, bool pad=false) : base(texture)
        {
            ConstructHelper(texture, useAlpha, pad);
        }

        void ConstructHelper(Texture2D texture, bool useAlpha, bool pad) {

            width = texture.width;
            height = texture.height;

            if (pad)
            {
                width += sobelX.Kernel.GetLength(1) - 1;
                width += gauss3Filter.Kernel.GetLength(1) - 1;

                height += sobelX.Kernel.GetLength(0) - 1;
                height += gauss3Filter.Kernel.GetLength(0) - 1;

            }

            int channels = useAlpha ? 4 : 3;

            filt1stride = width - sobelX.Kernel.GetLength(1) + 1;
            filt1height = height - sobelX.Kernel.GetLength(0) + 1;
            int filt1size = filt1height * filt1stride;

            edgeX = new double[filt1size, channels];
            edgeY = new double[filt1size, channels];

			Sx2 = new double[filt1size, channels];
			Sy2 = new double[filt1size, channels]; 
			Sxy = new double[filt1size, channels];

            filt2stride = filt1stride - gauss3Filter.Kernel.GetLength(1) + 1;
            filt2height = filt1height - gauss3Filter.Kernel.GetLength(0) + 1;

            int filt2size = filt2height * filt2stride;

            Gx2 = new double[filt2size, channels];
            Gy2 = new double[filt2size, channels];
			Gxy = new double[filt2size, channels];

            
            response = new double[filt2size, channels];
        }	

        public override IEnumerable<float> Convolve(double[,] data, int stride)
        {

            ImageAnalysis.Convolve.Valid(ref data, stride, ref edgeX, filt1stride, sobelX);
            yield return 0.15f;

            ImageAnalysis.Convolve.Valid(ref data, stride, ref edgeY, filt1stride, sobelY);
            yield return 0.3f;

			ImageAnalysis.Convolve.Pow (ref edgeX, 2, ref Sx2);
			yield return 0.32f;

			ImageAnalysis.Convolve.Pow (ref edgeY, 2, ref Sy2);
			yield return 0.34f;

			ImageAnalysis.Convolve.Multiply(ref edgeX, ref edgeY, ref Sxy);
			yield return 0.36f;

            ImageAnalysis.Convolve.Valid(ref Sx2, filt1stride, ref Gx2, filt2stride, gauss3Filter);
            yield return 0.45f;

			ImageAnalysis.Convolve.Valid(ref Sy2, filt1stride, ref Gy2, filt2stride, gauss3Filter);
            yield return 0.5f;

			ImageAnalysis.Convolve.Valid(ref Sxy, filt1stride, ref Gxy, filt2stride, gauss3Filter);
			yield return 0.65f;

			ImageAnalysis.Convolve.Response(ref Gx2, ref Gy2, ref Gxy, kappa, ref response);
            yield return 0.9f;

            Math.ValueScale01(ref response);
            yield return 0.95f;

            ImageAnalysis.Convolve.Convert(ref response, filt2stride, filt2height, Texture.width, ref target);
            yield return 1f;
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
