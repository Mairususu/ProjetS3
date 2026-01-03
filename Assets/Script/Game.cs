using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private List<Transform> SpawnPoints;
    [SerializeField] private Transform spawnContainer;
    private List<Enemy> _enemies = new();
    public static Game Instance;
    [SerializeField] private int round;
    
    public PlayerController Player => player;

    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        Instance = this;
        round = 0;
        InitSpawn();
    }

    private void Update()
    {
        if (_enemies.Count ==0)
        {
            round++;
            Debug.Log("Here we go: round"+ round);
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        for (int i = 0; i < 2+round/2; i++)
        {
            var enemy = Instantiate(enemyPrefab, SpawnPoints[0].position+Vector3.up, Quaternion.identity).GetComponent<Enemy>();
            enemy.Initialize((round+1)*5,round+2);
            _enemies.Add(enemy);
        }
        
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
    }
}
