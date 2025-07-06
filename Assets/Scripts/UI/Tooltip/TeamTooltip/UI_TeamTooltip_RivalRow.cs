using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_TeamTooltip_RivalRow : MonoBehaviour
{
    [Header("Elements")]
    public Image FlagIcon;
    public TextMeshProUGUI CountryText;
    public TextMeshProUGUI ResultsText;

    public void Init(Team team, int wins, int draws, int losses)
    {
        FlagIcon.sprite = team.FlagSmall;
        CountryText.text = team.Name;
        ResultsText.text = $"{wins}-{draws}-{losses}";
    }
}
