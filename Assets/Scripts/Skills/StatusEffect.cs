using UnityEngine;
using System.Collections;
using System;

public abstract class StatusEffect : MonoBehaviour
{
    public string effectName;
    [TextArea(2, 3)]
    public string description;
    public GameObject effectPrefab;
    private GameObject effectObject;
    public int duration = 3;
    protected CardInstance target;
    public CardInstance owner => target;

    public virtual void Initialize(CardInstance targetUnit, StatusEffect origin, int power)
    {
        target = targetUnit;
        effectPrefab = origin.effectPrefab;
        duration = origin.duration;
        if (effectPrefab)
            effectObject = Instantiate(effectPrefab, target.transform);
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
    public virtual string GetDescription()
    {
        return "";
    }
    protected virtual void OnExpire()
    {
        Destroy(effectObject);
    }
    private void OnDestroy()
    {
        if(effectObject)
            Destroy(effectObject);
    }
    public virtual void Reapply(StatusEffect newEffect, int power)
    {
        // status reaply logic. This method is for child classes
    }

    public virtual IEnumerator OnTurnStartCoroutine()
    {
        OnTurnStartInstant();
        yield return null;
    }
}
