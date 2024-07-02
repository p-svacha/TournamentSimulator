using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TeamMatch : Match
{
    public int NumTeams { get; private set; }
    public int NumPlayersPerTeam { get; private set; }
    public List<int> TeamPointDistribution { get; private set; } // How the round points are distributed among the teams based on team rank
    public List<MatchParticipant_Team> TeamParticipants { get; private set; }

    #region Before match

    // Create a new match with all attributes that are known from the start
    public TeamMatch(string name, Tournament tournament, int quarter, int day, int numTeams, int numPlayersPerTeam, List<int> teamPointDistribution, List<int> playerPointDistribution)
        : base(name, tournament, quarter, day, numTeams * numPlayersPerTeam, playerPointDistribution)
    {
        Type = MatchType.TeamMatch_1v1;
        NumTeams = numTeams;
        NumPlayersPerTeam = numPlayersPerTeam;
        TeamPointDistribution = teamPointDistribution;
        TeamParticipants = new List<MatchParticipant_Team>();
    }

    public void AddTeamToMatch(Team t, int seed)
    {
        if (IsDone) throw new System.Exception("Cannot add a team to match that is already done.");
        if (TeamParticipants.Count >= NumTeams) throw new System.Exception("Can't add a team to a match that is already full. (match has " + TeamParticipants.Count + "/" + NumTeams + " teams)");
        if (TeamParticipants.Any(x => x.Team == t)) throw new System.Exception("Can't add the same team to the match twice (" + t.Name + ")");

        TeamParticipants.Add(new MatchParticipant_Team(t, seed));
    }

    public override bool CanStartMatch()
    {
        if (IsDone) return false; // match already done
        if (IsRunning) return false; // match already running
        if (TeamParticipants.Count != NumTeams) return false; // wrong amount of teams
        if (!IsMatchToday()) return false; // match not today
        return true;
    }

    public override void StartMatch()
    {
        foreach (MatchParticipant_Team teamParticipant in TeamParticipants)
        {
            teamParticipant.SetPreMatchStats();

            // Get the players that will play for each team
            List<Player> teamPlayers = teamParticipant.Team.GetPlayersForMatch(this);
            foreach (Player teamPlayer in teamPlayers)
            {
                AddPlayerToMatch(teamPlayer, seed: 0, teamParticipant.Team);
            }
        }

        if (TeamParticipants.Any(t => Participants.Where(p => p.Team == t.Team).Count() != NumPlayersPerTeam))
            throw new System.Exception("Not all teams are exactly full"); // used as a fail-safe

        base.StartMatch();
    }

    #endregion

    #region During match

    public override void ApplyMatchRound(MatchRound round)
    {
        base.ApplyMatchRound(round);

        // Add to team points
        Dictionary<Team, int> teamScores = round.GetTeamScores();
        List<Team> teamRanking = teamScores.Keys.Reverse().ToList();

        int lastScore = -1;
        int lastPoints = -1;
        for (int rank = 0; rank < teamScores.Count; rank++)
        {
            Team team = teamRanking[rank];
            int score = teamScores[team];
            int pointsGained = TeamPointDistribution[TeamPointDistribution.Count - rank - 1];
            if (score == 0) pointsGained = 0;
            else if (score == lastScore) pointsGained = lastPoints;
            TeamParticipants.First(x => x.Team == team).IncreaseTotalScore(pointsGained);
            lastScore = score;
            lastPoints = pointsGained;
        }
    }

    #endregion

    #region After match

    public override void SetDone()
    {
        // Set match as done
        if (Rounds.Count == 0) throw new System.Exception("Can't end a match without any rounds.");
        IsDone = true;
        IsRunning = false;

        // Adjust team elos
        Dictionary<Team, int> newTeamElos = GetNewTeamEloRatings();
        foreach (KeyValuePair<Team, int> kvp in newTeamElos)
        {
            Team t = kvp.Key;
            int newElo = kvp.Value;

            TeamParticipants.First(x => x.Team == t).SetEloAfterMatch(newElo);
            t.SetElo(newElo);
        }

        // Adjust player elos
        AdjustPlayerElos();

        // Set team advancements
        for (int i = 0; i < NumAdvancements; i++)
        {
            int rank = i;
            Team advancingTeam = GetTeamRanking().Keys.ToList()[rank];
            TeamMatch targetMatch = (TeamMatch)Tournament.Matches[TargetMatchIndices[rank]];
            Debug.Log(advancingTeam.Name + " is advancing to " + targetMatch.ToString());
            targetMatch.AddTeamToMatch(advancingTeam, seed: rank);
        }

        // Check if tournament is done
        if (Tournament.Matches.All(x => x.IsDone)) Tournament.SetDone();

    }

    private Dictionary<Team, int> GetNewTeamEloRatings()
    {
        Dictionary<Team, int> teamRanking = GetTeamRanking();

        Dictionary<Team, int> newRatings = new Dictionary<Team, int>();
        foreach (Team team in teamRanking.Keys) newRatings.Add(team, team.Elo);

        for (int i = 0; i < teamRanking.Count; i++)
        {
            for (int j = i + 1; j < teamRanking.Count; j++)
            {
                bool isDraw = (teamRanking.Values.ToList()[i] == teamRanking.Values.ToList()[j]);
                AdjustTeamRatings(newRatings, teamRanking.Keys.ToList()[i], teamRanking.Keys.ToList()[j], isDraw);
            }
        }

        return newRatings;
    }

    private void AdjustTeamRatings(Dictionary<Team, int> newRatings, Team winner, Team loser, bool isDraw)
    {
        float expWinner = 1f / (1f + Mathf.Pow(10f, (loser.Elo - winner.Elo) / 400f));
        float expLoser = 1f / (1f + Mathf.Pow(10f, (winner.Elo - loser.Elo) / 400f));

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

    /// <summary>
    /// Returns an a list of all match participants ordered by match result.
    /// </summary>
    public override List<MatchParticipant> Ranking
    {
        get
        {
            if (IsDone || IsRunning) return Participants.OrderByDescending(x => TeamParticipants.First(tp => tp.Team == x.Team).TotalScore).ThenByDescending(x => x.TotalScore).ThenByDescending(x => x.Player.TiebreakerScore).ToList();
            else return Participants.OrderBy(x => TeamParticipants.First(tp => tp.Team == x.Team).Seed).ThenBy(x => x.Seed).ThenByDescending(x => x.Player.Elo).ThenByDescending(x => x.Player.TiebreakerScore).ToList();
        }
    }

    /// <summary>
    /// Returns the team ranking as a dictionary ordered by end score.
    /// </summary>
    public Dictionary<Team, int> GetTeamRanking()
    {
        if (IsDone)
        {
            return TeamParticipants.OrderByDescending(x => x.TotalScore).ToDictionary(x => x.Team, x => x.TotalScore);
        }

        else
        {
            return TeamParticipants.OrderBy(x => x.Seed).ToDictionary(x => x.Team, x => 0);
        }
    }

    #endregion

}
