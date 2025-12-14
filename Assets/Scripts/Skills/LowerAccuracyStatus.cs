using System.Collections;
using UnityEngine;

public class LowerAccuracyStatus : StatusEffect
{
    public int accuracyPenalty;
    public override void Initialize(CardInstance targetUnit, StatusEffect origin, int power)
    {
        base.Initialize(targetUnit, origin, power);
        LowerAccuracyStatus originAccStatus = origin as LowerAccuracyStatus;
        accuracyPenalty = originAccStatus.accuracyPenalty;
        EffectsManager.instance.CreateFloatingText(target.transform.position, "Accuracy down", Color.black);
    }
    public override string GetDescription()
    {
        return $"Lower accuracy:\n  Loss: {accuracyPenalty}\n  duration: {duration}";
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
        LowerAccuracyStatus newAccStatus = newEffect as LowerAccuracyStatus;
        duration = Mathf.Max(duration, newAccStatus.duration);
        accuracyPenalty = Mathf.Min(accuracyPenalty + newAccStatus.accuracyPenalty, 95); // penalties stack. 95 is max penalty
    }

    protected override void OnExpire()
    {
        base.OnExpire();
    }
}
