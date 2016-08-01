using UnityEngine;
using System.Collections.Generic;

public static class ScreenShot {

    public static IEnumerator<WaitForEndOfFrame> CaptureByBounds(
        RectTransform t, Camera cam, System.Action<Texture2D> callback, bool outerBounds=true)
    {
        //Bounds
        Vector2 min = t.rect.min;
        Vector2 max = t.rect.max;

        //The corners
        var A = cam.WorldToScreenPoint(t.TransformPoint(min));
        var B = cam.WorldToScreenPoint(t.TransformPoint(new Vector3(min.x, max.y)));
        var C = cam.WorldToScreenPoint(t.TransformPoint(max));
        var D = cam.WorldToScreenPoint(t.TransformPoint(new Vector3(max.x, min.y)));
        Debug.Log(min);
        Debug.Log(max);
        //Screen rect
        float x = 0;
        float y = 0;
        int w = 0;
        int h = 0;

        //Because rect could be skewed (by rotations) in the camera's view plane
        //The rect could eith be the Outer (capture everything inside the RectTransform)
        //or Inner (have the maximum size without capturing anything outside the RectTransform)
        if (outerBounds)
        {
            x = Mathf.Min(A.x, B.x, C.x, D.x);
            y = Mathf.Min(A.y, B.y, C.y, D.y);
            w = Mathf.CeilToInt(Mathf.Max(A.x, B.x, C.x, D.x) - x);
            h = Mathf.CeilToInt(Mathf.Max(A.y, B.y, C.y, D.y) - y);
        }
        else {
            throw new System.NotImplementedException("Sorry don't need this one");
        }

        var screenRect = new Rect(x, y, w, h);

        //Capture screen shot
        yield return new WaitForEndOfFrame();

        Texture2D tex = new Texture2D(w, h);
        tex.ReadPixels(screenRect, 0, 0);
        tex.Apply();

        //Let the callback get the captured texture
        callback(tex);
    }

    public static void WriteToFile(Texture2D tex, string path)
    {
        byte[] bytes = tex.EncodeToPNG();
        System.IO.FileStream f = System.IO.File.OpenWrite(path);
        System.IO.BinaryWriter w = new System.IO.BinaryWriter(f);
        w.Write(bytes);
        f.Close();
    }
}
