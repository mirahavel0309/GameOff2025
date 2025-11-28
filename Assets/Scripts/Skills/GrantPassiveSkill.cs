using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrantPassiveSkill : BaseSkill
{
    [Header("Skill Visuals")]
    public GameObject effectPrefab;
    public ElementIconLibrary elementsLib;

    [Header("Projectile Timing")]
    public float elementalRiseHeight = 2.0f;
    public float elementalLaunchDuration = 0.4f;
    public float mergeDelay = 0.3f;
    public float effectDuration = 0.6f;

    [Header("Passive Skill to Grant")]
    public PassiveSkill passiveToGrant;   // Prefab or component reference
    public ElementType mainElement;

    public override void Execute()
    {
        GameManager.Instance.StartCoroutine(ApplyPassivesRoutine());
    }
    public override string UpdatedDescription()
    {
        HeroInstance hero = GameManager.Instance.GetHeroOfelement(mainElement);
        return description.Replace("<passive_info>", passiveToGrant.GetDescription(hero.spellPower));
    }

    private IEnumerator ApplyPassivesRoutine()
    {
        GameManager.Instance.SetPlayerInput(false);

        InfoPanel.instance.ShowMessage("Empowering all allies...");

        // Launch elemental projectiles above heroes (visuals)
        yield return GameManager.Instance.StartCoroutine(
            PerformElementalLaunches(
                elementsLib,
                requiredElements,
                elementalRiseHeight,
                elementalLaunchDuration,
                mergeDelay
            )
        );

        InfoPanel.instance.Hide();

        // Spawn global effect (optional)
        if (effectPrefab != null)
        {
            // Spawn once at center of team
            Vector3 midpoint = GetPlayerMidpoint();
            GameObject fx = GameObject.Instantiate(effectPrefab, midpoint, Quaternion.identity);
            GameObject.Destroy(fx, effectDuration);
        }

        yield return new WaitForSeconds(0.1f);

        // Apply passive to all allied heroes
        List<HeroInstance> heroes = GameManager.Instance.PlayerHeroes;
        HeroInstance mainHero = GameManager.Instance.GetHeroOfelement(mainElement);

        foreach (var hero in heroes)
        {
            if (hero == null) continue;

            // Remove old version if present
            PassiveSkill existing = hero.GetComponent(passiveToGrant.GetType()) as PassiveSkill;
            if (existing != null)
            {
                GameObject.Destroy(existing);
            }

            // Add new passive
            PassiveSkill newPassive = hero.gameObject.AddComponent(passiveToGrant.GetType()) as PassiveSkill;

            // Optional: copy over values from the assigned template
            CopyPassiveValues(passiveToGrant, newPassive);

            // Optional spellPower scaling
            newPassive.InitializeFromCaster(mainHero);
        }

        GameManager.Instance.SetPlayerInput(true);
        GameManager.Instance.RegisterActionUse();
    }

    private Vector3 GetPlayerMidpoint()
    {
        var heroes = GameManager.Instance.PlayerHeroes;
        if (heroes.Count == 0) return Vector3.zero;

        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (var hero in heroes)
        {
            if (hero != null)
            {
                sum += hero.transform.position;
                count++;
            }
        }

        return count > 0 ? sum / count : Vector3.zero;
    }

    private void CopyPassiveValues(PassiveSkill source, PassiveSkill target)
    {
        // Copies public fields so PassiveSkill prefabs can hold data
        var type = source.GetType();
        var fields = type.GetFields();

        foreach (var f in fields)
        {
            f.SetValue(target, f.GetValue(source));
        }
    }
}
