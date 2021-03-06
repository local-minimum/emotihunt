﻿using UnityEngine;
using System.Collections.Generic;

namespace ImageAnalysis.Textures
{
    public class DoGTexture : ConvolvableTexture
    {
        Filter gauss1Filter = Filter.Get<Filters.Gaussian5x5S1>();
        Filter gauss3Filter = Filter.Get<Filters.Gaussian5x5S3>();
        double[,] gauss1;
        double[,] gauss3;
        double[,] DoG;
        int gaussStride;
        int gaussHeight;

        public DoGTexture(Texture2D texture) : base(texture)
        {
            gaussStride = texture.width - gauss1Filter.Kernel.GetLength(1) + 1;
            gaussHeight = texture.height - gauss1Filter.Kernel.GetLength(0) + 1;
            int gaussSizes = gaussHeight * gaussStride;
            gauss1 = new double[gaussSizes, 3];
            gauss3 = new double[gaussSizes, 3];
            DoG = new double[gaussSizes, 3];
        }

        public override IEnumerable<float> Convolve(double[,] data, int stride)
        {

            ImageAnalysis.Convolve.Valid(ref data, stride, ref gauss1, gaussStride, this.gauss1Filter);
            yield return 0.4f;
            ImageAnalysis.Convolve.Valid(ref data, stride, ref gauss3, gaussStride, gauss3Filter);
            yield return 0.8f;
            ImageAnalysis.Convolve.Subtract(ref gauss1, ref gauss3, ref DoG);
            yield return 0.9f;
            ImageAnalysis.Convolve.Convert(ref DoG, gaussStride, gaussHeight, Texture.width, ref target);
            yield return 1.0f;
        }
    }
}