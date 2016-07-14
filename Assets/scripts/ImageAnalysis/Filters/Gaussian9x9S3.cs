namespace ImageAnalysis.Filters
{
    public class Gaussian9x9S3 : Gaussian
    {
        double[,] kernel = new double[9, 9];

        public Gaussian9x9S3()
        {
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    kernel[y, x] = GetValue(x, y, 3.0f);
                }
            }
            SetScale();
        }

        public override double[,] Kernel
        {
            get
            {
                return kernel;
            }
        }

        public override string Name
        {
            get
            {
                return "Gaussian 9x9 Kernel, Sigma 3.0";
            }
        }
    }
}