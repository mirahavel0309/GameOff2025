using UnityEngine;

public enum Elements{Fire, Water, Nature, Wind};
public class EnchantmentCardInstance : CardInstance
{
    //public enum Elements { Fire, Water, Nature, Wind }
    public Elements elementType;
    public override void Initialize()
    {
        attackText.enabled = false;
        healthText.enabled = false;
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

        ApplyEnchantment(selectedHero);

        // Remove from hand after use
        //GameManager.Instance.playerHand.RemoveCard(this);

        // Optionally destroy the card's GameObject (since it's consumed)
        Destroy(gameObject, 0.1f);
    }

    private void ApplyEnchantment(HeroInstance hero)
    {
        // Basic test effect: +1 to current attack
        hero.currentAttack += 1;
        hero.UpdateVisuals();

        //Debug.Log($"{cardData.cardName} used! {hero.cardData.cardName}'s attack increased by 1.");
    }
}
