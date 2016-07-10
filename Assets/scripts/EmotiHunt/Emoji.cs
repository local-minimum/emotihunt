using UnityEngine;
using System;
using ImageAnalysis;
using System.Collections.Generic;

[Serializable]
public class Emoji: ScriptableObject
{
    public string emojiName;
    public string secret;    
    public string hash;

    public Color[] pixels;
    public int pixelStride;
    public int height;

    public Coordinate[] corners;

}

[Serializable]
public class EmojiDB: ScriptableObject
{
    public List<Emoji> emojis;
}
