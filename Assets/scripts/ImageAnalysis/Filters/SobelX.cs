namespace ImageAnalysis.Filters
{
    public class SobelX : Filter
    {

        private double[,] kernel = new double[,] 
        { 
            {-1, 0, 1},
            {-2, 0, 2},
            {-1, 0, 1}
        };

        public override double Bias
        {
            get
            {
                return 0;                
            }
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
                return "Sobel X";
            }
        }

        public override double Scale
        {
            get
            {
                return 1;
            }
        }
    }
}