using UnityEngine;
using System;
using ImageAnalysis;

[Serializable]
public struct Emoji
{
    public string name;
    public string secret;    
    public long hash;

    public Color[] pixels;
    public int pixelStride;
    public int height;

    public Coordinate[] points;

}
