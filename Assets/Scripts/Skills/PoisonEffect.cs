using System.Collections;
using UnityEngine;

public class PoisonEffect : StatusEffect
{
    public int damagePerTurn = 3;

    public override void Initialize(CardInstance targetUnit, StatusEffect origin, int power)
    {
        base.Initialize(targetUnit, origin, power);
        PoisonEffect originEffect = (PoisonEffect)origin;

        damagePerTurn = Mathf.RoundToInt(originEffect.damagePerTurn * power * 0.01f);
        target = targetUnit;
        EffectsManager.instance.CreateFloatingText(target.transform.position, "Poisoned", Color.black);
    }
    public override string GetDescription()
    {
        return $"Poisoned:\n  damage: {damagePerTurn}\n  duration: {duration}";
    }
    public override IEnumerator OnTurnStartCoroutine()
    {
        if (target == null)
            yield break;

        target.TakeDamage(damagePerTurn, ElementType.Nature, 100);

        // add visual effect for posin here and wait a bit
        yield return new WaitForSeconds(0.3f);
        yield return GameManager.Instance.StartCoroutine(target.ResolveDeathIfNeeded());

        duration--;
        if (duration <= 0)
        {
            OnExpire();
            Destroy(this);
        }
    }
    public override void Reapply(StatusEffect newEffect, int power)
    {
        duration = Mathf.Max(duration, newEffect.duration); 
        
        PoisonEffect newPoison = newEffect as PoisonEffect;
        damagePerTurn = Mathf.Max(damagePerTurn, Mathf.RoundToInt(newPoison.damagePerTurn * power * 0.01f));
    }

    protected override void OnExpire()
    {
        base.OnExpire();
    }
}
