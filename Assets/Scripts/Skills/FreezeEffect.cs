using System.Collections;
using UnityEngine;

public class FreezeEffect : StatusEffect
{
    public override void Initialize(CardInstance targetUnit, StatusEffect origin)
    {
        base.Initialize(targetUnit, origin);
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
    public override void Reapply(StatusEffect newEffect)
    {
        // do nothing freeze effects don't stack
    }

    protected override void OnExpire()
    {
        base.OnExpire();
    }
}
