using System.Collections;
using UnityEngine;

public class RegenerationPassive : PassiveSkill
{
    public int healAmount = 1;

    public override IEnumerator OnTurnStart()
    {
        owner.Heal(healAmount);
        yield return new WaitForSeconds(0.15f);
    }
}