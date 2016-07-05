using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ImageAnalysis
{
    public static class Math
    {

        public static int[,] ArgSort(ref double[,] I)
        {
            int[,] args = new int[I.GetLength(0), I.GetLength(1)];

            for (int color=0, colors = I.GetLength(1); color<colors; color++)
            {
                WriteIndicesTo(ref args,
                    GetChannelAsEnumerable(I, color).Select((x, i) => new KeyValuePair<int, double>(i, x)).OrderBy(x => x.Value).Select(x => x.Key).ToArray(),
                    color);
            } 
            
            return args;
        }

        public static IEnumerable<double> GetChannelAsEnumerable(double[,] I, int color)
        {
            for (int i=0, l = I.GetLength(0); i< l; i++)
            {
                yield return I[i, color];
            }
        }

        public static void WriteIndicesTo(ref int[,] arr, int[] indices, int color)
        {
            for (int i = 0, l = arr.GetLength(0); i < l; i++)
            {
                arr[i, color] = indices[i];
                
            }
        }
    }
}