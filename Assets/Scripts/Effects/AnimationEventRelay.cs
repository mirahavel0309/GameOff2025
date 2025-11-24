using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private AttackMonsterSkill[] monsterSkills;

    private void Awake()
    {
        monsterSkills = GetComponentsInParent<AttackMonsterSkill>();
    }
    public void OnProjectileCastEvent()
    {
        foreach (var skill in monsterSkills)
        {
            skill.OnProjectileCastEvent();
        }
    }
}
