using System.Collections;
using UnityEngine;

public class TimedLifePassive : PassiveSkill
{
    public int turnsToLive = 3;
    private int turnsLeft;
    public override void Initialize()
    {
        base.Initialize();
        turnsLeft = turnsToLive;
    }
    public override IEnumerator OnTurnStart()
    {
        turnsLeft--;
        EffectsManager.instance.CreateFloatingText(transform.position, $"turns: {turnsLeft}", Color.black);
        yield return new WaitForSeconds(0.10f);

        if (turnsLeft <= 0)
        {
            yield return owner.Despawn();
        }

        yield return new WaitForSeconds(0.10f);
    }
}