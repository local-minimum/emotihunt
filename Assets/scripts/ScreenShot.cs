using UnityEngine;
using System.Linq;

public enum OverflowMode { Crop,  Overflow};
public enum BoxMode { Outer, Inner};

public static class ScreenShot {

    public static void CaptureByBounds(
    RectTransform t, Camera cam, System.Action<Texture2D> callback)
    {
        CaptureByBounds(t, cam, callback, BoxMode.Outer, OverflowMode.Crop, OverflowMode.Crop);
    }

    /// <summary>
    /// Captures part of the screen.
    /// 
    /// NOTE: It must be called during end of frame from coroutine yielding NewEndOfFrame.
    /// </summary>
    public static void CaptureByBounds(
        RectTransform t, Camera cam, System.Action<Texture2D> callback, BoxMode boxMode, OverflowMode canvasOverflowMode, OverflowMode screenOverflowMode)
    {

        //Corners as positions of canvas
        Vector3[] corners = GetCorners(t);

        Rect rect = BoxCorners(corners, boxMode);
        Debug.Log(rect);

        var c = t.GetComponentsInParent<Canvas>().Last();
        if (canvasOverflowMode == OverflowMode.Crop)
        {
            rect = CropRectToInsideOther(rect, c.pixelRect);
        }
        Debug.Log(rect);

        if (screenOverflowMode == OverflowMode.Crop)
        {
            rect = CropRectToInsideOther(rect, new Rect(0, 0, Screen.width, Screen.height));
        }
        Debug.Log(rect);

        //Capture screen shot
        Texture2D tex = new Texture2D(Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height));
        tex.ReadPixels(rect, 0, 0);
        tex.Apply();

        //Let the callback get the captured texture
        callback(tex);
    }

    public static void WriteToFile(Texture2D tex, string path)
    {
        
        byte[] bytes = tex.EncodeToPNG();
        System.IO.FileStream f = System.IO.File.OpenWrite(path);
        f.Write(bytes, 0, bytes.Length);
        f.Close();
        
    }

    public static Vector3[] GetCorners(RectTransform t)
    {
        //Bounds
        Vector2 min = t.rect.min;
        Vector2 max = t.rect.max;

        //The corners
        var A = t.TransformPoint(min);
        var B = t.TransformPoint(new Vector3(min.x, max.y));
        var C = t.TransformPoint(max);
        var D = t.TransformPoint(new Vector3(max.x, min.y));

        //Corners in canvas space
        return new Vector3[] { A, B, C, D };
    }

    public static Rect BoxCorners(Vector3[] corners, BoxMode mode)
    {
        //Screen rect
        float x = 0;
        float y = 0;
        int w = 0;
        int h = 0;

        //Because rect could be skewed (by rotations) in the camera's view plane
        //The rect could eith be the Outer (capture everything inside the RectTransform)
        //or Inner (have the maximum size without capturing anything outside the RectTransform)
        if (mode == BoxMode.Outer)
        {
            x = corners.Select(v => v.x).Min();
            y = corners.Select(v => v.y).Min();
            w = Mathf.CeilToInt(corners.Select(v => v.x).Max() - x);
            h = Mathf.CeilToInt(corners.Select(v => v.y).Max() - y);
        }
        else {
            throw new System.NotImplementedException("Sorry don't need this one");
        }

        return new Rect(x, y, w, h);
    }

    public static Rect CropRectToInsideOther(Rect A, Rect B)
    {

        var ret = new Rect(Mathf.Max(A.x, B.x), Mathf.Max(A.y, B.y), 0, 0);
        ret.width = Mathf.Min(A.width, B.xMax - ret.x);
        ret.height = Mathf.Min(A.height, B.yMax - ret.y);
        return ret;
    }
}
