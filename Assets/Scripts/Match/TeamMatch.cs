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
        t.AddMatch(this);
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
            Team advancingTeam = GetTeamRanking()[rank].Team;
            TeamMatch targetMatch = (TeamMatch)Tournament.Matches[TargetMatchIndices[rank]];
            Debug.Log(advancingTeam.Name + " is advancing to " + targetMatch.ToString());

            int targetSeed = rank;
            if (TargetMatchSeeds.Count > 0) targetSeed = TargetMatchSeeds[rank];
            targetMatch.AddTeamToMatch(advancingTeam, targetSeed);
        }
    }

    private Dictionary<Team, int> GetNewTeamEloRatings()
    {
        List<MatchParticipant_Team> teamRanking = GetTeamRanking();

        Dictionary<Team, int> newRatings = new Dictionary<Team, int>();
        foreach (Team team in teamRanking.Select(x => x.Team)) newRatings.Add(team, team.Elo[Discipline.Def]);

        for (int i = 0; i < teamRanking.Count; i++)
        {
            for (int j = i + 1; j < teamRanking.Count; j++)
            {
                AdjustTeamRatings(newRatings, teamRanking[i].Team, teamRanking[j].Team);
            }
        }

        return newRatings;
    }

    private void AdjustTeamRatings(Dictionary<Team, int> newRatings, Team winner, Team loser)
    {
        float expWinner = 1f / (1f + Mathf.Pow(10f, (loser.Elo[Discipline.Def] - winner.Elo[Discipline.Def]) / 400f));
        float expLoser = 1f / (1f + Mathf.Pow(10f, (winner.Elo[Discipline.Def] - loser.Elo[Discipline.Def]) / 400f));

        if (IsDraw())
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

    public override List<MatchParticipant_Player> GetPlayerSeeding()
    {
        return PlayerParticipants.OrderBy(x => GetParticipant(x.Team).Seed).ThenBy(x => x.Seed).ThenByDescending(x => x.EloBeforeMatch).ToList();
    }

    /// <summary>
    /// Returns the team ranking as a dictionary ordered by end score.
    /// </summary>
    public List<MatchParticipant_Team> GetTeamRanking()
    {
        if (IsDone || IsRunning)
        {
            if (Format == MatchFormatDefOf.SingleGame)
            {
                return new List<MatchParticipant_Team>(Games[0].GetTeamRanking());
            }
            throw new System.NotImplementedException();
        }
        else return GetTeamSeeding();
    }

    public int GetTeamMatchScore(Team team) => GetTeamMatchScore(GetParticipant(team));
    public int GetTeamMatchScore(MatchParticipant_Team team)
    {
        if (Format == MatchFormatDefOf.SingleGame)
        {
            return Games[0].GetTeamPoints(team);
        }
        throw new System.NotImplementedException();
    }

    public List<MatchParticipant_Team> GetTeamSeeding()
    {
        return TeamParticipants.OrderBy(x => x.Seed).ThenByDescending(x => x.Team.Elo).ToList();
    }

    /// <summary>
    /// Returns the opponent team in a 1v1 match.
    /// </summary>
    public MatchParticipant_Team GetOpponent(Team team)
    {
        if (TeamParticipants.Count != 2) throw new System.Exception("Can only get opponent in a 1v1 match");

        return TeamParticipants.First(x => x != GetParticipant(team));
    }

    /// <summary>
    /// Returns the team of a player in this match.
    /// </summary>
    public Team GetTeamOf(Player p) => GetParticipant(p).Team;

    public List<Player> GetTeamMembersOf(Team t)
    {
        if (!IncludesTeam(t)) throw new System.Exception($"Team {t.Name} is not in this match.");

        return PlayerParticipants.Select(p => p.Player).Where(p => GetTeamOf(p) == t).ToList();
    }

    public bool IsDraw()
    {
        if (!IsDone) throw new System.Exception("Match has to be done to identify if it has been a draw.");
        if (TeamParticipants.Count != 2) throw new System.Exception("Draws are only possible for 1v1 matches.");

        if (Group == null) return false; // Matches outside of groups can never end in a draw
        return GetTeamMatchScore(TeamParticipants[0]) == GetTeamMatchScore(TeamParticipants[1]);
    }

    /// <summary>
    /// Returns if the given team has won this match. Throws exception for none 1v1 matches.
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public bool IsWinner(Team team)
    {
        if (IsDraw()) return false;

        return GetTeamRanking().IndexOf(GetParticipant(team)) == 0;
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

        foreach (MatchParticipant_Team t in TeamParticipants) t.Team.AddMatch(this);
    }

    #endregion

}
