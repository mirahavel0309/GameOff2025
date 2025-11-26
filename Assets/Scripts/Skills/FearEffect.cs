using System.Collections;
using UnityEngine;

public class FearEffect : StatusEffect
{
    public ElementType affectedElement;
    public override void Initialize(CardInstance targetUnit, StatusEffect origin, int power)
    {
        base.Initialize(targetUnit, origin, power);
        EffectsManager.instance.CreateFloatingText(target.transform.position, "Fear!!", Color.black);

        affectedElement = ElementType.Physical;
        HeroInstance hero = targetUnit.GetComponent<HeroInstance>();
        if (hero)
        {
            affectedElement = hero.mainElement;
            PlayerHand.instance.RemoveCardsOfType(affectedElement);
            GameManager.Instance.RemoveElementFromDeck(affectedElement);
        }
    }
    public override IEnumerator OnTurnStartCoroutine()
    {
        // no effect here skip 
        duration--;
        if (duration <= 0)
        {
            OnExpire();
            Destroy(this);
        }
        yield return null;
    }
    public override void Reapply(StatusEffect newEffect, int power)
    {
        // do nothing
    }

    protected override void OnExpire()
    {
        base.OnExpire();
        GameManager.Instance.AddElementToDeck(affectedElement);
    }
}
