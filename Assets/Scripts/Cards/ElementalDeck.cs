using System.Collections.Generic;
using UnityEngine;

public class ElementalDeck : MonoBehaviour
{
    public List<ElementType> availableElements = new List<ElementType>
    {
        //ElementType.Fire,
        //ElementType.Water,
        //ElementType.Wind
    };

    [Header("References")]
    public GameObject elementalCardPrefab; // Prefab with ElementalCardInstance script
    public PlayerHand playerHand;

    private void Start()
    {
        //DrawMultiple(5);
    }
    public void DrawCard()
    {
        if (availableElements.Count == 0)
        {
            Debug.LogWarning("No elements defined in deck!");
            return;
        }

        // Pick random element type
        ElementType element =
            availableElements[Random.Range(0, availableElements.Count)];

        // Instantiate and add to hand
        GameObject cardGO = Instantiate(elementalCardPrefab, transform.position, Quaternion.identity, playerHand.transform);
        ElementalCardInstance card = cardGO.GetComponent<ElementalCardInstance>();
        card.Initialize(element);

        playerHand.AddCard(card);
    }

    // Optional: Draw multiple cards at once
    public void DrawMultiple(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if(playerHand.GetCards().Count >= 5)
            return;
            DrawCard();
        }
    }

    internal void DrawUntilHandIsFull()
    {
        int cardsNeeded = 5 - playerHand.GetCards().Count;
        if (cardsNeeded > 0)
        {
            for (int i = 0; i < cardsNeeded; i++)
                DrawCard();
        }
    }
}
