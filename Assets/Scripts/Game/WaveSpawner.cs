using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    //[SerializeField] private List<Card> possibleCards;
    [SerializeField] private List<CardInstance> enemies;
    [SerializeField] private TroopsField enemyField;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private int cardsPerWave = 2;
    [SerializeField] private float spawnDelay = 0.5f;

    private int currentWave = 0;
    private bool waveActive = false;

    void Start()
    {
        //StartCoroutine(StartFirstWave());
    }

    private IEnumerator StartFirstWave()
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(SpawnWave());
    }
    public IEnumerator SpawnWaveCoroutine()
    {
        yield return StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        if (waveActive)
            yield break;

        waveActive = true;
        currentWave++;

        for (int i = 0; i < cardsPerWave; i++)
        {
            yield return new WaitForSeconds(spawnDelay);
            SpawnRandomEnemyCard();
        }

        waveActive = false;
    }

    private void SpawnRandomEnemyCard()
    {
        CardInstance randomEnemy = enemies[Random.Range(0, enemies.Count)];

        GameObject newCardGO = Instantiate(randomEnemy, transform.position, Quaternion.identity).gameObject;
        CardInstance cardInstance = newCardGO.GetComponent<CardInstance>();

        if (cardInstance != null)
        {
            //cardInstance.SetCardData(randomCard);
            cardInstance.ChangeState(CardState.OnField);
            cardInstance.currentContainer = enemyField;
            cardInstance.troopsField = enemyField;
            enemyField.AddCard(cardInstance);
        }
    }
}
