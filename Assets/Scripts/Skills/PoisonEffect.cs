using System.Collections;
using UnityEngine;

public class PoisonEffect : StatusEffect
{
    public int damagePerTurn = 3;

    public override void Initialize(CardInstance targetUnit, StatusEffect origin)
    {
        base.Initialize(targetUnit, origin);
        PoisonEffect originEffect = (PoisonEffect)origin;

        damagePerTurn = originEffect.damagePerTurn;
        target = targetUnit;
    }
    public override IEnumerator OnTurnStartCoroutine()
    {
        if (target == null)
            yield break;

        target.TakeDamage(damagePerTurn, ElementType.Nature);

        // add visual effect for posin here and wait a bit
        yield return new WaitForSeconds(0.3f);

        duration--;
        if (duration <= 0)
        {
            OnExpire();
            Destroy(this);
        }
    }
    public override void Reapply(StatusEffect newEffect)
    {
        duration = Mathf.Max(duration, newEffect.duration); 
        
        PoisonEffect newPoison = newEffect as PoisonEffect;
        damagePerTurn = Mathf.Max(damagePerTurn, newPoison.damagePerTurn);
    }

    protected override void OnExpire()
    {
        base.OnExpire();
    }
}
