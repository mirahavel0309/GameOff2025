using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopsField : MonoBehaviour
{
    [Header("Field Layout Settings")]
    public float cardSpacing = 2.5f;       // distance between cards
    public float fieldHeightOffset = 0f;   // optional vertical offset
    public float moveDuration = 0.5f;      // movement duration
    public bool horizontalPlacement = true;

    private List<CardInstance> cardsOnField = new List<CardInstance>();
    private Dictionary<CardInstance, Vector3> targetPositions = new Dictionary<CardInstance, Vector3>();

    public void AddCard(CardInstance card)
    {
        if (card == null) return;

        cardsOnField.Add(card);
        RecalculateTargetPositions();
        StopAllCoroutines();
        StartCoroutine(SmoothMoveAllCards());
    }

    public void RemoveCard(CardInstance card)
    {
        if (card == null) return;

        cardsOnField.Remove(card);
        RecalculateTargetPositions();
        StopAllCoroutines();
        StartCoroutine(SmoothMoveAllCards());
    }
    private void RecalculateTargetPositions()
    {
        targetPositions.Clear();

        if (cardsOnField.Count == 0)
            return;
        if (horizontalPlacement)
        {
            float totalWidth = (cardsOnField.Count - 1) * cardSpacing;
            float startX = transform.position.x - totalWidth / 2;

            for (int i = 0; i < cardsOnField.Count; i++)
            {
                Vector3 targetPos = new Vector3(
                    startX + i * cardSpacing,
                    transform.position.y + fieldHeightOffset,
                    0f
                );
                targetPositions[cardsOnField[i]] = targetPos;
            }
        }
        else
        {
            float totalHeight = (cardsOnField.Count - 1) * cardSpacing;
            float startY = transform.position.y - totalHeight / 2;

            for (int i = 0; i < cardsOnField.Count; i++)
            {
                Vector3 targetPos = new Vector3(
                    transform.position.x,
                    startY + i * cardSpacing,
                    0f
                );
                targetPositions[cardsOnField[i]] = targetPos;
            }
        }
    }
    public void AddCardRepresentation(GameObject cardObj)
    {
        cardsOnField.Add(cardObj.GetComponent<CardInstance>()); // or null if hero
        UpdateCardPositions();
    }

    private System.Collections.IEnumerator SmoothMoveAllCards()
    {
        float elapsed = 0f;
        Dictionary<CardInstance, Vector3> startPositions = new Dictionary<CardInstance, Vector3>();

        foreach (var card in cardsOnField)
        {
            if (card != null)
                startPositions[card] = card.transform.position;
        }

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            float tSmooth = Mathf.SmoothStep(0f, 1f, t);

            foreach (var card in cardsOnField)
            {
                if (card == null) continue;
                if (!targetPositions.ContainsKey(card)) continue;

                Vector3 start = startPositions.ContainsKey(card) ? startPositions[card] : transform.position;
                Vector3 end = targetPositions[card];
                card.transform.position = Vector3.Lerp(start, end, tSmooth);
            }

            yield return null;
        }

        foreach (var card in cardsOnField)
        {
            if (card == null) continue;
            if (targetPositions.ContainsKey(card))
                card.transform.position = targetPositions[card];
        }
    }

    public List<CardInstance> GetCards()
    {
        return cardsOnField;
    }
    private void UpdateCardPositions()
    {
        if (cardsOnField.Count == 0)
            return;

        float totalWidth = (cardsOnField.Count - 1) * cardSpacing;
        Vector3 startPos = transform.position - new Vector3(totalWidth / 2f, 0, 0);

        for (int i = 0; i < cardsOnField.Count; i++)
        {
            Vector3 targetPos = startPos + new Vector3(i * cardSpacing, 0, 0);
            StartCoroutine(MoveCard(cardsOnField[i], targetPos, moveDuration));
        }
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
}
