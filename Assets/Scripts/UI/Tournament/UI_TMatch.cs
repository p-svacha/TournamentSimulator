using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Overlays;

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

    [Header("Dimensions")]
    public float Height;
    public float Width;

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
                for (int i = match.GetPlayerRanking().Count; i < match.MinPlayers; i++)
                {
                    UI_TMatchPlayer groupPlayer = Instantiate(PlayerPrefab, PlayerContainer.transform);

                    // If there is a clear advancement leading to this slot, show where it comes from
                    string slotText = "";
                    MatchAdvancementTarget incomingAdvancement = match.IncomingAdvancements.FirstOrDefault(target => target.TargetSeed == i);
                    if (incomingAdvancement != null) slotText = $"Rank {incomingAdvancement.SourceRank + 1} from {incomingAdvancement.SourceMatch.Name}";

                    groupPlayer.InitEmpty(slotText);
                }
            }
        }

        // Set dimensions
        RectTransform rect = GetComponent<RectTransform>();
        if (PlayerContainer != null) LayoutRebuilder.ForceRebuildLayoutImmediate(PlayerContainer.GetComponent<RectTransform>());
        if (PlayerContainer != null) LayoutRebuilder.ForceRebuildLayoutImmediate(PlayerContainer.transform.parent.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        Height = rect.sizeDelta.y;
        Width = rect.sizeDelta.x;
    }
}
