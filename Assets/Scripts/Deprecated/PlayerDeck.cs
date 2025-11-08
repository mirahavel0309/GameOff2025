using System.Collections.Generic;
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    // old iteratrion! Deprecated!
    [Header("Deck Setup")]
    [SerializeField] private List<Card> cards = new List<Card>();
    [SerializeField] private GameObject cardPrefab; // Prefab containing CardInstance
    [SerializeField] private PlayerHand playerHand; // Reference to player's hand

    private System.Random rng = new System.Random();

    private void Start()
    {
        Shuffle();
    }

    public void Shuffle()
    {
        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (cards[k], cards[n]) = (cards[n], cards[k]);
        }
    }

    public void Initialize(List<Card> deckList, PlayerHand ownerHand)
    {
        cards = new List<Card>(deckList);
        playerHand = ownerHand;
        Shuffle();
    }

    public void Draw()
    {
        if (cards.Count == 0)
        {
            Debug.LogWarning("Deck is empty!");
            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError("Card Prefab not assigned to PlayerDeck!");
            return;
        }

        Card cardToDraw = cards[0];
        cards.RemoveAt(0);

        // Instantiate the prefab and set card data
        GameObject newCardObj = Instantiate(cardPrefab);
        newCardObj.transform.position = transform.position;
        CardInstance cardInstance = newCardObj.GetComponent<CardInstance>();

        if (cardInstance == null)
        {
            Debug.LogError("Card prefab missing CardInstance component!");
            return;
        }

        cardInstance.SetCardData(cardToDraw);
        cardInstance.ChangeState(CardState.InHand);

        //// Add to player hand
        //if (playerHand != null)
        //{
        //    playerHand.AddCard(cardInstance);
        //    cardInstance.currentContainer = playerHand;
        //}
        //else
        //{
        //    Debug.LogError("PlayerHand not assigned to PlayerDeck!");
        //}
    }
}
