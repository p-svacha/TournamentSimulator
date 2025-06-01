using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Dashboard : UI_Screen
{
    [Header("Elements")]
    public UI_Leaderboard Leaderboard;

    public Button SeasonSelectionPrevBtn;
    public TextMeshProUGUI SeasonSelectionLabel;
    public Button SeasonSelectionNextBtn;

    public UI_Schedule Schedule;
    public UI_PlayerList GrandLeagueList;
    public UI_PlayerList ChallengeLeagueList;
    public UI_PlayerList OpenLeagueList;

    [Header("Prefabs")]
    public UI_PlayerListElement PlayerListElement;



    // State
    public int SelectedSeason { get; set; }

    public override void Init(UI_Base baseUI)
    {
        base.Init(baseUI);

        Leaderboard.Init();

        SelectedSeason = Database.Season;
        SeasonSelectionPrevBtn.onClick.AddListener(SeasonSelectionPrevBtn_OnClick);
        SeasonSelectionNextBtn.onClick.AddListener(SeasonSelectionNextBtn_OnClick);
    }

    public void Refresh()
    {
        Leaderboard.Refresh();

        SeasonSelectionLabel.text = "Season " + SelectedSeason;
        UpdateSchedule();

        UpdateLeagueRanking(Database.GetLeague(TournamentType.GrandLeague, SelectedSeason), GrandLeagueList.ListContainer);
        UpdateLeagueRanking(Database.GetLeague(TournamentType.ChallengeLeague, SelectedSeason), ChallengeLeagueList.ListContainer);
        UpdateLeagueRanking(Database.GetLeague(TournamentType.OpenLeague, SelectedSeason), OpenLeagueList.ListContainer);
    }

    private void SeasonSelectionPrevBtn_OnClick()
    {
        SelectedSeason--;
        if (SelectedSeason < 1) SelectedSeason = 1;
        Refresh();
    }

    private void SeasonSelectionNextBtn_OnClick()
    {
        SelectedSeason++;
        if (SelectedSeason > Database.LatestSeason) SelectedSeason = Database.LatestSeason;
        Refresh();
    }

    #region Season Info

    private void UpdateSchedule()
    {
        Schedule.UpdateList(BaseUI, SelectedSeason);
    }

    private void UpdateLeagueRanking(League l, GameObject container)
    {
        HelperFunctions.DestroyAllChildredImmediately(container);
        int counter = 1;
        foreach (Player p in l.Ranking)
        {
            UI_PlayerListElement elem = Instantiate(PlayerListElement, container.transform);
            Color c = ColorManager.Singleton.DefaultColor;
            if (counter <= l.NumPromotions) c = ColorManager.Singleton.AdvanceColor;
            if (counter > l.Ranking.Count - l.NumRelegations) c = ColorManager.Singleton.KoColor;
            elem.Init(counter++, p, l.Standings[p].ToString(), c, showLeagueIcon: false);
        }
    }

    #endregion
}
