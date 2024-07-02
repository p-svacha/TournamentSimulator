using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class HelperFunctions
{
    #region Math

    /// <summary>
    /// Modulo that handles negative values in a logical way.
    /// </summary>
    public static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public static float SmoothLerp(float start, float end, float t)
    {
        t = Mathf.Clamp01(t);
        t = t * t * (3f - 2f * t);
        return Mathf.Lerp(start, end, t);
    }

    public static Vector3 SmoothLerp(Vector3 start, Vector3 end, float t)
    {
        t = Mathf.Clamp01(t);
        t = t * t * (3f - 2f * t);
        return Vector3.Lerp(start, end, t);
    }

    #endregion

    #region Random

    public static T GetWeightedRandomElement<T>(Dictionary<T, int> weightDictionary)
    {
        int probabilitySum = weightDictionary.Sum(x => x.Value);
        int rng = Random.Range(0, probabilitySum);
        int tmpSum = 0;
        foreach (KeyValuePair<T, int> kvp in weightDictionary)
        {
            tmpSum += kvp.Value;
            if (rng < tmpSum) return kvp.Key;
        }
        throw new System.Exception();
    }
    public static T GetWeightedRandomElement<T>(Dictionary<T, float> weightDictionary)
    {
        float probabilitySum = weightDictionary.Sum(x => x.Value);
        float rng = Random.Range(0, probabilitySum);
        float tmpSum = 0;
        foreach (KeyValuePair<T, float> kvp in weightDictionary)
        {
            tmpSum += kvp.Value;
            if (rng < tmpSum) return kvp.Key;
        }
        throw new System.Exception();
    }

    public static float RandomGaussian(float minValue = 0.0f, float maxValue = 1.0f)
    {
        float u, v, S;

        do
        {
            u = 2.0f * Random.value - 1.0f;
            v = 2.0f * Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        // Standard Normal Distribution
        float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / 3.0f;
        return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
    }

    /// <summary>
    /// Returns a random number in a gaussian distribution. About 2/3 of generated numbers are within the standard deviation of the mean.
    /// </summary>
    public static float NextGaussian(float mean, float standard_deviation)
    {
        return mean + NextGaussian() * standard_deviation;
    }
    private static float NextGaussian()
    {
        float v1, v2, s;
        do
        {
            v1 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            v2 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            s = v1 * v1 + v2 * v2;
        } while (s >= 1.0f || s == 0f);
        s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);

        return v1 * s;
    }
    public static Vector2Int GetRandomNearPosition(Vector2Int pos, float standard_deviation)
    {
        float x = NextGaussian(pos.x, standard_deviation);
        float y = NextGaussian(pos.y, standard_deviation);

        return new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
    }

    #endregion

    #region UI

    /// <summary>
    /// Destroys all children of a GameObject immediately.
    /// </summary>
    public static void DestroyAllChildredImmediately(GameObject obj, int skipElements = 0)
    {
        int numChildren = obj.transform.childCount;
        for (int i = skipElements; i < numChildren; i++) GameObject.DestroyImmediate(obj.transform.GetChild(skipElements).gameObject);
    }

    public static Sprite Texture2DToSprite(Texture2D tex)
    {
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// Sets the Left, Right, Top and Bottom attribute of a RectTransform
    /// </summary>
    public static void SetRectTransformMargins(RectTransform rt, float left, float right, float top, float bottom)
    {
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    public static void SetLeft(RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void SetRight(RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void SetTop(RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void SetBottom(RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }

    /// <summary>
    /// Unfocusses any focussed button/dropdown/toggle UI element so that keyboard inputs don't get 'absorbed' by the UI element.
    /// </summary>
    public static void UnfocusNonInputUiElements()
    {
        if (EventSystem.current.currentSelectedGameObject != null && (
            EventSystem.current.currentSelectedGameObject.GetComponent<Button>() != null ||
            EventSystem.current.currentSelectedGameObject.GetComponent<TMP_Dropdown>() != null ||
            EventSystem.current.currentSelectedGameObject.GetComponent<Toggle>() != null
            ))
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    /// <summary>
    /// Returns if any ui element is currently focussed.
    /// </summary>
    public static bool IsUiFocussed()
    {
        return EventSystem.current.currentSelectedGameObject != null;
    }

    /// <summary>
    /// Returns is the mouse is currently hovering over a UI element.
    /// </summary>
    public static bool IsMouseOverUi()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    #endregion

    #region Color

    public static Color SmoothLerpColor(Color c1, Color c2, float t)
    {
        t = Mathf.Clamp01(t); // Ensure t is in the range [0, 1]
        return Color.Lerp(c1, c2, SmoothStep(t));
    }

    // SmoothStep function for smoother interpolation
    private static float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    public static ColorData Color2Data(Color c)
    {
        ColorData data = new ColorData();
        data.R = c.r;
        data.G = c.g;
        data.B = c.b;
        return data;
    }

    public static Color Data2Color(ColorData data)
    {
        return new Color(data.R, data.G, data.B);
    }

    public static Color[] GetMostCommonColors(Sprite sprite)
    {
        // Ensure we have a sprite
        if (sprite == null)
        {
            Debug.LogError("No sprite assigned.");
            return new Color[0];
        }

        // Get the texture and pixel data
        Texture2D texture = sprite.texture;
        Color[] pixels = texture.GetPixels();

        // Dictionary to count occurrences of each color
        Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();

        // Count each color
        foreach (Color pixel in pixels)
        {
            if (pixel.a < 0.2f) continue; // ignore transparent pixels

            if (colorCounts.ContainsKey(pixel))
                colorCounts[pixel]++;
            else
                colorCounts[pixel] = 1;
        }

        // Sort the colors by their count in descending order and take the top 2
        var sortedColors = colorCounts.OrderByDescending(c => c.Value).Select(c => c.Key).ToArray();

        return sortedColors;
    }

    #endregion
}
