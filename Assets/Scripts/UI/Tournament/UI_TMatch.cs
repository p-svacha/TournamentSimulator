using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Match display in tournament view
/// </summary>
public class UI_TMatch : MonoBehaviour
{
    [Header("Elements")]
    public TextMeshProUGUI TitleText;
    public Button MatchOverviewButton;
    public GameObject PlayerContainer;
    public bool IsCompact;

    [Header("Prefabs")]
    public UI_TMatchPlayer PlayerPrefab;

    public void Init(UI_Base baseUI, Match match)
    {
        MatchOverviewButton.onClick.AddListener(() => baseUI.DisplayMatchOverviewScreen(match));

        if(TitleText != null) TitleText.text = match.Name;

        foreach(MatchParticipant p in match.Ranking)
        {
            UI_TMatchPlayer groupPlayer = Instantiate(PlayerPrefab, PlayerContainer.transform);
            groupPlayer.Init(match, p, IsCompact);
        }

        for(int i = match.Ranking.Count; i < match.NumPlayers; i++)
        {
            UI_TMatchPlayer groupPlayer = Instantiate(PlayerPrefab, PlayerContainer.transform);
            groupPlayer.Init(match, null, IsCompact);
        }
    }
}
