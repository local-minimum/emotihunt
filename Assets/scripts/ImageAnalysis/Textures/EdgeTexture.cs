using UnityEngine;
using System.Collections.Generic;

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

        public override IEnumerable<float> Convolve(double[,] data, int stride)
        {
            
            ImageAnalysis.Convolve.Valid(ref data, stride, ref xEdges, edgeStride, sobelX);
            yield return 0.4f;
            ImageAnalysis.Convolve.Valid(ref data, stride, ref yEdges, edgeStride, sobelY);
            yield return 0.8f;
            ImageAnalysis.Convolve.Add(ref xEdges, ref yEdges, ref edges);
            yield return 0.9f;
            ImageAnalysis.Convolve.Convert(ref edges, edgeStride, edgeHeight, Texture.width, ref target);
            yield return 1.0f;

        }
    }
}