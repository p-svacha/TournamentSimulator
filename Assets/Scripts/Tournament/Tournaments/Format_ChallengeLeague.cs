using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Format_ChallengeLeague : Format_Classic24
{
    public Format_ChallengeLeague(TournamentData data) : base(data) { }
    public Format_ChallengeLeague(DisciplineDef disciplineDef, int season, int quarter, int day, League league) : base(disciplineDef, TournamentType.ChallengeLeague, season, quarter, day, league)
    {
        Name = "Challenge League";
    }
}
