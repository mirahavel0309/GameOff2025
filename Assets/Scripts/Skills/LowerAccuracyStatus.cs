using System.Collections;
using UnityEngine;

public class LowerAccuracyStatus : StatusEffect
{
    public int accuracyPenalty;
    public override void Initialize(CardInstance targetUnit, StatusEffect origin)
    {
        base.Initialize(targetUnit, origin);
        LowerAccuracyStatus originAccStatus = origin as LowerAccuracyStatus;
        accuracyPenalty = originAccStatus.accuracyPenalty;
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
        LowerAccuracyStatus newAccStatus = newEffect as LowerAccuracyStatus;
        duration = Mathf.Max(duration, newAccStatus.duration);
        accuracyPenalty = Mathf.Min(accuracyPenalty + newAccStatus.accuracyPenalty, 95); // penalties stack. 95 is max penalty
    }

    protected override void OnExpire()
    {
        base.OnExpire();
    }
}
