using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Format_ChallengeLeague : Format_Classic24
{
    public Format_ChallengeLeague(TournamentData data) : base(data) { }
    public Format_ChallengeLeague(LeagueType type, int season, int quarter, int day, List<Player> players, List<League> allLeagues) : base(type, season, quarter, day, players, allLeagues) { }

    public override void Initialize()
    {
        base.Initialize();
        Name = "Challenge League";
    }
}
