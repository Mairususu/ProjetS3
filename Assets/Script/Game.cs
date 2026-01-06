using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameUI gameUI;
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private List<Transform> SpawnPoints;
    [SerializeField] private Transform spawnContainer;
    private List<Enemy> _enemies = new();
    private bool spawnable=true;
    public static Game Instance;
    [Header("Bonus")]
    [SerializeField] private GameObject healthItem;
    [SerializeField] private List<GameObject> bonusItems;
    [Header("Avancé actuel")]
    [SerializeField] private int round;
    [SerializeField] private int enemyKilled;
    [SerializeField] private int score;
    [Header("Record")]
    [SerializeField] private int highScore;
    [SerializeField] private int highScoreKilled;
    [SerializeField] private int highRound;

    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        Instance = this;
        round =highRound= 0;
        enemyKilled =highScoreKilled= 0;
        score = highScore = 0;
        InitSpawn();
        
        //Initialisation du GameUI
        gameUI.UpdateAll(score, enemyKilled,round);
        gameUI.UpdateHighScore( highScore, highScoreKilled, highRound);
    }

    private void Update()
    {
        if (_enemies.Count ==0 && spawnable)
        {
            round++;
            spawnable=false;
            Debug.Log("Here we go: round"+ round);
            gameUI.UpdateRound(round);
            SpawnHealth();
            StartCoroutine(TrannsitionCoroutine());
        }
    }

    IEnumerator TrannsitionCoroutine()
    {
        gameUI.ShowTransitionPanel();
        float time = 5;
        gameUI.UpdateTransition(time);
        while (time > 0)
        {
            yield return new WaitForSeconds(1f);
            time--;
            gameUI.UpdateTransition(time);
        }
        gameUI.HideTransitionPanel();
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        for (int i = 0; i < 2+round/2; i++)
        {
            var enemy = Instantiate(enemyPrefab, SpawnPoints[Random.Range(0,SpawnPoints.Count)].position+Vector3.up, Quaternion.identity).GetComponent<Enemy>();
            enemy.Initialize((round+1)*5,round+2);
            _enemies.Add(enemy);
        }
        spawnable=true;
    }

    private void SpawnHealth()
    {
        Instantiate(healthItem, spawnContainer.position, Quaternion.identity);
    }

    private void SpawnBonus(Transform trans)
    {
        Instantiate(bonusItems[Random.Range(0,bonusItems.Count)], trans.position, Quaternion.identity);
    }
    private void InitSpawn()
    {
        for (int i = 0; i < spawnContainer.childCount; i++)
        {
            SpawnPoints.Add(spawnContainer.GetChild(i));
        }
    }
    public void RemoveEnemy(Enemy enemy)
    {
        _enemies.Remove(enemy);
        enemyKilled++;
        score+=round*10;
        gameUI.UpdateAll(score,enemyKilled,round);
        if (Random.Range(0, 100) < 10)
        {
            SpawnBonus(enemy.transform);
        }
    }

    public void RestartGame()
    {
        if (score > highScore)
        {
            highScore = score;
            highScoreKilled = enemyKilled;
            highRound=round;
            gameUI.UpdateHighScore(highScore,highScoreKilled,round);
        }
        
        round = 0;
        enemyKilled = 0;
        score = 0;
        gameUI.HideDeathPanel();
        player=Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<PlayerController>();
        player.SetUI(playerUI);
        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy  = _enemies[0];
            _enemies.Remove(enemy);
            Destroy(enemy.gameObject);
        }
        
    }

    public void PlayerDie()
    {
        gameUI.ShowDeathPanel(score,enemyKilled,round);
    }
}
