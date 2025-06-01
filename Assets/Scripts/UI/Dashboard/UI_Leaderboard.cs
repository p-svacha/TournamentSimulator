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

    public void Refresh()
    {
        UpdateEloLeaderboard();
        UpdateMedalLeaderboard();
    }

    private void UpdateEloLeaderboard()
    {
        EloLeaderboard.Clear();

        if (ParticipantTypeDropdown.value == 0) // Players
        {
            int counter = 1;
            foreach (Player p in Database.WorldRanking)
            {
                UI_PlayerListElement elem = Instantiate(PlayerListElement, EloLeaderboard.ListContainer.transform);
                elem.Init(counter++, p, p.Elo.ToString(), ColorManager.Singleton.DefaultColor, showLeagueIcon: true);
            }
        }
        if (ParticipantTypeDropdown.value == 1) // Teams
        {
            int counter = 1;
            foreach (Team t in Database.TeamWorldRanking)
            {
                UI_PlayerListElement elem = Instantiate(PlayerListElement, EloLeaderboard.ListContainer.transform);
                elem.InitTeamRanking(counter++, t);
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
                foreach (League l in Database.Leagues.Values.Where(x => x.LeagueType == TournamentType.GrandLeague && x.IsDone))
                    Database.GetAddMedals(l.Ranking.ToDictionary(x => l.Ranking.IndexOf(x), x => new List<Player>() { x }), medals);

            if (MedalTournamentTypeDropdown.value == 0 || MedalTournamentTypeDropdown.value == 2) // Season Cup
                foreach (Tournament t in Database.Tournaments.Values.Where(x => x.Format == TournamentType.SeasonCup && x.IsDone))
                    Database.GetAddMedals(t.PlayerRanking, medals);

            if (MedalTournamentTypeDropdown.value == 0 || MedalTournamentTypeDropdown.value == 3) // World Cup
                foreach (Tournament t in Database.Tournaments.Values.Where(x => x.Format == TournamentType.WorldCup && x.IsDone))
                    Database.GetAddMedals(t.PlayerRanking, medals);

            // Order
            medals = medals.OrderByDescending(x => 3 * x.Value.x + 2 * x.Value.y + x.Value.z).ThenByDescending(x => x.Value.x).ThenByDescending(x => x.Value.y).ThenByDescending(x => x.Value.z).ToDictionary(x => x.Key, x => x.Value);

            // Display
            int rank = 1;
            foreach (var medal in medals)
            {
                UI_PlayerListElement elem = Instantiate(PlayerListElement, MedalLeaderboard.ListContainer.transform);
                elem.InitPlayerMedals(rank++, medal.Key, medal.Value);
            }
        }

        if (ParticipantTypeDropdown.value == 1) // Teams
        {
            // Get medals
            Dictionary<Team, Vector3Int> medals = new Dictionary<Team, Vector3Int>();

            if (MedalTournamentTypeDropdown.value == 0 || MedalTournamentTypeDropdown.value == 1) // Grand League
                foreach (League l in Database.Leagues.Values.Where(x => x.LeagueType == TournamentType.GrandLeague && x.IsDone))
                    Database.GetAddMedals(l.Ranking.ToDictionary(x => l.Ranking.IndexOf(x), x => new List<Team>() { Database.GetNationalTeam(x.Country) }), medals);

            if (MedalTournamentTypeDropdown.value == 0 || MedalTournamentTypeDropdown.value == 2) // Season Cup
                foreach (Tournament t in Database.Tournaments.Values.Where(x => x.Format == TournamentType.SeasonCup && x.IsDone))
                    Database.GetAddMedals(t.TeamRanking, medals);

            if (MedalTournamentTypeDropdown.value == 0 || MedalTournamentTypeDropdown.value == 3) // World Cup
                foreach (Tournament t in Database.Tournaments.Values.Where(x => x.Format == TournamentType.WorldCup && x.IsDone))
                    Database.GetAddMedals(t.TeamRanking, medals);

            // Order
            medals = medals.OrderByDescending(x => 3 * x.Value.x + 2 * x.Value.y + x.Value.z).ThenByDescending(x => x.Value.x).ThenByDescending(x => x.Value.y).ThenByDescending(x => x.Value.z).ToDictionary(x => x.Key, x => x.Value);

            // Display
            int rank = 1;
            foreach (var medal in medals)
            {
                UI_PlayerListElement elem = Instantiate(PlayerListElement, MedalLeaderboard.ListContainer.transform);
                elem.InitTeamMedals(rank++, medal.Key, medal.Value);
            }
        }
    }
}
