using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_LeagueDashboard : UI_Dashboard
{
    [Header("Elements")]
    public UI_Leaderboard Leaderboard;
    public UI_Schedule Schedule;
    public UI_PlayerList GrandLeagueList;
    public UI_PlayerList ChallengeLeagueList;
    public UI_PlayerList OpenLeagueList;

    [Header("Prefabs")]
    public UI_PlayerListElement PlayerListElement;

    public override string Label => "League Dashboard";

    public override void Init()
    {
        Leaderboard.Init();
    }

    public override void Refresh(DisciplineDef discipline, int season)
    {
        Leaderboard.Refresh(discipline);

        UpdateSchedule(season);

        UpdateLeagueRanking(discipline, Database.GetLeague(TournamentType.GrandLeague, season), GrandLeagueList.ListContainer);
        UpdateLeagueRanking(discipline, Database.GetLeague(TournamentType.ChallengeLeague, season), ChallengeLeagueList.ListContainer);
        UpdateLeagueRanking(discipline, Database.GetLeague(TournamentType.OpenLeague, season), OpenLeagueList.ListContainer);
    }

    private void UpdateSchedule(int season)
    {
        Schedule.UpdateList(season);
    }

    private void UpdateLeagueRanking(DisciplineDef discipline, League l, GameObject container)
    {
        HelperFunctions.DestroyAllChildredImmediately(container);
        int rank = 1;
        foreach (Player p in l.Ranking)
        {
            UI_PlayerListElement elem = Instantiate(PlayerListElement, container.transform);

            Color c = ColorManager.Singleton.DefaultColor;
            if (rank <= l.NumPromotions) c = ColorManager.Singleton.AdvanceColor;
            if (rank > l.Ranking.Count - l.NumRelegations) c = ColorManager.Singleton.KoColor;

            elem.InitLeagueList_Player(discipline, rank, l, p, c);
            rank++;
        }
    }
}
