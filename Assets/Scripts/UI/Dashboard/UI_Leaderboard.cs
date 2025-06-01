using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// The UI panel containing the elo and medal leaderboard, complete with player/team and tournament type selection.
/// </summary>
public class UI_Leaderboard : MonoBehaviour
{
    private DisciplineDef Discipline;

    [Header("Display Settings")]
    public bool ShowMatchStatisticsInEloLeaderboard;

    [Header("Elements")]
    public TMP_Dropdown ParticipantTypeDropdown;
    public UI_PlayerList EloLeaderboard;
    public TMP_Dropdown MedalTournamentTypeDropdown;
    public UI_PlayerList MedalLeaderboard;

    [Header("Prefabs")]
    public UI_PlayerListElement PlayerListElement;

    public void Init()
    {
        ParticipantTypeDropdown.onValueChanged.AddListener(ParticipantTypeDropdown_OnValueChanged);
        MedalTournamentTypeDropdown.onValueChanged.AddListener(MedalTournamentTypeDropdown_OnValueChanged);
    }

    private void ParticipantTypeDropdown_OnValueChanged(int value)
    {
        UpdateEloLeaderboard();
        UpdateMedalLeaderboard();
    }

    private void MedalTournamentTypeDropdown_OnValueChanged(int value)
    {
        UpdateMedalLeaderboard();
    }

    public void Refresh(DisciplineDef discipline)
    {
        Discipline = discipline;
        UpdateEloLeaderboard();
        UpdateMedalLeaderboard();
    }

    private void UpdateEloLeaderboard()
    {
        EloLeaderboard.Clear();

        if (ParticipantTypeDropdown.value == 0) // Players
        {
            int rank = 1;
            foreach (Player p in Database.GetPlayerEloRanking(Discipline))
            {
                UI_PlayerListElement elem = Instantiate(PlayerListElement, EloLeaderboard.ListContainer.transform);
                if (ShowMatchStatisticsInEloLeaderboard) elem.InitEloList_Player_Full(Discipline, rank, p);
                else elem.InitEloList_Player_Short(Discipline, rank, p);
                rank++;
            }
        }
        if (ParticipantTypeDropdown.value == 1) // Teams
        {
            int rank = 1;
            foreach (Team t in Database.GetTeamEloRanking(Discipline))
            {
                UI_PlayerListElement elem = Instantiate(PlayerListElement, EloLeaderboard.ListContainer.transform);
                if (ShowMatchStatisticsInEloLeaderboard) elem.InitEloList_Team_Full(Discipline, rank, t);
                else elem.InitEloList_Team_Short(Discipline, rank, t);
                rank++;
            }
        }
    }
    private void UpdateMedalLeaderboard()
    {
        MedalLeaderboard.Clear();

        if (ParticipantTypeDropdown.value == 0) // Players
        {
            // Get medals
            Dictionary<Player, Vector3Int> medals = new Dictionary<Player, Vector3Int>();

            if (MedalTournamentTypeDropdown.value == 0 || MedalTournamentTypeDropdown.value == 1) // Grand League
                foreach (League l in Database.AllLeagues.Where(x => x.Discipline == Discipline && x.LeagueType == TournamentType.GrandLeague && x.IsDone))
                    Database.GetAddMedals(l.Ranking.ToDictionary(x => l.Ranking.IndexOf(x), x => new List<Player>() { x }), medals);

            if (MedalTournamentTypeDropdown.value == 0 || MedalTournamentTypeDropdown.value == 2) // Season Cup
                foreach (Tournament t in Database.AllTournaments.Where(x => x.Discipline == Discipline && x.Format == TournamentType.SeasonCup && x.IsDone))
                    Database.GetAddMedals(t.PlayerRanking, medals);

            if (MedalTournamentTypeDropdown.value == 0 || MedalTournamentTypeDropdown.value == 3) // World Cup
                foreach (Tournament t in Database.AllTournaments.Where(x => x.Discipline == Discipline && x.Format == TournamentType.WorldCup && x.IsDone))
                    Database.GetAddMedals(t.PlayerRanking, medals);

            // Order
            medals = medals.OrderByDescending(x => 3 * x.Value.x + 2 * x.Value.y + x.Value.z).ThenByDescending(x => x.Value.x).ThenByDescending(x => x.Value.y).ThenByDescending(x => x.Value.z).ToDictionary(x => x.Key, x => x.Value);

            // Display
            int rank = 1;
            foreach (var medal in medals)
            {
                UI_PlayerListElement elem = Instantiate(PlayerListElement, MedalLeaderboard.ListContainer.transform);
                elem.InitMedalList_Player(Discipline, rank, medal.Key, medal.Value);
                rank++;
            }
        }

        if (ParticipantTypeDropdown.value == 1) // Teams
        {
            // Get medals
            Dictionary<Team, Vector3Int> medals = new Dictionary<Team, Vector3Int>();

            if (MedalTournamentTypeDropdown.value == 0 || MedalTournamentTypeDropdown.value == 1) // Grand League
                foreach (League l in Database.AllLeagues.Where(x => x.Discipline == Discipline && x.LeagueType == TournamentType.GrandLeague && x.IsDone))
                    Database.GetAddMedals(l.Ranking.ToDictionary(x => l.Ranking.IndexOf(x), x => new List<Team>() { Database.GetNationalTeam(x.Country) }), medals);

            if (MedalTournamentTypeDropdown.value == 0 || MedalTournamentTypeDropdown.value == 2) // Season Cup
                foreach (Tournament t in Database.AllTournaments.Where(x => x.Discipline == Discipline && x.Format == TournamentType.SeasonCup && x.IsDone))
                    Database.GetAddMedals(t.TeamRanking, medals);

            if (MedalTournamentTypeDropdown.value == 0 || MedalTournamentTypeDropdown.value == 3) // World Cup
                foreach (Tournament t in Database.AllTournaments.Where(x => x.Discipline == Discipline && x.Format == TournamentType.WorldCup && x.IsDone))
                    Database.GetAddMedals(t.TeamRanking, medals);

            // Order
            medals = medals.OrderByDescending(x => 3 * x.Value.x + 2 * x.Value.y + x.Value.z).ThenByDescending(x => x.Value.x).ThenByDescending(x => x.Value.y).ThenByDescending(x => x.Value.z).ToDictionary(x => x.Key, x => x.Value);

            // Display
            int rank = 1;
            foreach (var medal in medals)
            {
                UI_PlayerListElement elem = Instantiate(PlayerListElement, MedalLeaderboard.ListContainer.transform);
                elem.InitMedalList_Team(rank, medal.Key, medal.Value);
                rank++;
            }
        }
    }
}
