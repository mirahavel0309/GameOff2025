using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AttackSkill : BaseSkill
{
    [Header("Skill Projectile Prefabs")]
    public GameObject formedProjectilePrefab; // The final projectile prefab

    [Header("Projectile Timing")]
    public float elementalRiseHeight = 2.0f;
    public float elementalLaunchDuration = 0.4f;
    public float mergeDelay = 0.3f;
    public float travelDuration = 0.4f;
    public float impactDelay = 0.1f;
    public int baseDamage = 5;
    public ElementType damageType;
    public ElementIconLibrary elementsLib;
    public GameObject onHitEffect;
    [Header("Status Effect")]
    public StatusEffect statusEffect;   // The effect to apply
    [Range(0, 100)]
    public int chanceToProc = 0;        // % chance to apply
    public AudioClip soundLaunch;
    public AudioClip soundHit;
    private List<PassiveSkill> passiveSkills;
    public override IEnumerator Execute()
    {
        passiveSkills = GetComponents<PassiveSkill>().ToList();
        yield return GameManager.Instance.StartCoroutine(WaitForTargetAndAttack());
    }
    public void HighLightUnits(List<CardInstance> enemies)
    {
        foreach (var e in enemies)
        {
            if (e != null)
                e.ShowSelector(SelectionState.Red);
        }
    }
    public void HideHighlight(List<CardInstance> enemies)
    {
        foreach (var e in enemies)
        {
            if (e != null)
                e.HideSelector();
        }
    }
    public override string UpdatedDescription()
    {
        HeroInstance hero = GameManager.Instance.GetHeroOfelement(damageType);
        return description.Replace("<damage>", Mathf.RoundToInt(baseDamage * (hero.spellPower / 100f)).ToString());
    }
    public int GetAttackPower()
    {
        int final = baseDamage;

        foreach (var passive in passiveSkills)
            final = passive.ModifyAttack(final);

        return final;
    }

    private IEnumerator WaitForTargetAndAttack()
    {
        GameManager.Instance.SetPlayerInput(false);
        List<CardInstance> enemies = GameManager.Instance.enemyField.GetCards();

        HighLightUnits(enemies);

        GameManager.Instance.SelectedTarget = null;
        InfoPanel.instance.ShowMessage("Select enemy as target...");

        yield return new WaitUntil(() => GameManager.Instance.SelectedTarget != null);

        HideHighlight(enemies);
        InfoPanel.instance.Hide();
        var target = GameManager.Instance.SelectedTarget;
        GameManager.Instance.SelectTarget(null);

        yield return GameManager.Instance.StartCoroutine(PerformAttackVisuals(target));

        HeroInstance hero = GameManager.Instance.GetHeroOfelement(damageType);
        int damageDealth = target.TakeDamage(Mathf.RoundToInt(GetAttackPower() * (hero.spellPower / 100f)), damageType);
        if (statusEffect != null && damageDealth > 0)
        {
            int roll = Random.Range(0, 100);
            if (roll < chanceToProc)
            {
                target.AddStatusEffect(statusEffect, hero.spellPower);
            }
        }

        if (onHitEffect)
            Instantiate(onHitEffect, target.transform.position, Quaternion.identity);

        if (soundHit)
            EffectsManager.instance.CreateSoundEffect(soundHit, transform.position);

        yield return StartCoroutine(target.ResolveDeathIfNeeded());
        GameManager.Instance.SetPlayerInput(true);
        GameManager.Instance.RegisterActionUse();
    }
    private IEnumerator PerformAttackVisuals(CardInstance target)
    {
        yield return GameManager.Instance.StartCoroutine(
            PerformElementalLaunches(elementsLib, requiredElements, elementalRiseHeight, elementalLaunchDuration, mergeDelay)
        );

        if (formedProjectilePrefab != null)
        {
            GameObject formedProjectile = Instantiate(formedProjectilePrefab, mergePoint, Quaternion.identity);
            if (soundLaunch)
                EffectsManager.instance.CreateSoundEffect(soundLaunch, transform.position);

            yield return MoveProjectile(formedProjectile, target.transform.position, travelDuration);

            // Impact delay before damage application
            yield return new WaitForSeconds(impactDelay);

            Destroy(formedProjectile);
        }
        else
        {
            Debug.LogError("Formed projectile prefab missing in AttackSkill!");
        }
    }
}
