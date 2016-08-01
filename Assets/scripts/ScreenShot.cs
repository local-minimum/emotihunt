using UnityEngine;
using System.Linq;

public enum OverflowMode { Crop,  Overflow};
public enum BoxMode { Outer, Inner};

public static class ScreenShot {

    /// <summary>
    /// Captures part of the screen.
    /// 
    /// Uses BoxMode.Outer and OverflowMode.Crop for both Canvas and Screen.
    /// 
    /// NOTE: It must be called during end of frame from coroutine yielding NewEndOfFrame.
    /// NOTE: It only is tested to work for Screen Overlay canvas, Camera parameter is kept for future expansions beyond such canvases.
    /// </summary>    
    /// <param name="transform">Transform to use for screenshot bounds</param>
    /// <param name="cam">Camera (not in use)</param>
    /// <param name="callback">Method that will get the texture as parameter when texture has been filled.</param>
    public static void CaptureByBounds(
    RectTransform transform, Camera cam, System.Action<Texture2D> callback)
    {
        CaptureByBounds(transform, cam, callback, BoxMode.Outer, OverflowMode.Crop, OverflowMode.Crop);
    }

    /// <summary>
    /// Captures part of the screen.
    /// 
    /// NOTE: It must be called during end of frame from coroutine yielding NewEndOfFrame.
    /// NOTE: It only is tested to work for Screen Overlay canvas, Camera parameter is kept for future expansions beyond such canvases.
    /// </summary>
    /// <param name="transform">Transform to use for screenshot bounds</param>
    /// <param name="cam">Camera (not in use)</param>
    /// <param name="callback">Method that will get the texture as parameter when texture has been filled.</param>
    /// <param name="boxMode">How to box the RectTransform when rotated</param>
    /// <param name="canvasOverflowMode">If Rect should be Cropped if it overflows canvas</param>
    /// <param name="screenOverflowMode">If Rect should be Cropped if it overflows screen</param>
    public static void CaptureByBounds(
        RectTransform transform, Camera cam, System.Action<Texture2D> callback, BoxMode boxMode, OverflowMode canvasOverflowMode, OverflowMode screenOverflowMode)
    {

        //Corners as positions of canvas
        Vector3[] corners = GetCorners(transform);

        Rect rect = BoxCorners(corners, boxMode);

        var c = transform.GetComponentsInParent<Canvas>().Last();
        if (canvasOverflowMode == OverflowMode.Crop)
        {
            rect = CropRectToInsideOther(rect, c.pixelRect);
        }

        if (screenOverflowMode == OverflowMode.Crop)
        {
            rect = CropRectToInsideOther(rect, new Rect(0, 0, Screen.width, Screen.height));
        }

        //Capture screen shot
        Texture2D tex = new Texture2D(Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height));
        tex.ReadPixels(rect, 0, 0);
        tex.Apply();

        //Let the callback get the captured texture
        callback(tex);
    }

    /// <summary>
    /// Gets the corners of a RectTransform in GlobalSpace.
    /// </summary>
    /// <param name="transform"></param>
    /// <returns>Array of corner points</returns>
    public static Vector3[] GetCorners(RectTransform transform)
    {
        //Bounds
        Vector2 min = transform.rect.min;
        Vector2 max = transform.rect.max;

        //The corners
        var A = transform.TransformPoint(min);
        var B = transform.TransformPoint(new Vector3(min.x, max.y));
        var C = transform.TransformPoint(max);
        var D = transform.TransformPoint(new Vector3(max.x, min.y));

        //Corners in canvas space
        return new Vector3[] { A, B, C, D };
    }

    /// <summary>
    /// Method to create a non-rotated bounding box based on corner data
    /// 
    /// NOTE: Only BoxMode.Outer is supported at the moment.
    /// </summary>
    /// <param name="corners">Array of corner points</param>
    /// <param name="mode">Boxing mode.</param>
    /// <returns>Bounding rect</returns>
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

    /// <summary>
    /// Crops the first Rect so it is fully contained within the second.
    /// </summary>
    /// <param name="A">Rect to be cropped if needed</param>
    /// <param name="B">Bounding rect</param>
    /// <returns>Cropped rect</returns>
    public static Rect CropRectToInsideOther(Rect A, Rect B)
    {

        var ret = new Rect(Mathf.Max(A.x, B.x), Mathf.Max(A.y, B.y), 0, 0);
        ret.width = Mathf.Min(A.width, B.xMax - ret.x);
        ret.height = Mathf.Min(A.height, B.yMax - ret.y);
        return ret;
    }

    /// <summary>
    /// Method to write Texture2D to file.
    /// </summary>
    /// <param name="tex">The texture</param>
    /// <param name="path">File path</param>
    public static void WriteToFile(Texture2D tex, string path)
    {

        byte[] bytes = tex.EncodeToPNG();
        System.IO.FileStream f = System.IO.File.OpenWrite(path);
        f.Write(bytes, 0, bytes.Length);
        f.Close();

    }
}
