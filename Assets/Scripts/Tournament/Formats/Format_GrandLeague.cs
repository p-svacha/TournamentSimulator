using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Format_GrandLeague : Format_Classic24
{
    public Format_GrandLeague(TournamentData data) : base(data) { }
    public Format_GrandLeague(TournamentSimulator sim, LeagueType type, int season, int quarter, int day, List<Player> players, List<League> allLeagues) : base(sim, type, season, quarter, day, players, allLeagues) { }

    public override void Initialize()
    {
        base.Initialize();
        Name = "Grand League";
    }
}
