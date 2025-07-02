using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Discipline
{
    public DisciplineDef Def { get; private set; }
    public string Label => Def.Label;
    public List<SkillDef> Skills { get; private set; }

    public Discipline(DisciplineDef def)
    {
        Def = def;
        Skills = new List<SkillDef>(def.Skills);
    }

    #region Load / Save

    public DisciplineData ToData()
    {
        DisciplineData data = new DisciplineData();

        data.DefName = Def.DefName;
        data.Skills = Skills.Select(s => s.DefName).ToList();

        return data;
    }

    public Discipline(DisciplineData data)
    {
        Def = DefDatabase<DisciplineDef>.GetNamed(data.DefName);
        Skills = data.Skills.Select(s => DefDatabase<SkillDef>.GetNamed(s)).ToList();
    }

    #endregion
}
