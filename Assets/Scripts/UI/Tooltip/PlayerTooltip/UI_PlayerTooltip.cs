using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UI_PlayerTooltip : UI_Tooltip
{
    public static UI_PlayerTooltip Singleton;

    [Header("Header")]
    public Image FlagIcon;
    public TextMeshProUGUI FirstNameText;
    public TextMeshProUGUI LastNameText;
    public Image LeagueIcon;

    public GameObject SkillContainer;

    [Header("Attributes")]
    public TextMeshProUGUI InconsistencyText;
    public TextMeshProUGUI TiebreakerScoreText;
    public TextMeshProUGUI MistakeChanceText;

    [Header("Stats")]
    public TextMeshProUGUI AgeText;
    public TextMeshProUGUI EloText;
    public TextMeshProUGUI LeagueRankText;
    public TextMeshProUGUI CountryRankText;
    public TextMeshProUGUI CountryLabelText;
    public TextMeshProUGUI RegionRankText;
    public TextMeshProUGUI RegionLabelText;
    public TextMeshProUGUI ContinentRankText;
    public TextMeshProUGUI ContinentLabelText;

    public GameObject HistoryContainer;

    [Header("Prefabs")]
    public UI_SkillRow SkillRowPrefab;
    public UI_HistoryRow HistoryRowPrefab;

    private void Awake()
    {
        gameObject.SetActive(false);
        Singleton = this;
    }

    public void Init(Player p)
    {
        ShowTooltip();

        // Header
        FlagIcon.sprite = p.FlagBig;
        FirstNameText.text = p.FirstName;
        LastNameText.text = p.LastName;

        LeagueIcon.sprite = p.LeagueIcon;
        LeagueIcon.color = p.LeagueColor;

        // Skills
        HelperFunctions.DestroyAllChildredImmediately(SkillContainer);
        foreach(SkillDef skillDef in TournamentSimulator.SkillDefs)
        {
            UI_SkillRow skillRow = Instantiate(SkillRowPrefab, SkillContainer.transform);
            skillRow.Init(p, skillDef);
        }

        // Attributes
        InconsistencyText.text = p.Inconsistency.ToString("0.0");
        TiebreakerScoreText.text = p.TiebreakerScore.ToString("0.0");
        MistakeChanceText.text = p.MistakeChance.ToString("0.0%");

        // Stats
        AgeText.text = p.Age.ToString();
        EloText.text = p.Elo + " (#" + p.WorldRank + ")";
        LeagueRankText.text = p.League == null ? "-" : p.CurrentLeaguePoints + " (#" + p.LeagueRank + ")";

        CountryLabelText.text = p.Country.Name;
        Dictionary<Player, int> countryRanking = Database.GetCountryRanking(p.Country.Name);
        CountryRankText.text = (countryRanking.Keys.ToList().IndexOf(p) + 1) + " / " + countryRanking.Count;

        RegionLabelText.text = p.Country.Region;
        Dictionary<Player, int> regionRanking = Database.GetRegionRanking(p.Country.Region);
        RegionRankText.text = (regionRanking.Keys.ToList().IndexOf(p) + 1) + " / " + regionRanking.Count;

        ContinentLabelText.text = p.Country.Continent;
        Dictionary<Player, int> continentRanking = Database.GetContinentRanking(p.Country.Continent);
        ContinentRankText.text = (continentRanking.Keys.ToList().IndexOf(p) + 1) + " / " + continentRanking.Count;

        // History
        HelperFunctions.DestroyAllChildredImmediately(HistoryContainer, skipElements: 1);
        foreach(League league in Database.Leagues.Values.Where(x => x.Season < Database.Season && x.Players.Contains(p)))
        {
            UI_HistoryRow historyRow = Instantiate(HistoryRowPrefab, HistoryContainer.transform);
            historyRow.Init(p, league);
        }
    }
}
