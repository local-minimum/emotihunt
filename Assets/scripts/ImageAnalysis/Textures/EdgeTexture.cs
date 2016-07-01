using UnityEngine;


namespace ImageAnalysis.Textures
{
    public class EdgeTexture : ConvolvableTexture
    {
        Filter sobelX = Filter.Get<Filters.SobelX>();
        Filter sobelY = Filter.Get<Filters.SobelY>();
        Color[] xEdges;
        Color[] yEdges;
        Color[] edges;
        int edgeStride;
        int edgeHeight;

        public EdgeTexture(Texture2D texture) : base(texture)
        {
            edgeStride = texture.width - sobelX.Kernel.GetLength(1) + 1;
            edgeHeight = texture.height - sobelX.Kernel.GetLength(0) + 1;
            int edgeSizes = edgeHeight * edgeStride;
            xEdges = new Color[edgeSizes];
            yEdges = new Color[edgeSizes];
            edges = new Color[edgeSizes];
        }

        protected override void _Convolve(Color[] data, int stride)
        {
                        
            ImageAnalysis.Convolve.Valid(data, stride, ref xEdges, edgeStride, sobelX);
            ImageAnalysis.Convolve.Valid(data, stride, ref yEdges, edgeStride, sobelY);
            ImageAnalysis.Convolve.Add(xEdges, yEdges, ref edges);
            ImageAnalysis.Convolve.Resize(edges, edgeStride, edgeHeight, Texture.width, ref target);

        }
    }
}