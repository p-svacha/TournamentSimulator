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

        Database.RefreshMedals();
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

        TournamentType filter = TournamentType.None;
        if (MedalTournamentTypeDropdown.value == 1) filter = TournamentType.GrandLeague;
        if (MedalTournamentTypeDropdown.value == 2) filter = TournamentType.SeasonCup;
        if (MedalTournamentTypeDropdown.value == 3) filter = TournamentType.WorldCup;
        if (MedalTournamentTypeDropdown.value == 4) filter = TournamentType.BIGCup;

        if (ParticipantTypeDropdown.value == 0) // Players
        {
            // Get medals
            Dictionary<Player, Vector3Int> medals = Database.GetPlayerMedalLeaderboard(filter);

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
            Dictionary<Team, Vector3Int> medals = Database.GetTeamMedalLeaderboard(filter);

            // Display
            int rank = 1;
            foreach (var medal in medals)
            {
                UI_PlayerListElement elem = Instantiate(PlayerListElement, MedalLeaderboard.ListContainer.transform);
                elem.InitMedalList_Team(Discipline, rank, medal.Key, medal.Value);
                rank++;
            }
        }
    }
}
