using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_TGroup : MonoBehaviour
{
    [Header("Elements")]
    public TextMeshProUGUI TitleText;
    public GameObject LeaderboardCountainer;
    public GameObject MatchContainer;

    [Header("Prefabs")]
    public UI_TGroupParticipant LeaderboardRowPrefab;
    public UI_1v1ResultDisplay MatchRowPrefab;

    public void Init(UI_Base baseUI, TournamentGroup group)
    {
        TitleText.text = group.Name;

        HelperFunctions.DestroyAllChildredImmediately(LeaderboardCountainer);
        foreach(TournamentGroupParticipant p in group.GetGroupLeaderboard())
        {
            UI_TGroupParticipant leaderboardRow = Instantiate(LeaderboardRowPrefab, LeaderboardCountainer.transform);
            leaderboardRow.Init(p);
        }

        HelperFunctions.DestroyAllChildredImmediately(MatchContainer);
        foreach(Match m in group.Matches)
        {
            UI_1v1ResultDisplay matchRow = Instantiate(MatchRowPrefab, MatchContainer.transform);
            matchRow.DisplayMatch((TeamMatch)m);
            matchRow.GetComponent<UI_TMatch>().Init(baseUI, m);
        }
    }
}
