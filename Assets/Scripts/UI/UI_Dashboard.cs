using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Dashboard : UI_Screen
{
    [Header("Elements")]
    public UI_PlayerList RatingList;
    public UI_PlayerList GrandLeagueList;
    public UI_PlayerList ChallengeLeagueList;
    public UI_PlayerList OpenLeagueList;
    public UI_Schedule Schedule;
    public GameObject MedalList;

    [Header("Prefabs")]
    public UI_PlayerListElement ListElement;
    public UI_PlayerMedalListElement MedalListElement;

    public void UpdateRatingList(List<Player> players)
    {
        foreach (Transform t in RatingList.ListContainer.transform) Destroy(t.gameObject);

        int counter = 1;
        foreach (Player p in players.OrderByDescending(x => x.Elo))
        {
            UI_PlayerListElement elem = Instantiate(ListElement, RatingList.ListContainer.transform);
            elem.Init(counter++, p, p.Elo.ToString(), ColorManager.Singleton.DefaultColor, showLeagueIcon: true);
        }
    }

    public void UpdateMedalList(List<System.Tuple<Player, int, int, int>> medals)
    {
        foreach (Transform t in MedalList.transform) Destroy(t.gameObject);

        int counter = 1;
        foreach (System.Tuple<Player, int, int, int> medal in medals)
        {
            UI_PlayerMedalListElement elem = Instantiate(MedalListElement, MedalList.transform);
            elem.Init(counter++, medal.Item1, ColorManager.Singleton.DefaultColor, medal.Item2, medal.Item3, medal.Item4);
        }
    }

    public void UpdateSchedule(List<Tournament> tournaments)
    {
        Schedule.UpdateList(BaseUI, tournaments);
    }

    public void UpdateGrandLeagueList(League l)
    {
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
    public void UpdateChallengeLeagueList(League l)
    {
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
    public void UpdateOpenLeagueList(League l)
    {
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
}
