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
            Label = "Football",
            Description = "The classic football game."
        },

        new DisciplineDef()
        {
            DefName = "AgeOfEmpires",
            Label = "Age of Empires",
            Description = "Age of Empires II"
        },
    };
}
