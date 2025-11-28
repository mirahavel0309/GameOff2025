using System.Collections;
using UnityEngine;

public class SummonerControllerPassive : PassiveSkill
{
    [Header("Controlled Monster Skills")]
    public BaseMonsterSkill attackSkill;
    public BaseMonsterSkill summonSkill;
    public BaseMonsterSkill healSkill;

    public override IEnumerator OnTurnStart()
    {
        var enemyField = GameManager.Instance.enemyField;
        int count = enemyField.GetCards().Count;

        //focus on summoning
        if (count <= 2)
        {
            SetSkillEnabled(attackSkill, false);
            SetSkillEnabled(healSkill, false);
            SetSkillEnabled(summonSkill, true);
            yield break; ;
        }

        // all skills enabled
        if (count > 2 && count <= 4)
        {
            SetSkillEnabled(attackSkill, true);
            SetSkillEnabled(healSkill, true);
            SetSkillEnabled(summonSkill, true);
            yield break; ;
        }

        // disable summoning
        if (count > 4)
        {
            SetSkillEnabled(attackSkill, true);
            SetSkillEnabled(healSkill, true);
            SetSkillEnabled(summonSkill, false);
        }
        yield break;
    }

    private void SetSkillEnabled(BaseMonsterSkill skill, bool enabled)
    {
        if (skill == null) return;
        skill.enabled = enabled;
    }
}
