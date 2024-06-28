using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Dashboard : UI_Screen
{
    [Header("Elements")]
    public UI_PlayerList RatingList;
    public GameObject MedalList;

    public Button SeasonSelectionPrevBtn;
    public TextMeshProUGUI SeasonSelectionLabel;
    public Button SeasonSelectionNextBtn;

    public UI_Schedule Schedule;
    public UI_PlayerList GrandLeagueList;
    public UI_PlayerList ChallengeLeagueList;
    public UI_PlayerList OpenLeagueList;

    [Header("Prefabs")]
    public UI_PlayerListElement ListElement;
    public UI_PlayerMedalListElement MedalListElement;

    // State
    public int SelectedSeason { get; set; }

    public override void Init(UI_Base baseUI)
    {
        base.Init(baseUI);

        SelectedSeason = Database.Season;
        SeasonSelectionPrevBtn.onClick.AddListener(SeasonSelectionPrevBtn_OnClick);
        SeasonSelectionNextBtn.onClick.AddListener(SeasonSelectionNextBtn_OnClick);
    }

    public void Refresh()
    {
        UpdateRatingList();
        UpdateMedalList();

        SeasonSelectionLabel.text = "Season " + SelectedSeason;
        UpdateSchedule();
        UpdateGrandLeagueList();
        UpdateChallengeLeagueList();
        UpdateOpenLeagueList();
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

    #region General Info

    private void UpdateRatingList()
    {
        foreach (Transform t in RatingList.ListContainer.transform) Destroy(t.gameObject);

        int counter = 1;
        foreach (Player p in Database.WorldRanking)
        {
            UI_PlayerListElement elem = Instantiate(ListElement, RatingList.ListContainer.transform);
            elem.Init(counter++, p, p.Elo.ToString(), ColorManager.Singleton.DefaultColor, showLeagueIcon: true);
        }
    }
    private void UpdateMedalList()
    {
        List<System.Tuple<Player, int, int, int>> medals = Database.GetHistoricGrandLeagueMedals();
        foreach (Transform t in MedalList.transform) Destroy(t.gameObject);

        int counter = 1;
        foreach (System.Tuple<Player, int, int, int> medal in medals)
        {
            UI_PlayerMedalListElement elem = Instantiate(MedalListElement, MedalList.transform);
            elem.Init(counter++, medal.Item1, ColorManager.Singleton.DefaultColor, medal.Item2, medal.Item3, medal.Item4);
        }
    }

    #endregion

    #region Season Info

    private void UpdateSchedule()
    {
        Schedule.UpdateList(BaseUI, SelectedSeason);
    }
    private void UpdateGrandLeagueList()
    {
        League l = Database.GetLeague(TournamentType.GrandLeague, SelectedSeason);
        foreach (Transform t in GrandLeagueList.ListContainer.transform) Destroy(t.gameObject);
        int counter = 1;
        foreach (Player p in l.Ranking)
        {
            UI_PlayerListElement elem = Instantiate(ListElement, GrandLeagueList.ListContainer.transform);
            Color c = ColorManager.Singleton.DefaultColor;
            if (counter >= 20) c = ColorManager.Singleton.KoColor;
            elem.Init(counter++, p, l.Standings[p].ToString(), c, showLeagueIcon: false);
        }
    }
    private void UpdateChallengeLeagueList()
    {
        League l = Database.GetLeague(TournamentType.ChallengeLeague, SelectedSeason);
        foreach (Transform t in ChallengeLeagueList.ListContainer.transform) Destroy(t.gameObject);
        int counter = 1;
        foreach (Player p in l.Ranking)
        {
            UI_PlayerListElement elem = Instantiate(ListElement, ChallengeLeagueList.ListContainer.transform);
            Color c = ColorManager.Singleton.DefaultColor;
            if (counter <= 5) c = ColorManager.Singleton.AdvanceColor;
            if (counter >= 20) c = ColorManager.Singleton.KoColor;
            elem.Init(counter++, p, l.Standings[p].ToString(), c, showLeagueIcon: false);
        }
    }
    private void UpdateOpenLeagueList()
    {
        League l = Database.GetLeague(TournamentType.OpenLeague, SelectedSeason);
        foreach (Transform t in OpenLeagueList.ListContainer.transform) Destroy(t.gameObject);
        int counter = 1;
        foreach (Player p in l.Ranking)
        {
            UI_PlayerListElement elem = Instantiate(ListElement, OpenLeagueList.ListContainer.transform);
            Color c = ColorManager.Singleton.DefaultColor;
            if (counter <= 5) c = ColorManager.Singleton.AdvanceColor;
            else if (counter > l.Players.Count - 5) c = ColorManager.Singleton.KoColor;
            elem.Init(counter++, p, l.Standings[p].ToString(), c, showLeagueIcon: false);
        }
    }

    #endregion
}
