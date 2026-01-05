using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [Header("Game UI")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text killedText;
    [SerializeField] private TMP_Text roundText;
    [Header("HighScore UI")]
    [SerializeField] private TMP_Text highRoundText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text highKilledText;
    [Header("Game Over")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private TMP_Text roundDeathText;
    [SerializeField] private TMP_Text killedDeathText;
    [SerializeField] private TMP_Text scoreDeathText;
    //variables privés
    private bool isPaused = false;
    void Start()
    {
        menuPanel.SetActive(false);
        deathPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
            {
                ResumeMenu();
            }
            else
            {
                PauseMenu();
            }
            isPaused = !isPaused;
        }
    }
    #region UpdateUI
    public void UpdateAll(int score,int killed,int round)
    {
        UpdateScore(score);
        UpdateEnemyKilled(killed);
        UpdateRound(round);
    }

    public void UpdateScore(int score)
    {
        scoreText.text ="Score Actuel: "+ score;
    }

    public void UpdateEnemyKilled(int killed)
    {
        killedText.text ="Nombre d'ennemis tués: "+killed;
    }

    public void UpdateRound(int round)
    {
        roundText.text = "Round: " + round;
    }

    public void UpdateHighScore(int highScore, int highKilled,int highRound)
    {
        highScoreText.text ="Le meilleur score jusqu'ici est:"+ highScore;
        highKilledText.text = "Le recor d'ennemi tué est:"+ highKilled;;
        highRoundText.text = "Le round est : "+ highRound;
    }
    #endregion
    #region PauseMenu
    private void PauseMenu()
    {
        Time.timeScale = 0;
        menuPanel.SetActive(true);
    }

    private void ResumeMenu()
    {
        Time.timeScale = 1;
        menuPanel.SetActive(false);
    }
    #endregion
    #region GameOver
    public void ShowDeathPanel(int score,int killed,int round)
    {
        Time.timeScale = 0;
        roundDeathText.text = "Tu étais au round: "+round; ;
        killedDeathText.text = "Tu as tué: "+killed;
        scoreDeathText.text = "Ton score était de: "+score;
        deathPanel.SetActive(true);
    }

    public void HideDeathPanel()
    {
        Time.timeScale = 1; 
        deathPanel.SetActive(false);
    }
    #endregion
}
