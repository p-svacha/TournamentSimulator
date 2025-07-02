using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// A team vs team vs team ... vs team match.
/// </summary>
public class TeamMatch : Match
{
    public new List<TeamGame> Games => base.Games.Select(g => (TeamGame)g).ToList();

    public int NumTeams { get; private set; }
    public int NumPlayersPerTeam { get; private set; }
    public List<int> TeamPointDistribution { get; private set; } // How the round points are distributed among the teams based on team rank
    public List<MatchParticipant_Team> TeamParticipants { get; private set; }


    public MatchParticipant_Team GetParticipant(Team t) => TeamParticipants.First(x => x.Team == t);
    public bool IncludesTeam(Team t) => TeamParticipants.Any(x => x.Team == t);
    public override int NumParticipants => NumTeams;

    #region Before match

    // Create a new match with all attributes that are known from the start
    public TeamMatch(string name, Tournament tournament, int quarter, int day, MatchFormatDef format, int numTeams, int numPlayersPerTeam, List<int> teamPointDistribution, List<int> playerPointDistribution, TournamentGroup group = null)
        : base(name, tournament, quarter, day, format, numTeams * numPlayersPerTeam, playerPointDistribution, group)
    {
        IsTeamMatch = true;
        NumTeams = numTeams;
        NumPlayersPerTeam = numPlayersPerTeam;
        TeamPointDistribution = teamPointDistribution;
        TeamParticipants = new List<MatchParticipant_Team>();
    }

    public void AddTeamToMatch(Team t, int seed = 0)
    {
        if (IsDone) throw new System.Exception("Cannot add a team to match that is already done.");
        if (TeamParticipants.Count >= NumTeams) throw new System.Exception("Can't add a team to a match that is already full. (match has " + TeamParticipants.Count + "/" + NumTeams + " teams)");
        if (TeamParticipants.Any(x => x.Team == t)) throw new System.Exception("Can't add the same team to the match twice (" + t.Name + ")");

        TeamParticipants.Add(new MatchParticipant_Team(this, t, seed));
    }

    public override bool CanStartMatch()
    {
        if (IsDone) return false; // match already done
        if (IsRunning) return false; // match already running
        if (TeamParticipants.Count != NumTeams) return false; // wrong amount of teams
        if (!IsToday) return false; // match not today
        return true;
    }

    protected override void StartMatch()
    {
        foreach (MatchParticipant_Team teamParticipant in TeamParticipants)
            teamParticipant.SetPreMatchStats();

        base.StartMatch();
    }

    protected override Game CreateGame(int index, List<GameModifierDef> gameModifiers)
    {
        return new TeamGame(this, index, gameModifiers);
    }

    public override void OnDayStart()
    {
        base.OnDayStart();

        // Select the players that will play for each team
        foreach (MatchParticipant_Team teamParticipant in TeamParticipants)
        {
            List<Player> teamPlayers = teamParticipant.Team.GetPlayersForMatch(this);
            foreach (Player teamPlayer in teamPlayers)
            {
                AddPlayerToMatch(teamPlayer, seed: 0, teamParticipant.Team);
            }
        }

        if (TeamParticipants.Any(t => PlayerParticipants.Where(p => p.Team == t.Team).Count() != NumPlayersPerTeam))
            throw new System.Exception("Not all teams are exactly full"); // used as a fail-safe
    }

    #endregion

    #region After match

    protected override void SetDone()
    {
        MarkMatchAsDone();
        AdjustTeamElos();
        AdjustPlayerElos();
        SetTeamAdvancements();
        TryConcludeParents();
    }

    private void AdjustTeamElos()
    {
        Dictionary<Team, int> newTeamElos = GetNewTeamEloRatings();
        foreach (KeyValuePair<Team, int> kvp in newTeamElos)
        {
            Team t = kvp.Key;
            int newElo = kvp.Value;

            GetParticipant(t).SetEloAfterMatch(newElo);
            t.SetElo(Discipline.Def, newElo);
        }
    }

