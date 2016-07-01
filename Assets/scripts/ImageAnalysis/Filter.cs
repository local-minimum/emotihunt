using System.Collections.Generic;
using System;


namespace ImageAnalysis
{

    public abstract class Filter
    {

        private static Dictionary<Type, Filter> instances = new Dictionary<Type, Filter>();

        private static readonly object padlock = new object();

        public static T Get<T>() where T: Filter 
        {
            lock (padlock)
            {
                if (!instances.ContainsKey(typeof(T)))
                {
                    instances.Add(typeof(T), Activator.CreateInstance<T>());
                }
            }
            return (T) instances[typeof(T)];
        }

        public abstract string Name
        {
            get;
        }

        public abstract double Scale
        {

            get;
        }

        public abstract double Bias
        {
            get;
        }

        public abstract double[,] Kernel
        {
            get;
        }
    }

}
