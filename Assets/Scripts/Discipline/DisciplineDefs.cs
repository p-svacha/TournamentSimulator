using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DisciplineDefs
{
    public static List<DisciplineDef> Defs => new List<DisciplineDef>()
    {
        new DisciplineDef()
        {
            DefName = "Football",
            Label = "football",
            Description = "The classic football game."
        },

        new DisciplineDef()
        {
            DefName = "AgeOfEmpires",
            Label = "age of empires",
            Description = "Age of Empires II"
        },
    };
}
