using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner instance;
    [Header("Wave Settings")]
    //[SerializeField] private List<Card> possibleCards;
    [SerializeField] public List<CardInstance> enemies;
    [SerializeField] private TroopsField enemyField;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private int cardsPerWave = 2;
    [SerializeField] private float spawnDelay = 0.5f;
    public float healthScale = 1;
    public float damageScale = 1;

    private int currentWave = 0;
    private bool waveActive = false;
    public GameObject BossPrefab;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        //StartCoroutine(StartFirstWave());
    }

    //private IEnumerator StartFirstWave()
    //{
    //    yield return new WaitForSeconds(0.5f);
    //    yield return StartCoroutine(SpawnWave());
    //}
    public IEnumerator SpawnWaveCoroutine(int enemyCount = 3)
    {
        yield return StartCoroutine(SpawnWave(enemyCount));
    }
    public IEnumerator SpawnBossCoroutine()
    {
        yield return StartCoroutine(SpawnBoss());
    }

    private IEnumerator SpawnWave(int enemyCount = 3)
    {
        if (waveActive)
            yield break;

        waveActive = true;
        currentWave++;

        for (int i = 0; i < enemyCount; i++)
        {
            yield return new WaitForSeconds(spawnDelay);
            SpawnRandomEnemyCard();
        }

        waveActive = false;
    }
    private IEnumerator SpawnBoss()
    {
        if (waveActive)
            yield break;

        waveActive = true;
        currentWave++;

        GameObject newCardGO = Instantiate(BossPrefab, transform.position, Quaternion.identity).gameObject;
        newCardGO.name = BossPrefab.name;
        CardInstance cardInstance = newCardGO.GetComponent<CardInstance>();
        cardInstance.ScalePower(healthScale, damageScale);

        if (cardInstance != null)
        {
            //cardInstance.SetCardData(randomCard);
            cardInstance.troopsField = enemyField;
            enemyField.AddCard(cardInstance);
        }

        waveActive = false;
    }

    private void SpawnRandomEnemyCard()
    {
        CardInstance randomEnemy = enemies[Random.Range(0, enemies.Count)];

        GameObject newCardGO = Instantiate(randomEnemy, transform.position, Quaternion.identity).gameObject;
        newCardGO.name = randomEnemy.name;
        CardInstance cardInstance = newCardGO.GetComponent<CardInstance>();
        cardInstance.ScalePower(healthScale, damageScale);

        if (cardInstance != null)
        {
            //cardInstance.SetCardData(randomCard);
            cardInstance.troopsField = enemyField;
            enemyField.AddCard(cardInstance);
        }
    }
}
