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

    // Initial values
    private static int MIN_INITIAL_SKILL_SCORE = 0;
    private static int MAX_INITIAL_SKILL_SCORE = 100;

    private static float MIN_INITIAL_INCONSISTENCY = 5f;
    private static float MAX_INITIAL_INCONSISTENCY = 15f;

    private static float MIN_INITIAL_TIEBREAKER_SCORE = 0f;
    private static float MAX_INITIAL_TIEBREAKER_SCORE = 100;

    private static float MIN_INITIAL_MISTAKE_CHANCE = 0.02f;
    private static float MAX_INITIAL_MISTAKE_CHANCE = 0.08f;

    // End of season adjustment values
    private static int SKILL_ADJUSTMENT_RANGE = 5;
    private static float INCONSISTENCY_ADJUSTMENT_RANGE = 1f;
    private static float MISTAKE_CHANCE_ADJUSTMENT_RANGE = 0.005f;

    #region Init

    public static void InitGenerator(List<Country> countries)
    {
        FemaleForenames = new Dictionary<string, List<string>>();
        MaleForenames = new Dictionary<string, List<string>>();
        Surnames = new Dictionary<string, List<string>>();

        // Limit that decides when a warning is logged for too few names for a country. Scales with country population. Decrease for more warnings.
        int minForenames = 16;
        float nameNumberWarningLimit = 3000000 / minForenames;
        int absoluteWarningLimit = (int)(3000000 / nameNumberWarningLimit);

        string line;
        foreach (Country c in countries)
        {
            string region = c.Name.ToLower().Replace(" ", "-").Replace(",", "").Replace("é", "e");

            if (File.Exists(NamePath + "forenames/" + region + "_forenames_male.txt"))
            {
                // male forenames
                MaleForenames.Add(region, new List<string>());
                System.IO.StreamReader mForenamesFile = new System.IO.StreamReader(NamePath + "forenames/" + region + "_forenames_male.txt");
                while ((line = mForenamesFile.ReadLine()) != null) MaleForenames[region].Add(line);

                int warningLimit = (int)(Mathf.Max(GetScaledPopulation(c.Population) / nameNumberWarningLimit, absoluteWarningLimit));
                if (MaleForenames[region].Count < warningLimit) Debug.LogWarning("Only " + MaleForenames[region].Count + " male fornames in dataset for " + region + ". (Warning limit = " + warningLimit + ")");
            }
            else Debug.LogError("No male forenames found for " + region);

            if (File.Exists(NamePath + "forenames/" + region + "_forenames_female.txt"))
            {
                // female forenames
                FemaleForenames.Add(region, new List<string>());
                System.IO.StreamReader fForenamesFile = new System.IO.StreamReader(NamePath + "forenames/" + region + "_forenames_female.txt");
                while ((line = fForenamesFile.ReadLine()) != null) FemaleForenames[region].Add(line);

                int warningLimit = (int)(Mathf.Max(GetScaledPopulation(c.Population) / nameNumberWarningLimit, absoluteWarningLimit));
                if (FemaleForenames[region].Count < warningLimit) Debug.LogWarning("Only " + FemaleForenames[region].Count + " female fornames in dataset for " + region + ". (Warning limit = " + warningLimit + ")");
            }
            else Debug.LogError("No female forenames found for " + region);

            if (File.Exists(NamePath + "surnames/" + region + "_surnames.txt"))
            {
                // surnames
                Surnames.Add(region, new List<string>());
                System.IO.StreamReader surnamesFile = new System.IO.StreamReader(NamePath + "surnames/" + region + "_surnames.txt");
                while ((line = surnamesFile.ReadLine()) != null) Surnames[region].Add(line);

                int warningLimit = (int)(Mathf.Max(GetScaledPopulation(c.Population) * 2 / nameNumberWarningLimit, absoluteWarningLimit * 3));
                if (Surnames[region].Count < warningLimit) Debug.LogWarning("Only " + Surnames[region].Count + " surnames in dataset for " + region + ". (Warning limit = " + warningLimit + ")");
            }
            else Debug.LogError("No surnames found for " + region);
        }
    }

    #endregion

    public static Player GenerateRandomPlayer(string region, string continent, int rating)
    {
        Country country;
        if (region != "") country = GetRandomCountryFromRegion(Database.Countries.Values.ToList(), region);
        else if (continent != "") country = GetRandomCountryFromContinent(Database.Countries.Values.ToList(), continent);
        else country = GetRandomCountry(Database.Countries.Values.ToList());

        string sex = GetRandomSex();
        string firstname = GetRandomFirstname(country, sex);
        string lastname = GetRandomLastname(country);

        Dictionary<SkillDef, int> skills = new Dictionary<SkillDef, int>();
        foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefs) skills.Add(skillDef, GetRandomSkillScore());

        float inconsistency = GetRandomInconsistency();
        float tiebreakerScore = GetRandomTiebreakerScore();
        float mistakeChance = GetRandomMistakeChance();

        Player player = new Player(firstname, lastname, country, sex, rating, skills, inconsistency, tiebreakerScore, mistakeChance);
        return player;
    }

    #region Initial Random Values

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
    public static string GetRandomSex()
    {
        if (Random.value < 0.5f) return "m";
        else return "w";
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
            int scaledPopulation = GetScaledPopulation(c.Population);
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

    // Used so countries with low populations still have some chance to get picked
    private static int GetScaledPopulation(int population)
    {
        return (int)(Mathf.Pow(Mathf.Log10(population), 8));
    }

    private static int GetRandomSkillScore()
    {
        return Random.Range(MIN_INITIAL_SKILL_SCORE, MAX_INITIAL_SKILL_SCORE + 1);
    }
    
    public static float GetRandomInconsistency()
    {
        return Random.Range(MIN_INITIAL_INCONSISTENCY, MAX_INITIAL_INCONSISTENCY);
    }
    public static float GetRandomTiebreakerScore()
    {
        return Random.Range(MIN_INITIAL_TIEBREAKER_SCORE, MAX_INITIAL_TIEBREAKER_SCORE);
    }
    public static float GetRandomMistakeChance()
    {
        return Random.Range(MIN_INITIAL_MISTAKE_CHANCE, MAX_INITIAL_MISTAKE_CHANCE);
    }

    public static int GetRandomSkillAdjustment()
    {
        return Random.Range(-SKILL_ADJUSTMENT_RANGE, SKILL_ADJUSTMENT_RANGE + 1);
    }
    public static float GetRandomInconsistencyAdjustment()
    {
        return Random.Range(-INCONSISTENCY_ADJUSTMENT_RANGE, INCONSISTENCY_ADJUSTMENT_RANGE);
    }
    public static float GetRandomMistakeChanceAdjustment()
    {
        return Random.Range(-MISTAKE_CHANCE_ADJUSTMENT_RANGE, MISTAKE_CHANCE_ADJUSTMENT_RANGE);
    }


    #endregion
}
