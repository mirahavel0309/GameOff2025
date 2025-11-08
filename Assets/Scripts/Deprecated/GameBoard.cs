using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [Header("Card Slots")]
    public CardSlot[] player1Slots;
    public CardSlot[] player2Slots;

    // Maybe later, you’ll have methods like:
    public bool PlaceCard(CardInstance card, int slotIndex, int playerId)
    {
        // Example: simple logic
        CardSlot slot = (playerId == 1 ? player1Slots[slotIndex] : player2Slots[slotIndex]);
        if (slot.HasCard) return false;

        slot.PlaceCard(card);
        return true;
    }
}
