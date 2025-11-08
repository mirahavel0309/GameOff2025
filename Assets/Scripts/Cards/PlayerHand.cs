using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    public static PlayerHand instance;
    [Header("Hand Layout Settings")]
    public float cardSpacing = 2.0f;
    public float moveDuration = 0.5f;
    public TroopsField troopsField;

    private List<ElementalCardInstance> cardsInHand = new List<ElementalCardInstance>();
    private Dictionary<ElementalCardInstance, Vector3> targetPositions = new Dictionary<ElementalCardInstance, Vector3>();

    private void Awake()
    {
        instance = this;
    }
    public void AddCard(ElementalCardInstance card)
    {
        cardsInHand.Add(card);
        RepositionCardsSmooth();
    }

    public void RemoveCard(ElementalCardInstance card)
    {
        if (cardsInHand.Contains(card))
        {
            cardsInHand.Remove(card);
            RepositionCardsSmooth();
        }
    }

    private void RepositionCardsSmooth()
    {
        StopAllCoroutines();
        StartCoroutine(SmoothRepositionCoroutine());
    }
    private IEnumerator SmoothRepositionCoroutine()
    {
        if (cardsInHand.Count == 0)
            yield break;

        float totalWidth = (cardsInHand.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        Vector3[] targetPositions = new Vector3[cardsInHand.Count];
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            targetPositions[i] = new Vector3(startX + i * cardSpacing, 0f, 0f);
        }

        float elapsed = 0f;
        Vector3[] startPositions = new Vector3[cardsInHand.Count];
        for (int i = 0; i < cardsInHand.Count; i++)
            startPositions[i] = cardsInHand[i].transform.localPosition;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            for (int i = 0; i < cardsInHand.Count; i++)
                cardsInHand[i].transform.localPosition = Vector3.Lerp(startPositions[i], targetPositions[i], t);

            yield return null;
        }

        for (int i = 0; i < cardsInHand.Count; i++)
            cardsInHand[i].transform.localPosition = targetPositions[i];
    }
    public List<ElementalCardInstance> GetCards()
    {
        return cardsInHand;
    }
}
