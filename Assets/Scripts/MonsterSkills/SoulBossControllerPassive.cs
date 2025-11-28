using System.Collections;
using UnityEngine;

public class SoulBossControllerPassive : PassiveSkill
{
    [Header("Skills To Cycle Through (in order)")]
    public BaseMonsterSkill[] skills;

    private int currentIndex = 0;

    public override IEnumerator OnTurnStart()
    {
        if (skills == null || skills.Length == 0)
        {
            Debug.LogWarning("SoulBossControllerPassive: No skills assigned!");
            yield break;
        }

        // Disable all skills
        foreach (var skill in skills)
        {
            if (skill != null)
                skill.enabled = false;
        }

        // Enable next skill
        BaseMonsterSkill selected = skills[currentIndex];
        if (selected != null)
        {
            selected.enabled = true;
            Debug.Log($"SoulBossControllerPassive: Enabled skill '{selected.name}' at index {currentIndex}");
        }

        // Advance index for next turn
        currentIndex++;
        if (currentIndex >= skills.Length)
            currentIndex = 0;   // loop back
    }
}
