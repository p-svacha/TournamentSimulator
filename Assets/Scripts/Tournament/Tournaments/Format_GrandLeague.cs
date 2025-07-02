using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Format_GrandLeague : Format_Classic24
{
    public Format_GrandLeague(TournamentData data) : base(data) { }
    public Format_GrandLeague(DisciplineDef disciplineDef, int season, int quarter, int day, League league) : base(disciplineDef, TournamentType.GrandLeague, season, quarter, day, league)
    {
        Name = "Grand League";
    }
}
