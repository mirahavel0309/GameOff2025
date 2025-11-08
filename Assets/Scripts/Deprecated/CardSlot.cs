using UnityEngine;

public class CardSlot : MonoBehaviour
{
    public CardInstance currentCard;
    public bool HasCard => currentCard != null;

    public void PlaceCard(CardInstance card)
    {
        currentCard = card;
        // Later you can instantiate a card visual prefab here
    }

    public void RemoveCard()
    {
        currentCard = null;
    }
}
