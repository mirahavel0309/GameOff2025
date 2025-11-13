using UnityEngine;
using System.Collections;
using System;

public abstract class StatusEffect : MonoBehaviour
{
    public string effectName;
    [TextArea(2, 3)]
    public string description;
    public GameObject effectPrefab;
    public int duration = 3;
    protected CardInstance target;

    public virtual void Initialize(CardInstance targetUnit, StatusEffect origin)
    {
        target = targetUnit;
    }
    public virtual void OnTurnStartInstant()
    {
        duration--;
        if (duration <= 0)
        {
            OnExpire();
            Destroy(this);
        }
    }
    protected virtual void OnExpire()
    {

    }
    public virtual void Reapply(StatusEffect newEffect)
    {
        // status reaply logic. This method is for child classes
    }

    public virtual IEnumerator OnTurnStartCoroutine()
    {
        OnTurnStartInstant();
        yield return null;
    }
}
