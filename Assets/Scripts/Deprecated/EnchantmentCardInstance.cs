using UnityEngine;

public enum Elements{Fire, Water, Nature, Wind};
public class EnchantmentCardInstance : CardInstance
{
    //public enum Elements { Fire, Water, Nature, Wind }
    public Elements elementType;
    public override void Initialize()
    {
    }
    protected override void HandleLeftClick()
    {
        if (!GameManager.Instance.PlayerInputEnabled)
            return;

        var selectedHero = GameManager.Instance.GetSelectedHero();

        if (selectedHero == null)
        {
            Debug.Log("No hero selected. Select a hero before using an enchantment card.");
            return;
        }


        // Remove from hand after use
        //GameManager.Instance.playerHand.RemoveCard(this);

        // Optionally destroy the card's GameObject (since it's consumed)
        Destroy(gameObject, 0.1f);
    }
}