    private void SetTeamAdvancements()
    {
        for (int i = 0; i < NumAdvancements; i++)
        {
            int rank = i;
            Team advancingTeam = TeamRanking[rank].Team;
            TeamMatch targetMatch = (TeamMatch)Tournament.Matches[TargetMatchIndices[rank]];
            Debug.Log(advancingTeam.Name + " is advancing to " + targetMatch.ToString());

            int targetSeed = rank;
            if (TargetMatchSeeds.Count > 0) targetSeed = TargetMatchSeeds[rank];
            targetMatch.AddTeamToMatch(advancingTeam, targetSeed);
        }
    }

    private Dictionary<Team, int> GetNewTeamEloRatings()
    {
        List<MatchParticipant_Team> teamRanking = TeamRanking;

        Dictionary<Team, int> newRatings = new Dictionary<Team, int>();
        foreach (Team team in teamRanking.Select(x => x.Team)) newRatings.Add(team, team.Elo[Discipline.Def]);

        for (int i = 0; i < teamRanking.Count; i++)
        {
            for (int j = i + 1; j < teamRanking.Count; j++)
            {
                bool isDraw = (teamRanking[i].MatchScore == teamRanking[j].MatchScore);
                AdjustTeamRatings(newRatings, teamRanking[i].Team, teamRanking[j].Team, isDraw);
            }
        }

        return newRatings;
    }

    private void AdjustTeamRatings(Dictionary<Team, int> newRatings, Team winner, Team loser, bool isDraw)
    {
        float expWinner = 1f / (1f + Mathf.Pow(10f, (loser.Elo[Discipline.Def] - winner.Elo[Discipline.Def]) / 400f));
        float expLoser = 1f / (1f + Mathf.Pow(10f, (winner.Elo[Discipline.Def] - loser.Elo[Discipline.Def]) / 400f));

        if (isDraw)
        {
            newRatings[winner] += (int)(20 * (0.5f - expWinner));
            newRatings[loser] += (int)(20 * (0.5f - expLoser));
        }
        else
        {
            newRatings[winner] += (int)(20 * (1 - expWinner));
            newRatings[loser] += (int)(20 * (0 - expLoser));
        }
    }

    public override List<MatchParticipant_Player> PlayerSeeding => PlayerParticipants.OrderBy(x => GetParticipant(x.Team).Seed).ThenBy(x => x.Seed).ThenByDescending(x => x.EloBeforeMatch).ToList();

    /// <summary>
    /// Returns the team ranking as a dictionary ordered by end score.
    /// </summary>
    public List<MatchParticipant_Team> TeamRanking => IsDone ? TeamParticipants.OrderByDescending(x => x.MatchScore).ToList() : TeamSeeding;
    public List<MatchParticipant_Team> TeamSeeding => TeamParticipants.OrderBy(x => x.Seed).ThenByDescending(x => x.Team.Elo).ToList();

    /// <summary>
    /// Returns the opponent team in a 1v1 match.
    /// </summary>
    public MatchParticipant_Team GetOpponent(Team team)
    {
        if (TeamParticipants.Count != 2) throw new System.Exception("Can only get opponent in a 1v1 match");

        return TeamParticipants.First(x => x != GetParticipant(team));
    }

    #endregion

    #region Save / Load

    public override MatchData ToData()
    {
        MatchData data = base.ToData();
        data.NumTeams = NumTeams;
        data.NumPlayersPerTeam = NumPlayersPerTeam;
        data.TeamPointDistribution = TeamPointDistribution;
        data.TeamParticipants = TeamParticipants.Select(x => x.ToData()).ToList();
        return data;
    }

    public TeamMatch(MatchData data) : base(data)
    {
        NumTeams = data.NumTeams;
        NumPlayersPerTeam = data.NumPlayersPerTeam;
        TeamPointDistribution = data.TeamPointDistribution;
        TeamParticipants = data.TeamParticipants.Select(x => new MatchParticipant_Team(this, x)).ToList();
    }

    #endregion

}
