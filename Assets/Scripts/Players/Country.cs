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
    public Sprite Flag { get; private set; }

    public Country(int id, string name, int population, string region, string continent)
    {
        Id = id;
        Name = name;
        Population = population;
        Region = region;
        Continent = continent;
        Flag = Resources.Load<Sprite>("Icons/Flags/" + Name.Replace(" ", "-").ToLower());
    }
}
