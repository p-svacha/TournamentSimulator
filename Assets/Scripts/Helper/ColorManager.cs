using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public Color DefaultColor;
    public Color AdvanceColor;
    public Color OngoingColor;
    public Color KoColor;

    [Header("League")]
    public Sprite[] LeagueIcons;
    public Sprite NoLeagueIcon;
    public Color[] LeagueColors;
    public Color NoLeagueColor;

    [Header("Table/List")]
    public Color TableListLightColor;
    public Color TableListDarkColor;

    [Header("Text")]
    public Color GreenTextColor;
    public Color RedTextColor;

    public static ColorManager Singleton { get { return GameObject.Find("ColorManager").GetComponent<ColorManager>(); } }

    /// <summary>
    /// Checks if two colors are similar based on a given tolerance.
    /// </summary>
    /// <param name="color1">The first color.</param>
    /// <param name="color2">The second color.</param>
    /// <param name="tolerance">The maximum allowed difference (distance) between the two colors.</param>
    /// <returns>True if the colors are similar, false otherwise.</returns>
    public bool AreColorsSimilar(Color color1, Color color2, float tolerance = 0.3f)
    {
        // Calculate the squared difference between each color component
        float distanceSquared =
            Mathf.Pow(color1.r - color2.r, 2) +
            Mathf.Pow(color1.g - color2.g, 2) +
            Mathf.Pow(color1.b - color2.b, 2);

        // Use the squared distance to avoid unnecessary sqrt calculations
        return distanceSquared <= Mathf.Pow(tolerance, 2);
    }
}
