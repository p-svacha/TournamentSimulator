using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_TeamTooltip : UI_Tooltip
{
    public static UI_TeamTooltip Instance;

    [Header("Header")]
    public Image FlagIcon;
    public TextMeshProUGUI CountryNameText;

    [Header("Rating")]
    public TextMeshProUGUI EloText;
    public TextMeshProUGUI RegionRankText;
    public TextMeshProUGUI RegionLabelText;
    public TextMeshProUGUI ContinentRankText;
    public TextMeshProUGUI ContinentLabelText;

    public Image GoldMedal;
    public TextMeshProUGUI GoldMedalText;
    public Image SilverMedal;
    public TextMeshProUGUI SilverMedalText;
    public Image BronzeMedal;
    public TextMeshProUGUI BronzeMedalText;

    [Header("Containers")]
    public GameObject PlayersContainer;
    public GameObject RivalsContainer;

    [Header("Prefabs")]
    public UI_TeamTooltip_TeamPlayerRow PlayerRowPrefab;
    public UI_TeamTooltip_RivalRow RivalRowPrefab;

    private void Awake()
    {
        gameObject.SetActive(false);
        Instance = this;
    }

    public void Init(DisciplineDef discipline, Team team)
    {
        ShowTooltip();

        // Header
        FlagIcon.sprite = team.FlagBig;
        CountryNameText.text = team.Name;

        // Rating
        EloText.text = $"{team.Elo[discipline]} (#" + team.GetWorldRank(discipline) + ")";

        RegionLabelText.text = team.Country.Region;
        Dictionary<Team, int> regionRanking = Database.GetRegionTeamRanking(discipline, team.Country.Region);
        RegionRankText.text = (regionRanking.Keys.ToList().IndexOf(team) + 1) + " / " + regionRanking.Count;

        ContinentLabelText.text = team.Country.Continent;
        Dictionary<Team, int> continentRanking = Database.GetContinentTeamRanking(discipline, team.Country.Continent);
        ContinentRankText.text = (continentRanking.Keys.ToList().IndexOf(team) + 1) + " / " + continentRanking.Count;

        // Medals
        Vector3Int teamMedals = Database.GetTeamMedals(team);

        GoldMedal.enabled = teamMedals.x > 0;
        GoldMedalText.text = teamMedals.x == 0 ? "" : teamMedals.x.ToString();

        SilverMedal.enabled = teamMedals.y > 0;
        SilverMedalText.text = teamMedals.y == 0 ? "" : teamMedals.y.ToString();

        BronzeMedal.enabled = teamMedals.z > 0;
        BronzeMedalText.text = teamMedals.z == 0 ? "" : teamMedals.z.ToString();

        // Players
        HelperFunctions.DestroyAllChildredImmediately(PlayersContainer, skipElements: 1);
        Dictionary<Player, int> appearances = Database.GetTeamAppearances(team);
        foreach(var app in appearances)
        {
            UI_TeamTooltip_TeamPlayerRow row = GameObject.Instantiate(PlayerRowPrefab, PlayersContainer.transform);
            row.Init(app.Key, app.Value);
        }

        // Rivals
        HelperFunctions.DestroyAllChildredImmediately(RivalsContainer, skipElements: 1);
        Dictionary<Team, Vector3Int> rivals = Database.GetTeamRivals(team, maxAmount: 5);
        foreach(var rival in rivals)
        {
            UI_TeamTooltip_RivalRow row = GameObject.Instantiate(RivalRowPrefab, RivalsContainer.transform);
            row.Init(rival.Key, rival.Value.x, rival.Value.y, rival.Value.z);
        }
    }
}
