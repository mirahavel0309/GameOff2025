using System.Collections;
using UnityEngine;

public class BurnEffect : StatusEffect
{
    public int damagePerTurn = 4;

    public override void Initialize(CardInstance targetUnit, StatusEffect origin)
    {
        base.Initialize(targetUnit, origin);
        BurnEffect originEffect = (BurnEffect)origin;

        damagePerTurn = originEffect.damagePerTurn;
        target = targetUnit;
    }
    public override IEnumerator OnTurnStartCoroutine()
    {
        if (target == null)
            yield break;

        target.TakeDamage(damagePerTurn, ElementType.Fire);

        // add visual effect for posin here and wait a bit
        yield return new WaitForSeconds(0.3f);

        // Handle duration countdown and expiration
        duration--;
        if (duration <= 0)
        {
            OnExpire();
            Destroy(this);
        }
    }
    public override void Reapply(StatusEffect newEffect)
    {
        duration++;

        BurnEffect newPoison = newEffect as BurnEffect;
        damagePerTurn += newPoison.damagePerTurn;
    }

    protected override void OnExpire()
    {
        base.OnExpire();
    }
}
