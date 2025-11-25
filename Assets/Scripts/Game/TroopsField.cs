using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TroopsField : MonoBehaviour
{
    [Header("Field Settings")]
    public List<Transform> fieldPositions = new List<Transform>();
    public Transform spawnPoint;
    public float moveDuration = 0.5f;

    private List<CardInstance> cardsOnField = new List<CardInstance>();
    private Dictionary<CardInstance, Transform> cardToPosition = new Dictionary<CardInstance, Transform>();
    private HashSet<Transform> occupiedPositions = new HashSet<Transform>();

    public void AddCard(CardInstance card, bool setPosition = true)
    {
        if (card == null)
            return;

        Transform freeSlot = FindFreePosition();
        if (freeSlot == null)
        {
            Debug.LogWarning("No free position available on TroopsField!");
            return;
        }

        cardsOnField.Add(card);
        cardToPosition[card] = freeSlot;
        occupiedPositions.Add(freeSlot);

        // Place card at spawn point and animate move to target
        if(setPosition)
            card.transform.position = spawnPoint.position;
        StartCoroutine(MoveCardToPosition(card, freeSlot.position));
        card.troopsField = this;
    }
    public void ReasignPositions(CardInstance card)
    {
        Transform freeSlot = FindFreePosition();
        if (freeSlot == null)
        {
            Debug.LogWarning("No free position available on TroopsField!");
            return;
        }
        cardToPosition[card] = freeSlot;
        occupiedPositions.Add(freeSlot);
        card.transform.position = spawnPoint.position;
        StartCoroutine(MoveCardToPosition(card, freeSlot.position));
    }

    public void RemoveCard(CardInstance card)
    {
        if (card == null)
            return;

        if (cardToPosition.ContainsKey(card))
        {
            occupiedPositions.Remove(cardToPosition[card]);
            cardToPosition.Remove(card);
        }

        cardsOnField.Remove(card);
    }
    private Transform FindFreePosition()
    {
        foreach (var pos in fieldPositions)
        {
            if (!occupiedPositions.Contains(pos))
                return pos;
        }
        return null;
    }

    private IEnumerator MoveCardToPosition(CardInstance card, Vector3 targetPosition)
    {
        Vector3 startPosition = card.transform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            card.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            yield return null;
        }

        card.transform.position = targetPosition;
    }

    public List<CardInstance> GetCards()
    {
        return cardsOnField;
    }

    private IEnumerator MoveCard(CardInstance card, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = card.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            card.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        card.transform.position = targetPosition;
    }

    internal void ClearPositions()
    {
        occupiedPositions.Clear();
    }

    public bool AllHeroesDefeated()
    {
        return cardsOnField.Count > 0 && cardsOnField.Where(x => !(x as HeroInstance).isDefeated).Count() == 0;
    }
}
