using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string playerName;
    public PlayerDeck deck;
    public PlayerHand hand;
    public int playerId;

    public Player(string name, PlayerDeck deckData, int id)
    {
        playerName = name;
        deck = deckData;
        hand = new PlayerHand();
        playerId = id;
    }

    public void DrawCard()
    {
        //CardInstance drawn = deck.Draw();
        //if (drawn != null)
        //    hand.AddCard(drawn);
    }
}
