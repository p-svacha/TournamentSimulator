using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LeagueDashboard : UI_Dashboard
{
    [Header("Elements")]
    public UI_Leaderboard Leaderboard;
    public UI_Schedule Schedule;
    public UI_PlayerList GrandLeagueList;
    public UI_PlayerList ChallengeLeagueList;
    public UI_PlayerList OpenLeagueList;
    public Toggle TiebreakerToggle;

    [Header("Prefabs")]
    public UI_PlayerListElement PlayerListElement;

    // Current display settings
    private DisciplineDef Discipline;
    private int Season;
    private bool IsShowingTiebreakers;

    public override string Label => "League Dashboard";

    public override void Init()
    {
        Leaderboard.Init();

        IsShowingTiebreakers = false;
        TiebreakerToggle.onValueChanged.AddListener(ShowTiebreakers);
    }

    public void ShowTiebreakers(bool value)
    {
        if (value == IsShowingTiebreakers) return;

        IsShowingTiebreakers = value;
        Refresh();
    }

    public override void Refresh(DisciplineDef discipline, int season)
    {
        Discipline = discipline;
        Season = season;
        Refresh();
    }

    public void Refresh()
    {
        Leaderboard.Refresh(Discipline);

        UpdateSchedule(Season);

        TiebreakerToggle.isOn = IsShowingTiebreakers;
        UpdateLeagueRanking(Discipline, Database.GetLeague(TournamentType.GrandLeague, Season), GrandLeagueList.ListContainer, IsShowingTiebreakers);
        UpdateLeagueRanking(Discipline, Database.GetLeague(TournamentType.ChallengeLeague, Season), ChallengeLeagueList.ListContainer, IsShowingTiebreakers);
        UpdateLeagueRanking(Discipline, Database.GetLeague(TournamentType.OpenLeague, Season), OpenLeagueList.ListContainer, IsShowingTiebreakers);
    }

    private void UpdateSchedule(int season)
    {
        Schedule.UpdateList(season);
    }

    private void UpdateLeagueRanking(DisciplineDef discipline, League league, GameObject container, bool showTiebreakers)
    {
        HelperFunctions.DestroyAllChildredImmediately(container);
        int rank = 1;
        foreach (Player player in league.Ranking)
        {
            UI_PlayerListElement elem = Instantiate(PlayerListElement, container.transform);

            Color backgroundColor = ColorManager.Singleton.DefaultColor;
            if (rank <= league.NumPromotions) backgroundColor = ColorManager.Singleton.AdvanceColor;
            if (rank > league.Ranking.Count - league.NumRelegations) backgroundColor = ColorManager.Singleton.KoColor;

            elem.InitLeagueList_Player(discipline, rank, league, player, backgroundColor, showTiebreakers);
            rank++;
        }
    }
}
