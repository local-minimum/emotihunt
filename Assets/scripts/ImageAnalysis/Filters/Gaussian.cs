
namespace ImageAnalysis.Filters
{
    public abstract class Gaussian : Filter
    {        
        private float scale = -1;
        public float GetValue(int x, int y, float sigma)
        {
            return (float) (1.0 / (2 * System.Math.PI * System.Math.Pow(sigma, 2)) * System.Math.Exp(-(System.Math.Pow(x, 2) + System.Math.Pow(y, 2))/(2 * System.Math.Pow(sigma, 2))));
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