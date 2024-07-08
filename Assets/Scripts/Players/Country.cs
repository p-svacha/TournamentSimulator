using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Country
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public int Population { get; private set; }
    public string Region { get; private set; }
    public string Continent { get; private set; }
    public Sprite FlagBig { get; private set; }
    public Sprite FlagSmall { get; private set; }
    public string FifaCode { get; private set; }

    public Country(int id, string name, int population, string region, string continent, string fifaCode)
    {
        Id = id;
        Name = name;
        Population = population;
        Region = region;
        Continent = continent;
        FifaCode = fifaCode;
        FlagBig = Resources.Load<Sprite>("Icons/Flags/180x120/" + FifaCode);
        FlagSmall = Resources.Load<Sprite>("Icons/Flags/48x32/" + FifaCode);
    }
}
