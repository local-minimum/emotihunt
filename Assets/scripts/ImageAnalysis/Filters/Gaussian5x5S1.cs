namespace ImageAnalysis.Filters
{
    public class Gaussian5x5S1 : Gaussian
    {
        double[,] kernel = new double[5,5];

        public Gaussian5x5S1()
        {            
            for (int y=0;y<5;y++)
            {
                for (int x=0; x<5;x++)
                {
                    kernel[y, x] = GetValue(x, y, 1.0f);
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
                return "Gaussian 5x5 Kernel, Sigma 1.0";
            }
        }
    }
}