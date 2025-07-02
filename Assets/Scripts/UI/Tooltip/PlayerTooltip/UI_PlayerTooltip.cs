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

    public void Init(DisciplineDef discipline, Player player)
    {
        ShowTooltip();

        // Header
        FlagIcon.sprite = player.FlagBig;
        FirstNameText.text = player.FirstName;
        LastNameText.text = player.LastName;

        LeagueIcon.sprite = player.LeagueIcon;
        LeagueIcon.color = player.LeagueColor;

        // Skills
        HelperFunctions.DestroyAllChildredImmediately(SkillContainer);
        bool darkBackground = true;
        foreach(SkillDef skillDef in discipline.Skills)
        {
            Skill skill = player.Skills[skillDef];
            UI_SkillRow skillRow = Instantiate(SkillRowPrefab, SkillContainer.transform);
            skillRow.Init(player, skill, darkBackground ? ColorManager.Singleton.TableListDarkColor : ColorManager.Singleton.TableListLightColor);
            darkBackground = !darkBackground;
        }

        // Stats
        AgeText.text = player.Age.ToString();
        EloText.text = player.Elo[discipline] + " (#" + player.GetWorldRank(discipline) + ")";
        LeagueRankText.text = player.League == null ? "-" : player.CurrentLeaguePoints + " (#" + player.LeagueRank + ")";

        CountryLabelText.text = player.Country.Name;
        Dictionary<Player, int> countryRanking = Database.GetCountryRanking(discipline, player.Country.Name);
        CountryRankText.text = (countryRanking.Keys.ToList().IndexOf(player) + 1) + " / " + countryRanking.Count;

        RegionLabelText.text = player.Country.Region;
        Dictionary<Player, int> regionRanking = Database.GetRegionRanking(discipline, player.Country.Region);
        RegionRankText.text = (regionRanking.Keys.ToList().IndexOf(player) + 1) + " / " + regionRanking.Count;

        ContinentLabelText.text = player.Country.Continent;
        Dictionary<Player, int> continentRanking = Database.GetContinentRanking(discipline, player.Country.Continent);
        ContinentRankText.text = (continentRanking.Keys.ToList().IndexOf(player) + 1) + " / " + continentRanking.Count;

        // History
        HelperFunctions.DestroyAllChildredImmediately(HistoryContainer, skipElements: 1);
        foreach(League league in Database.AllLeagues.Where(x => x.Season < Database.Season && x.Players.Contains(player)))
        {
            UI_HistoryRow historyRow = Instantiate(HistoryRowPrefab, HistoryContainer.transform);
            historyRow.Init(player, league);
        }
    }
}
