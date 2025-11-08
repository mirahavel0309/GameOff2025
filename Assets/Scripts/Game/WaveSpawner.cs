using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private List<Card> possibleCards;   // Cards the AI can use
    [SerializeField] private TroopsField enemyField;     // The field where enemy cards appear
    [SerializeField] private GameObject cardPrefab;      // The card prefab (same one as player)
    [SerializeField] private int cardsPerWave = 2;       // Number of cards per wave
    [SerializeField] private float spawnDelay = 0.5f;    // Delay between placing cards

    private int currentWave = 0;
    private bool waveActive = false;

    void Start()
    {
        StartCoroutine(StartFirstWave());
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

        Debug.Log($"Wave {currentWave} started!");

        for (int i = 0; i < cardsPerWave; i++)
        {
            yield return new WaitForSeconds(spawnDelay);
            SpawnRandomEnemyCard();
        }

        waveActive = false;
    }

    private void SpawnRandomEnemyCard()
    {
        if (possibleCards == null || possibleCards.Count == 0 || enemyField == null)
        {
            Debug.LogWarning("WaveSpawner is missing setup references!");
            return;
        }

        Card randomCard = possibleCards[Random.Range(0, possibleCards.Count)];

        GameObject newCardGO = Instantiate(cardPrefab, transform.position, Quaternion.identity);
        CardInstance cardInstance = newCardGO.GetComponent<CardInstance>();

        if (cardInstance != null)
        {
            cardInstance.SetCardData(randomCard);
            cardInstance.ChangeState(CardState.OnField);
            cardInstance.currentContainer = enemyField;
            cardInstance.troopsField = enemyField;
            enemyField.AddCard(cardInstance);
        }
    }
}
