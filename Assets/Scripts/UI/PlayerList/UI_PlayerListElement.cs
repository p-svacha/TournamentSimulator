using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_PlayerListElement : MonoBehaviour
{
    [Header("Elements")]
    public Image Background;
    public TextMeshProUGUI RankText;
    public Image FlagIcon;
    public TextMeshProUGUI NameText;
    public Image LeagueIcon;
    public TextMeshProUGUI ValueText;

    public Image GoldMedalIcon;
    public TextMeshProUGUI GoldMedalText;
    public Image SilverMedalIcon;
    public TextMeshProUGUI SilverMedalText;
    public Image BronzeMedalIcon;
    public TextMeshProUGUI BronzeMedalText;

    public void Init(int rank, Player p, string value, Color c, bool showLeagueIcon)
    {
        Background.color = c;
        RankText.text = rank.ToString();
        FlagIcon.sprite = p.FlagSmall;
        NameText.text = p.Name;

        LeagueIcon.sprite = p.LeagueIcon;
        LeagueIcon.color = p.LeagueColor;

        LeagueIcon.gameObject.SetActive(showLeagueIcon);
        ValueText.text = value;

        HideMedals();

        GetComponent<PlayerTooltipTarget>().Player = p;
    }

    public void InitTeamRanking(int rank, Team t)
    {
        Background.color = ColorManager.Singleton.DefaultColor;
        RankText.text = rank.ToString();
        FlagIcon.sprite = t.FlagSmall;
        NameText.text = t.Name;

        LeagueIcon.gameObject.SetActive(false);
        ValueText.text = t.Elo.ToString();

        HideMedals();
    }

    public void InitPlayerMedals(int rank, Player player, Vector3 medals)
    {
        Background.color = ColorManager.Singleton.DefaultColor;
        RankText.text = rank.ToString();
        FlagIcon.sprite = player.FlagSmall;
        NameText.text = player.Name;

        ShowMedals(medals);

        GetComponent<PlayerTooltipTarget>().Player = player;
    }

    public void InitTeamMedals(int rank, Team team, Vector3 medals)
    {
        Background.color = ColorManager.Singleton.DefaultColor;
        RankText.text = rank.ToString();
        FlagIcon.sprite = team.FlagSmall;
        NameText.text = team.Name;
        
        ShowMedals(medals);
    }

    private void HideMedals()
    {
        GoldMedalIcon.gameObject.SetActive(false);
        SilverMedalIcon.gameObject.SetActive(false);
        BronzeMedalIcon.gameObject.SetActive(false);
    }
    private void ShowMedals(Vector3 medalValues)
    {
        LeagueIcon.gameObject.SetActive(false);
        ValueText.gameObject.SetActive(false);
        GoldMedalIcon.gameObject.SetActive(true);
        SilverMedalIcon.gameObject.SetActive(true);
        BronzeMedalIcon.gameObject.SetActive(true);
        GoldMedalText.text = medalValues.x.ToString();
        SilverMedalText.text = medalValues.y.ToString();
        BronzeMedalText.text = medalValues.z.ToString();
    }
}
