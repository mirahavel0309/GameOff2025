using System.Collections;
using UnityEngine;

public class RegenerationPassive : PassiveSkill
{
    public float healRate = 0.2f;

    public override IEnumerator OnTurnStart()
    {
        if (owner.CurrentHealth > 0) //skip heal is HmiPlatform is below 0
            owner.Heal(Mathf.RoundToInt(owner.maxHealth * healRate));
        yield return new WaitForSeconds(0.15f);
    }
}