using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Dashboard : UI_Screen
{
    [Header("Elements")]
    public TMP_Dropdown ParticipantTypeDropdown;
    public UI_PlayerList RatingList;
    public TMP_Dropdown MedalTypeDropdown;
    public GameObject MedalListContainer;

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

        ParticipantTypeDropdown.onValueChanged.AddListener(ParticipantTypeDropdown_OnValueChanged);
        MedalTypeDropdown.onValueChanged.AddListener(MedalTypeDropdown_OnValueChanged);

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

    private void ParticipantTypeDropdown_OnValueChanged(int value)
    {
        UpdateRatingList();
        UpdateMedalList();
    }

    private void MedalTypeDropdown_OnValueChanged(int value)
    {
        UpdateMedalList();
    }

    #region General Info

    private void UpdateRatingList()
    {
        HelperFunctions.DestroyAllChildredImmediately(RatingList.ListContainer);

        if (ParticipantTypeDropdown.value == 0) // Players
        {
            int counter = 1;
            foreach (Player p in Database.WorldRanking)
            {
                UI_PlayerListElement elem = Instantiate(ListElement, RatingList.ListContainer.transform);
                elem.Init(counter++, p, p.Elo.ToString(), ColorManager.Singleton.DefaultColor, showLeagueIcon: true);
            }
        }
        if (ParticipantTypeDropdown.value == 1) // Teams
        {
            int counter = 1;
            foreach (Team t in Database.TeamWorldRanking)
            {
                UI_PlayerListElement elem = Instantiate(ListElement, RatingList.ListContainer.transform);
                elem.InitTeamRanking(counter++, t);
            }
        }
    }
    private void UpdateMedalList()
    {
        HelperFunctions.DestroyAllChildredImmediately(MedalListContainer);

        if(ParticipantTypeDropdown.value == 0) // Players
        {
            // Get medals
            Dictionary<Player, Vector3Int> medals = new Dictionary<Player, Vector3Int>();

            if(MedalTypeDropdown.value == 0 || MedalTypeDropdown.value == 1) // Grand League
                foreach (League l in Database.Leagues.Values.Where(x => x.LeagueType == TournamentType.GrandLeague && x.IsDone))
                    Database.GetAddMedals(l.Ranking.ToDictionary(x => l.Ranking.IndexOf(x), x => new List<Player>() { x }), medals);

            if (MedalTypeDropdown.value == 0 || MedalTypeDropdown.value == 2) // Season Cup
                foreach (Tournament t in Database.Tournaments.Values.Where(x => x.Format == TournamentType.SeasonCup && x.IsDone))
                    Database.GetAddMedals(t.PlayerRanking, medals);

            if (MedalTypeDropdown.value == 0 || MedalTypeDropdown.value == 3) // World Cup
                foreach (Tournament t in Database.Tournaments.Values.Where(x => x.Format == TournamentType.WorldCup && x.IsDone))
                    Database.GetAddMedals(t.PlayerRanking, medals);

            // Order
            medals = medals.OrderByDescending(x => 3 * x.Value.x + 2 * x.Value.y + x.Value.z).ThenByDescending(x => x.Value.x).ThenByDescending(x => x.Value.y).ThenByDescending(x => x.Value.z).ToDictionary(x => x.Key, x => x.Value);

            // Display
            int rank = 1;
            foreach (var medal in medals)
            {
                UI_PlayerMedalListElement elem = Instantiate(MedalListElement, MedalListContainer.transform);
                elem.Init(rank++, medal.Key, medal.Value);
            }
        }

        if (ParticipantTypeDropdown.value == 1) // Teams
        {
            // Get medals
            Dictionary<Team, Vector3Int> medals = new Dictionary<Team, Vector3Int>();

            if (MedalTypeDropdown.value == 0 || MedalTypeDropdown.value == 1) // Grand League
                foreach (League l in Database.Leagues.Values.Where(x => x.LeagueType == TournamentType.GrandLeague && x.IsDone))
                    Database.GetAddMedals(l.Ranking.ToDictionary(x => l.Ranking.IndexOf(x), x => new List<Team>() { Database.GetNationalTeam(x.Country) }), medals);

            if (MedalTypeDropdown.value == 0 || MedalTypeDropdown.value == 2) // Season Cup
                foreach (Tournament t in Database.Tournaments.Values.Where(x => x.Format == TournamentType.SeasonCup && x.IsDone))
                    Database.GetAddMedals(t.TeamRanking, medals);

            if (MedalTypeDropdown.value == 0 || MedalTypeDropdown.value == 3) // World Cup
                foreach (Tournament t in Database.Tournaments.Values.Where(x => x.Format == TournamentType.WorldCup && x.IsDone))
                    Database.GetAddMedals(t.TeamRanking, medals);

            // Order
            medals = medals.OrderByDescending(x => 3 * x.Value.x + 2 * x.Value.y + x.Value.z).ThenByDescending(x => x.Value.x).ThenByDescending(x => x.Value.y).ThenByDescending(x => x.Value.z).ToDictionary(x => x.Key, x => x.Value);

            // Display
            int rank = 1;
            foreach (var medal in medals)
            {
                UI_PlayerMedalListElement elem = Instantiate(MedalListElement, MedalListContainer.transform);
                elem.Init(rank++, medal.Key, medal.Value);
            }
        }
    }

    #endregion

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
            UI_PlayerListElement elem = Instantiate(ListElement, container.transform);
            Color c = ColorManager.Singleton.DefaultColor;
            if (counter <= l.NumPromotions) c = ColorManager.Singleton.AdvanceColor;
            if (counter > l.Ranking.Count - l.NumRelegations) c = ColorManager.Singleton.KoColor;
            elem.Init(counter++, p, l.Standings[p].ToString(), c, showLeagueIcon: false);
        }
    }

    #endregion
}
