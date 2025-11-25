using System.Collections;
using UnityEngine;

public abstract class PassiveSkill : MonoBehaviour
{
    protected CardInstance owner;
    private void Start()
    {
        Initialize();
    }

    public virtual void Initialize()
    {
        owner = GetComponent<CardInstance>();
    }

    public virtual void OnApply() { }

    public virtual int ModifyAttack(int value) => value;
    public virtual int ModifyDefense(int value) => value;
    public virtual IEnumerator OnTurnStart() { yield break;  }

    public virtual int OnDamageTaken(int incomingDamage) => incomingDamage;
}
