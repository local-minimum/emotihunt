using UnityEngine;


namespace ImageAnalysis.Textures
{
    public class EdgeTexture : ConvolvableTexture
    {
        Filter sobelX = Filter.Get<Filters.SobelX>();
        Filter sobelY = Filter.Get<Filters.SobelY>();
        double[,] xEdges;
        double[,] yEdges;
        double[,] edges;
        int edgeStride;
        int edgeHeight;

        public EdgeTexture(Texture2D texture) : base(texture)
        {
            edgeStride = texture.width - sobelX.Kernel.GetLength(1) + 1;
            edgeHeight = texture.height - sobelX.Kernel.GetLength(0) + 1;
            int edgeSizes = edgeHeight * edgeStride;
            xEdges = new double[edgeSizes, 3];
            yEdges = new double[edgeSizes, 3];
            edges = new double[edgeSizes, 3];
        }

        public override void Convolve(double[,] data, int stride)
        {
                        
            ImageAnalysis.Convolve.Valid(ref data, stride, ref xEdges, edgeStride, sobelX);
            ImageAnalysis.Convolve.Valid(ref data, stride, ref yEdges, edgeStride, sobelY);
            ImageAnalysis.Convolve.Add(ref xEdges, ref yEdges, ref edges);
            ImageAnalysis.Convolve.Convert(ref edges, edgeStride, edgeHeight, Texture.width, ref target);

        }
    }
}