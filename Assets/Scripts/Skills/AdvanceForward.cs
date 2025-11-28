using System.Collections;
using UnityEngine;

public class AdvanceForward : StatusEffect
{
    public ElementType element;

    public override void Initialize(CardInstance targetUnit, StatusEffect origin, int power)
    {
        base.Initialize(targetUnit, origin, power);


        target = targetUnit;
        EffectsManager.instance.CreateFloatingText(target.transform.position, "advance", Color.black);

    }

    public IEnumerator AdvanceAction()
    {
        target.speedCount = (int)(target.speedCount * 0.7);
        yield return new WaitForSeconds(0.5f);
        OnExpire();
    }

    public override void Reapply(StatusEffect newEffect, int power)
    {

    }

    protected override void OnExpire()
    {
        base.OnExpire();
    }
}
