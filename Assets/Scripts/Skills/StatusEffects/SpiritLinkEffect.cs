using System.Collections;
using UnityEngine;

public class SpiritLinkEffect : StatusEffect
{
    public override void Initialize(CardInstance targetUnit, StatusEffect origin, int power)
    {
        base.Initialize(targetUnit, origin, power);
        SpiritLinkManager.Instance.Register(this);
    }
    public override string GetDescription()
    {
        return $"Spirit linked";
    }

    protected override void OnExpire()
    {
        SpiritLinkManager.Instance.Unregister(this);
        base.OnExpire();
    }

    public int OnIncomingDamage(int dmg)
    {
        SpiritLinkManager.Instance.QueueDamage(target, dmg);

        return 0;
    }
}
