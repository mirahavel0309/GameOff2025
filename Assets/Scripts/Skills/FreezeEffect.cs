using System.Collections;
using UnityEngine;

public class FreezeEffect : StatusEffect
{
    //public int speedPenalty;
    private int originalSpeed;
    public override void Initialize(CardInstance targetUnit, StatusEffect origin, int power)
    {
        base.Initialize(targetUnit, origin, power);
        EffectsManager.instance.CreateFloatingText(target.transform.position, "Frozen", Color.black);
        originalSpeed = target.speed;
        target.speed -= Mathf.RoundToInt(target.speed * 0.5f);
        target.speedCount += 100;
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
        // do nothing freeze effects don't stack
    }

    protected override void OnExpire()
    {
        target.speed = originalSpeed;
        base.OnExpire();
    }
}
