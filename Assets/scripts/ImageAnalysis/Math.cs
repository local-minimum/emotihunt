using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ImageAnalysis
{
    [Serializable]
    public struct Coordinate
    {
        public int x;
        public int y;
        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

    }

    public static class Math
    {

        public static int[,] ArgSort(ref double[,] I)
        {
            int[,] args = new int[I.GetLength(0), I.GetLength(1)];

            for (int color = 0, colors = I.GetLength(1); color < colors; color++)
            {
                WriteIndicesTo(ref args,
                    GetChannelAsEnumerable(I, color)
                        .Select((v, i) => new { Value = v, Index = i })
                        .OrderByDescending(n => n.Value)
                        .Select(n => n.Index)
                        .ToArray(),
                    color);
            }

            return args;
        }

        public static IEnumerable<double> GetChannelAsEnumerable(double[,] I, int color)
        {
            for (int i = 0, l = I.GetLength(0); i < l; i++)
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

        public static int[,] FlexibleTake(ref double[,] arr, ref int[,] sortOrder, int N, double aheadCost)
        {

            int colors = sortOrder.GetLength(1);
            int[,] indx = new int[N, 2];
            int[] taken = new int[colors];

            for (int n = 0; n < N; n++)
            {
                //Guess it's first color
                indx[n, 0] = sortOrder[taken[0], 0];
                indx[n, 1] = 0;

                for (int color = 1; color < colors; color++)
                {
                    if (arr[n, color] * System.Math.Pow(aheadCost, taken[indx[n, 1]] - taken[color]) > arr[indx[n, 0], indx[n, 1]])
                    {
                        indx[n, 0] = sortOrder[taken[color], color];
                        indx[n, 1] = color;
                    }
                }
                taken[indx[n, 1]]++;
            }
            return indx;
        }

        public static int[,] FlexibleTake(ref double[,] arr, ref int[,] sortOrder, int N, double aheadCost, int stride, int minDistance)
        {

            int size = sortOrder.GetLength(0);
            int colors = sortOrder.GetLength(1);
            int[,] indx = new int[N, 2];
            int[] taken = new int[colors];
            int n = 0;
            while (n < N)
            {
                bool filled = false;
                bool anyColor = false;
                for (int color = 0; color < colors; color++)
                {
                    if (taken[color] >= size)
                    {
                        continue;
                    }
                    anyColor = true;
                    if (!filled || arr[n, color] * System.Math.Pow(aheadCost, taken[indx[n, 1]] - taken[color]) > arr[indx[n, 0], indx[n, 1]])
                    {
                        if (passesDistanceCheck(sortOrder[taken[color], color], stride, minDistance, ref indx, n))
                        {
                            indx[n, 0] = sortOrder[taken[color], color];
                            indx[n, 1] = color;
                            filled = true;
                        } else
                        {
                            //TODO: A bit unfair to tax the color for ahead costs but hey...
                            taken[color]++;
                        }
                    }
                }
                if (filled)
                {
                    taken[indx[n, 1]]++;
                    n++;
                }
                if (!anyColor)
                {
                    break;
                }
            }
            return indx;
        }


        static bool passesDistanceCheck(int pos, int stride, int minDistance, ref int[,] taken, int currentIndex)
        {
            for (int i=0; i<currentIndex; i++)
            {
                if (Mathf.Abs(pos % stride - taken[i, 0] % stride) < minDistance ||  Mathf.Abs(pos / stride - taken[i, 0] / stride) < minDistance)
                {
                    return false;
                }
            }
            return true;
        }

        public static double[] Max(ref double[,] I)
        {
            int colors = I.GetLength(1);
            double[] max = new double[colors];

            for (int color = 0; color < colors; color++)
            {
                for (int i = 0, l = I.GetLength(0); i < l; i++)
                {
                    if (max[color] > I[i, color] || i == 0)
                    {
                        max[color] = I[i, color];
                    }
                }
            }
            return max;
        }

        public static double[] Min(ref double[,] I)
        {
            int colors = I.GetLength(1);
            double[] min = new double[colors];

            for (int color = 0; color < colors; color++)
            {
                for (int i = 0, l = I.GetLength(0); i < l; i++)
                {
                    if (min[color] < I[i, color] || i == 0)
                    {
                        min[color] = I[i, color];
                    }
                }
            }
            return min;

        }

        public static double MaxValue(ref double[,] I)
        {
            int colors = I.GetLength(1);
            double max = 0;
            bool first = true;

            for (int color = 0; color < colors; color++)
            {
                for (int i = 0, l = I.GetLength(0); i < l; i++)
                {
                    if (first || max < I[i, color])
                    {
                        max = I[i, color];
                        first = false;
                    }
                }
            }
            return max;
        }

        public static double MinValue(ref double[,] I)
        {
            int colors = I.GetLength(1);
            double min = 0;
            bool first = true;

            for (int color = 0; color < colors; color++)
            {
                for (int i = 0, l = I.GetLength(0); i < l; i++)
                {
                    if (first || min > I[i, color])
                    {
                        min = I[i, color];
                        first = false;
                    }
                }
            }
            return min;
        }

        public static void ValueScale01(ref double[,] I)
        {
            double[] min = Min(ref I);
            double[] max = Max(ref I);
            ValueScale01(ref I, min, max);
        }

        public static void ValueScale01(ref double[,] I, double[] min, double[] max)
        {
            double[] span = new double[Mathf.Min(min.Length, max.Length)];
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = max[i] - min[i];
            }

            for (int color = 0, colors = Mathf.Min(I.GetLength(1), span.Length); color < colors; color++)
            {

                for (int i = 0, l = I.GetLength(0); i < l; i++)
                {
                    I[i, color] = (I[i, color] - min[color]) / span[color];
                }
            }
        }

        public static void ValueScaleUniform01(ref double[,] I)
        {
            double min = MinValue(ref I);
            double max = MaxValue(ref I);
            ValueScaleUniform01(ref I, min, max);
        }

        public static void ValueScaleUniform01(ref double[,] I, double min, double max)
        {
            double span = max - min;
            for (int color = 0, colors = I.GetLength(1); color < colors; color++)
            {

                for (int i = 0, l = I.GetLength(0); i < l; i++)
                {
                    I[i, color] = (I[i, color] - min) / span;
                }
            }
        }

        public static Coordinate ConvertCoordinate(int pos, int stride, int offset=0)
        {
            return new Coordinate((pos % stride) + offset, Mathf.FloorToInt(pos / stride) + offset);
        }

        public static Coordinate[] ConvertCoordinate(int[,] pos, int stride, int offset=0)
        {
            int l = pos.GetLength(0);
            Coordinate[] coords = new Coordinate[l];
            for (int i=0;i< l;i++)
            {
                coords[i] = ConvertCoordinate(pos[i, 0], stride, offset);
            }
            return coords;
        }

        public static Vector2 CoordinateToRelativeVector2(Coordinate coord, Texture2D tex, int offset=0)
        {
            return new Vector2(((float)coord.x + offset) / tex.width, ((float)coord.y + offset) / tex.height);
        }

        public static Vector3 RelativeVector2ToWorld(Vector2 v, RectTransform imageT, RectTransform canvasT)
        {
            Vector2 canvasPos = imageT.TransformPoint(v - imageT.pivot);
            Debug.Log(canvasPos);
            Debug.Log(canvasT.rect.size);
            return new Vector3(canvasPos.x, canvasPos.y);
        }
    }
}