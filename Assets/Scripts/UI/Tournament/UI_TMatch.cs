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
    public bool AlwaysOrderBySeed;
    public bool AlwaysUseGrayBackground;

    [Header("Prefabs")]
    public UI_TMatchPlayer PlayerPrefab;

    public void Init(UI_Base baseUI, Match match)
    {
        MatchOverviewButton.onClick.AddListener(() => baseUI.DisplayMatchOverviewScreen(match));

        if(TitleText != null) TitleText.text = match.Name;

        if (PlayerContainer != null)
        {
            if (match is TeamMatch teamMatch)
            {
                // add every participant as a row
                foreach (MatchParticipant_Team t in AlwaysOrderBySeed ? teamMatch.GetTeamSeeding() : teamMatch.GetTeamRanking())
                {
                    UI_TMatchPlayer groupPlayer = Instantiate(PlayerPrefab, PlayerContainer.transform);
                    groupPlayer.Init(teamMatch, t, IsCompact);
                }

                // add empty rows for slots that are not yet filled
                for (int i = teamMatch.GetTeamRanking().Count; i < teamMatch.NumTeams; i++)
                {
                    UI_TMatchPlayer groupPlayer = Instantiate(PlayerPrefab, PlayerContainer.transform);
                    groupPlayer.InitEmpty();
                }
            }
            else
            {
                // add every participant as a row
                foreach (MatchParticipant_Player p in AlwaysOrderBySeed ? match.GetPlayerSeeding() : match.GetPlayerRanking())
                {
                    UI_TMatchPlayer groupPlayer = Instantiate(PlayerPrefab, PlayerContainer.transform);
                    groupPlayer.Init(match, p, IsCompact);
                }

                // add empty rows for slots that are not yet filled
                for (int i = match.GetPlayerRanking().Count; i < match.NumPlayers; i++)
                {
                    UI_TMatchPlayer groupPlayer = Instantiate(PlayerPrefab, PlayerContainer.transform);
                    groupPlayer.InitEmpty();
                }
            }
        }
    }
}
