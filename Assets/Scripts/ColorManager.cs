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
}
