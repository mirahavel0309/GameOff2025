using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrantStatusEffectSkill : BaseSkill
{
    [Header("Skill Visuals")]
    public GameObject effectPrefab;
    public ElementIconLibrary elementsLib;

    [Header("Projectile Timing")]
    public float elementalRiseHeight = 2.0f;
    public float elementalLaunchDuration = 0.4f;
    public float mergeDelay = 0.3f;
    public float effectDuration = 0.6f;
    public ElementType mainElement;
    public StatusEffect statusEffect;
    public bool singleTarget = true;
    public AudioClip effectSound;

    public override IEnumerator Execute()
    {
        if (singleTarget)
            yield return GameManager.Instance.StartCoroutine(SingleTargetVersion());
        else
            yield return GameManager.Instance.StartCoroutine(AllHeroesVersion());
    }

    private IEnumerator SingleTargetVersion()
    {
        Debug.Log("Select friendly target to heal...");
        GameManager.Instance.SelectedTarget = null;
        InfoPanel.instance.ShowMessage("Select ally to heal...");

        // Wait for player to select a friendly hero
        CardInstance target = null;
        while (target == null)
        {
            // Wait for any target to be selected
            yield return new WaitUntil(() => GameManager.Instance.SelectedTarget != null);

            CardInstance clickedTarget = GameManager.Instance.SelectedTarget;
            GameManager.Instance.SelectTarget(null);

            // Validate by tag
            if (clickedTarget != null && clickedTarget.CompareTag("Player"))
            {
                target = clickedTarget;
            }
            else
            {
                Debug.Log("Invalid target! You can only heal friendly units.");
                InfoPanel.instance.ShowMessage("Invalid target! Select a friendly unit to heal...");
                // Small delay to avoid instant re-trigger
                yield return new WaitForSeconds(0.1f);
            }
        }

        InfoPanel.instance.Hide();
        GameManager.Instance.SelectTarget(null);

        // Launch elemental projectiles above heroes
        yield return GameManager.Instance.StartCoroutine(
            PerformElementalLaunches(elementsLib, requiredElements, elementalRiseHeight, elementalLaunchDuration, mergeDelay)
        );

        // Spawn healing effect
        if (effectPrefab != null)
        {
            GameObject healFx = GameObject.Instantiate(effectPrefab, target.transform.position, Quaternion.identity);
            GameObject.Destroy(healFx, effectDuration);
        }

        // Apply healing
        if (target != null)
        {
            HeroInstance mainHero = GameManager.Instance.GetHeroOfelement(mainElement);
            target.AddStatusEffect(statusEffect, mainHero.spellPower);
        }

        GameManager.Instance.SetPlayerInput(true);
    }

    private IEnumerator AllHeroesVersion()
    {
        yield return GameManager.Instance.StartCoroutine(
            PerformElementalLaunches(
                elementsLib,
                requiredElements,
                elementalRiseHeight,
                elementalLaunchDuration,
                mergeDelay
            )
        );

        yield return new WaitForSeconds(0.1f);

        List<HeroInstance> heroes = GameManager.Instance.PlayerHeroes.Where(x => !x.isDefeated).ToList();
        HeroInstance mainHero = GameManager.Instance.GetHeroOfelement(mainElement);

        foreach (var hero in heroes)
        {
            if (hero == null) continue;

            PassiveSkill existing = hero.GetComponent(statusEffect.GetType()) as PassiveSkill;
            if (existing != null)
            {
                GameObject.Destroy(existing);
            }
            hero.AddStatusEffect(statusEffect, mainHero.spellPower);
        }
        if(effectSound != null)
            EffectsManager.instance.CreateSoundEffect(effectSound, mergePoint);

        GameManager.Instance.SetPlayerInput(true);
    }
}
