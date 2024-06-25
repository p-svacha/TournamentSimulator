using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class PlayerGenerator
{
    private static string NamePath = "Assets/Resources/Names/";

    private static Dictionary<string, List<string>> FemaleForenames;
    private static Dictionary<string, List<string>> MaleForenames;
    private static Dictionary<string, List<string>> Surnames;

    #region Init

    public static void InitGenerator(List<Country> countries)
    {
        FemaleForenames = new Dictionary<string, List<string>>();
        MaleForenames = new Dictionary<string, List<string>>();
        Surnames = new Dictionary<string, List<string>>();

        List<string> regions = new List<string>();
        foreach (Country c in countries) regions.Add(c.Name.ToLower().Replace(" ", "-").Replace(",", ""));

        string line;
        foreach (string region in regions)
        {
            if (File.Exists(NamePath + "surnames/" + region + "_surnames.txt"))
            {
                // male forenames
                MaleForenames.Add(region, new List<string>());
                System.IO.StreamReader mForenamesFile = new System.IO.StreamReader(NamePath + "forenames/" + region + "_forenames_male.txt");
                while ((line = mForenamesFile.ReadLine()) != null) MaleForenames[region].Add(line);

                // female forenames
                FemaleForenames.Add(region, new List<string>());
                System.IO.StreamReader fForenamesFile = new System.IO.StreamReader(NamePath + "forenames/" + region + "_forenames_female.txt");
                while ((line = fForenamesFile.ReadLine()) != null) FemaleForenames[region].Add(line);

                // surnames
                Surnames.Add(region, new List<string>());
                System.IO.StreamReader surnamesFile = new System.IO.StreamReader(NamePath + "surnames/" + region + "_surnames.txt");
                while ((line = surnamesFile.ReadLine()) != null) Surnames[region].Add(line);
            }
            else Debug.LogWarning("No names found for " + region);
        }
    }

    #endregion

    public static Player GenerateRandomPlayer(TournamentSimulator sim, string region, string continent, int rating)
    {
        Country country;
        if (region != "") country = GetRandomCountryFromRegion(Database.Countries.Values.ToList(), region);
        else if (continent != "") country = GetRandomCountryFromContinent(Database.Countries.Values.ToList(), continent);
        else country = GetRandomCountry(Database.Countries.Values.ToList());

        string sex = GetRandomSex();
        string firstname = GetRandomFirstname(country, sex);
        string lastname = GetRandomLastname(country);

        Dictionary<SkillId, int> skills = new Dictionary<SkillId, int>();
        foreach (SkillDef skillDef in TournamentSimulator.SkillDefs) skills.Add(skillDef.Id, UnityEngine.Random.Range(0, 101));

        Player player = new Player(sim, firstname, lastname, country, sex, rating, skills);
        return player;
    }

    #region Random Getters
    public static string GetRandomFirstname(Country country, string sex)
    {
        string region = country.Name.ToLower().Replace(" ", "-").Replace(",", "");
        List<string> forenameCandidates;
        if (!MaleForenames.ContainsKey(region) || !FemaleForenames.ContainsKey(region)) Debug.LogError("No name found for " + region);
        if (sex == "m") forenameCandidates = MaleForenames[region];
        else forenameCandidates = FemaleForenames[region];
        string forename = forenameCandidates[Random.Range(0, forenameCandidates.Count)];
        return forename;
    }
    public static string GetRandomLastname(Country country)
    {
        string region = country.Name.ToLower().Replace(" ", "-").Replace(",", "");
        List<string> surnameCandidates = Surnames[region];
        string surname = surnameCandidates[Random.Range(0, surnameCandidates.Count)];
        return surname;
    }

    public static Country GetRandomCountry(List<Country> countries)
    {
        return GetRandomCountryFrom(countries);
    }
    public static Country GetRandomCountryFromRegion(List<Country> countries, string region)
    {
        return GetRandomCountryFrom(countries.Where(x => x.Region == region).ToList());
    }
    public static Country GetRandomCountryFromContinent(List<Country> countries, string continent)
    {
        return GetRandomCountryFrom(countries.Where(x => x.Continent == continent).ToList());
    }
    private static Country GetRandomCountryFrom(List<Country> countries)
    {
        int sum = 0;
        foreach(Country c in countries)
        {
            int scaledPopulation = (int)(Mathf.Pow(Mathf.Log10(c.Population), 8));
            sum += scaledPopulation;
        }
        int rng = Random.Range(0, sum);
        int tmp = 0;
        foreach (Country c in countries)
        {
            tmp += (int)(Mathf.Pow(Mathf.Log10(c.Population), 8));
            if (rng < tmp) return c;
        }
        throw new System.Exception();
    }

    public static string GetRandomSex()
    {
        if (Random.value < 0.5f) return "m";
        else return "w";
    }

    #endregion
}
