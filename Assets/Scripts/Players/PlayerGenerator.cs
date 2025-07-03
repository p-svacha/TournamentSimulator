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
    private static float MIN_INITIAL_SKILL_BASE_VALUE = 0f;
    private static float MAX_INITIAL_SKILL_BASE_VALUE = 100f;

    private static float MIN_INITIAL_INCONSISTENCY = 0f;
    private static float MAX_INITIAL_INCONSISTENCY = 20f;

    private static float MIN_INITIAL_MISTAKE_CHANCE = 0f;
    private static float MAX_INITIAL_MISTAKE_CHANCE = 0.20f;

    // End of game adjustment values
    private static float EOG_SKILL_ADJUSTMENT_RANGE = 0.5f;
    private static float EOG_INCONSISTENCY_ADJUSTMENT_RANGE = 0.2f;
    private static float EOG_MISTAKE_CHANCE_ADJUSTMENT_RANGE = 0.002f;

    // End of season adjustment values
    private static float EOS_SKILL_ADJUSTMENT_RANGE = 5;
    private static float EOS_INCONSISTENCY_ADJUSTMENT_RANGE = 2f;
    private static float EOS_MISTAKE_CHANCE_ADJUSTMENT_RANGE = 0.02f;

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

    public static Player GenerateRandomPlayer(string region, string continent)
    {
        Country country;
        if (region != "") country = GetRandomCountryFromRegion(Database.AllCountries.ToList(), region);
        else if (continent != "") country = GetRandomCountryFromContinent(Database.AllCountries.ToList(), continent);
        else country = GetRandomCountry(Database.AllCountries.ToList());

        string sex = GetRandomSex();
        string firstname = GetRandomFirstname(country, sex);
        string lastname = GetRandomLastname(country);

        Dictionary<SkillDef, Skill> skills = new Dictionary<SkillDef, Skill>();
        foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefs)
        {
            Skill skill = GenerateNewRandomizedSkill(skillDef);
            skills.Add(skillDef, skill);
        }
        Player player = new Player(firstname, lastname, country, sex, skills);
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

    public static Skill GenerateNewRandomizedSkill(SkillDef def)
    {
        float baseValue = GetRandomSkillBaseValue();
        float inconsistency = GetRandomInconsistency();
        float mistakeChance = GetRandomMistakeChance();
        return new Skill(def, baseValue, inconsistency, mistakeChance);
    }

    private static float GetRandomSkillBaseValue()
    {
        return Random.Range(MIN_INITIAL_SKILL_BASE_VALUE, MAX_INITIAL_SKILL_BASE_VALUE);
    }
    private static float GetRandomInconsistency()
    {
        return Random.Range(MIN_INITIAL_INCONSISTENCY, MAX_INITIAL_INCONSISTENCY);
    }
    private static float GetRandomMistakeChance()
    {
        return Random.Range(MIN_INITIAL_MISTAKE_CHANCE, MAX_INITIAL_MISTAKE_CHANCE);
    }


    public static float GetRandomEndOfGameSkillAdjustment()
    {
        return Random.Range(-EOG_SKILL_ADJUSTMENT_RANGE, EOG_SKILL_ADJUSTMENT_RANGE);
    }
    public static float GetRandomEndOfGameInconsistencyAdjustment()
    {
        return Random.Range(-EOG_INCONSISTENCY_ADJUSTMENT_RANGE, EOG_INCONSISTENCY_ADJUSTMENT_RANGE);
    }
    public static float GetRandomEndOfGameMistakeChanceAdjustment()
    {
        return Random.Range(-EOG_MISTAKE_CHANCE_ADJUSTMENT_RANGE, EOG_MISTAKE_CHANCE_ADJUSTMENT_RANGE);
    }

    public static float GetRandomEndOfSeasonSkillAdjustment()
    {
        return Random.Range(-EOS_SKILL_ADJUSTMENT_RANGE, EOS_SKILL_ADJUSTMENT_RANGE);
    }
    public static float GetRandomEndOfSeasonInconsistencyAdjustment()
    {
        return Random.Range(-EOS_INCONSISTENCY_ADJUSTMENT_RANGE, EOS_INCONSISTENCY_ADJUSTMENT_RANGE);
    }
    public static float GetRandomEndOfSeasonMistakeChanceAdjustment()
    {
        return Random.Range(-EOS_MISTAKE_CHANCE_ADJUSTMENT_RANGE, EOS_MISTAKE_CHANCE_ADJUSTMENT_RANGE);
    }


    #endregion
}
