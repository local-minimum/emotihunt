using UnityEngine;
using System.Collections;
using System;

namespace ImageAnalysis.Filters
{
    public abstract class Gaussian : Filter
    {        
        private float scale = -1;
        public float GetValue(int x, int y, float sigma)
        {
            return 1.0f / (2 * Mathf.PI * Mathf.Pow(sigma, 2)) * Mathf.Exp(-(Mathf.Pow(x, 2) + Mathf.Pow(y, 2))/(2 * Mathf.Pow(sigma, 2)));
        }

        protected void SetScale()
        {
            scale = 0;
            int width = Kernel.GetLength(1);
            for (int y = 0, height = Kernel.GetLength(0); y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    scale += (float)Kernel[y, x];
                }

            }
            scale = 1 / scale;
        }

        public override double Bias
        {
            get
            {
                return 0f;
            }
        }

        public override double Scale
        {
            get
            {
                return scale;
            }
        }

    }
}