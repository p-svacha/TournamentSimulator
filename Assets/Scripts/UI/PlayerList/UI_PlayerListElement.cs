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
    public TextMeshProUGUI MainValueText;
    public TextMeshProUGUI NumMatchesText;
    public TextMeshProUGUI NumWinsText;
    public TextMeshProUGUI NumLossesText;

    public GameObject WinRateContainer;
    public TextMeshProUGUI WinRateText;
    public GameObject WinRateBar;

    public Image GoldMedalIcon;
    public TextMeshProUGUI GoldMedalText;
    public Image SilverMedalIcon;
    public TextMeshProUGUI SilverMedalText;
    public Image BronzeMedalIcon;
    public TextMeshProUGUI BronzeMedalText;

    private void HideAllFields()
    {
        MainValueText.gameObject.SetActive(false);

        LeagueIcon.gameObject.SetActive(false);

        NumMatchesText.gameObject.SetActive(false);
        NumWinsText.gameObject.SetActive(false);
        NumLossesText.gameObject.SetActive(false);
        WinRateContainer.SetActive(false);

        GoldMedalIcon.gameObject.SetActive(false);
        SilverMedalIcon.gameObject.SetActive(false);
        BronzeMedalIcon.gameObject.SetActive(false);
    }

    private void ShowMainValue(string value)
    {
        MainValueText.gameObject.SetActive(true);
        MainValueText.text = value;
    }

    private void ShowMedals(Vector3 medalValues)
    {
        GoldMedalIcon.gameObject.SetActive(true);
        SilverMedalIcon.gameObject.SetActive(true);
        BronzeMedalIcon.gameObject.SetActive(true);
        GoldMedalText.text = medalValues.x.ToString();
        SilverMedalText.text = medalValues.y.ToString();
        BronzeMedalText.text = medalValues.z.ToString();
    }

    #region Player

    public void InitEloList_Player_Short(DisciplineDef discpline, int rank, Player player)
    {
        HideAllFields();

        SetBasicFields(discpline, rank, player);
        ShowMainValue(player.Elo[discpline].ToString());
        ShowLeagueInfo(player);
    }
    public void InitEloList_Player_Full(DisciplineDef discpline, int rank, Player player)
    {
        HideAllFields();

        SetBasicFields(discpline, rank, player);
        ShowMainValue(player.Elo[discpline].ToString());
        ShowMatchStatistics(player);
    }
    public void InitMedalList_Player(DisciplineDef discpline, int rank, Player player, Vector3 medals)
    {
        HideAllFields();

        SetBasicFields(discpline, rank, player);
        ShowMedals(medals);
    }
    public void InitLeagueList_Player(DisciplineDef discpline, int rank, League league, Player player, Color bgColor)
    {
        HideAllFields();

        SetBasicFields(discpline, rank, player);
        Background.color = bgColor;
        ShowMainValue(league.Standings[player].ToString());
    }

    private void SetBasicFields(DisciplineDef discpline, int rank, Player player)
    {
        Background.color = ColorManager.Singleton.DefaultColor;
        RankText.text = rank.ToString();
        FlagIcon.sprite = player.FlagSmall;
        NameText.text = player.Name;
        GetComponent<PlayerTooltipTarget>().Init(discpline, player);
    }
    private void ShowLeagueInfo(Player player)
    {
        LeagueIcon.gameObject.SetActive(true);
        LeagueIcon.sprite = player.LeagueIcon;
        LeagueIcon.color = player.LeagueColor;
    }
    private void ShowMatchStatistics(Player player)
    {
        NumMatchesText.gameObject.SetActive(true);

        NumMatchesText.text = $"{player.NumCompletedMatches} Matches";
    }

    #endregion

    #region Team

    public void InitEloList_Team_Short(DisciplineDef discpline, int rank, Team team)
    {
        HideAllFields();

        SetBasicFields(rank, team);
        ShowMainValue(team.Elo[discpline].ToString());
    }
    public void InitEloList_Team_Full(DisciplineDef discpline, int rank, Team team)
    {
        HideAllFields();

        SetBasicFields(rank, team);
        ShowMainValue(team.Elo[discpline].ToString());
    }
    public void InitMedalList_Team(int rank, Team team, Vector3 medals)
    {
        HideAllFields();

        SetBasicFields(rank, team);
        ShowMedals(medals);
    }


    private void SetBasicFields(int rank, Team t)
    {
        Background.color = ColorManager.Singleton.DefaultColor;
        RankText.text = rank.ToString();
        FlagIcon.sprite = t.FlagSmall;
        NameText.text = t.Name;
    }

    #endregion

}
