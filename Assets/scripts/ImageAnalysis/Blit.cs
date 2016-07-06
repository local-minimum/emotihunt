using UnityEngine;
using System.Collections;

namespace ImageAnalysis {
    public static class Blit {

        static int[] cross = new int[] {
            1, 2, 0, 0, 0, 2, 1,
            2, 1, 2, 0, 2, 1, 2,
            0, 2, 1, 2, 1, 2, 0,
            0, 0, 2, 1, 2, 0, 0,
            0, 2, 1, 2, 1, 2, 0,
            2, 1, 2, 0, 2, 1, 2,
            1, 2, 0, 0, 0, 2, 1,
        };

        static Color transparent = new Color(0, 0, 0, 0);
        static Color edgeColor = new Color(1, 1, 1, 0.5f);
        static Color[] colors = new Color[]
        {
            new Color(1, 0, 0, 0.9f),
            new Color(0, 1, 0, 0.9f),
            new Color(0, 0, 1, 0.9f)
        };

        public static void Pixels(Color[] pixels, int stride, Coordinate center, Texture2D target) {
            int height = pixels.Length / stride;
            target.SetPixels(Mathf.Max(0, center.x - stride / 2), Mathf.Max(0, center.y - height / 2), stride, height, pixels);
        }
        
        public static void Cross(Coordinate center, Texture2D target, int fillColor)
        {
            Color[] pixels = GetCrossImage(colors[fillColor], edgeColor);
            Pixels(pixels, 7, center, target);
        }

        public static Color[] GetCrossImage(Color fill, Color edge)
        {
            int l = cross.Length;
            Color[] I = new Color[l];
            for (int i=0; i< l; i++)
            {
                if (cross[i] == 0)
                {
                    I[i] = transparent;
                } else if (cross[i] == 1)
                {
                    I[i] = fill;
                } else if (cross[i] == 2)
                {
                    I[i] = edge;
                }
            }
            return I;
        }
    }
}