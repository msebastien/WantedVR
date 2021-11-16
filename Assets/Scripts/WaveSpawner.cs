using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawns waves of enemies
public class WaveSpawner : MonoBehaviour
{
    public enum SpawnState { SPAWNING, WAITING, COUNTING};

    [System.Serializable]
    public class Wave
    {
        public string name;
        public Transform enemyPrefab;
        public int count;
        public float rate;
    }

    public static WaveSpawner Instance;

    public Wave[] waves;
    public int nextWave = 0;

    public Transform[] spawnPoints;

    public float timeBetweenWaves = 10f;
    private float waveCountdown;

    private float searchCountdown = 1f;

    private SpawnState state = SpawnState.COUNTING;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(spawnPoints.Length == 0)
        {
            Debug.Log("No spawn points referenced.");
        }
        waveCountdown = timeBetweenWaves;
    }

    // Update is called once per frame
    void Update()
    {
        if(state == SpawnState.WAITING)
        {
            if(!IsEnemyAlive())
            {
                WaveCompleted();
            } 
            else
            {
                return;
            }
        }

        if(waveCountdown <= 0)
        {
            if(state != SpawnState.SPAWNING)
            {
                // Start spawning
                StartCoroutine(SpawnWave(waves[nextWave]));
            } 
        } 
        else
        {
            waveCountdown -= Time.deltaTime;
        }
    }

     void WaveCompleted()
    {
        // Begin a new round
        Debug.Log("Wave Completed!");

        state = SpawnState.COUNTING;
        waveCountdown = timeBetweenWaves;

        if(nextWave + 1 > waves.Length - 1)
        {
            nextWave = 0;
            Debug.Log("All waves complete. Looping.");
        } 
         else
        {
            nextWave++;
        }

        nextWave++;
    }

    bool IsEnemyAlive()
    {
        searchCountdown -= Time.deltaTime;
        // Search Enemies every one second
        if(searchCountdown <= 0f)
        {
            searchCountdown = 1f; 
            if (GameObject.FindGameObjectWithTag("Enemy") == null)
            {
                return false;
            }
        }
        
        return true;
    }
    
    // Coroutine
    IEnumerator SpawnWave(Wave _wave)
    {
        Debug.Log("Spawning Wave: " + _wave.name);
        state = SpawnState.SPAWNING;

        for(int i = 0; i < _wave.count; i++)
        {
            SpawnEnemy(_wave.enemyPrefab);
            yield return new WaitForSeconds(1f / _wave.rate); // delay between spawn
        }

        state = SpawnState.WAITING;

        yield break;
    }

    void SpawnEnemy(Transform _enemy)
    {
        // Spawn enemy
        Debug.Log("Spawning Enemy: " + _enemy.name);
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)]; 
        Instantiate(_enemy, transform.position, transform.rotation);
    }
}
