using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MassDamageSkill : BaseSkill
{
    [Header("Skill Visuals")]
    public GameObject damageEffectPrefab;
    public ElementIconLibrary elementsLib;

    [Header("Projectile Timing")]
    public float elementalRiseHeight = 2.0f;
    public float elementalLaunchDuration = 0.4f;
    public float mergeDelay = 0.3f;
    public float damageEffectDuration = 0.6f;

    [Header("Damage")]
    public int baseDamage = 8;
    public ElementType damageElement;
    public int baseAccuracy = 90;
    [Header("Status Effect")]
    public StatusEffect statusEffect;
    [Range(0, 100)]
    public int chanceToProc = 0;

    public override void Execute()
    {
        GameManager.Instance.StartCoroutine(PerformMassHeal());
    }
    public override string UpdatedDescription()
    {
        HeroInstance hero = GameManager.Instance.GetHeroOfelement(damageElement);
        return description.Replace("<damage>", Mathf.RoundToInt(baseDamage * (hero.spellPower / 100f)).ToString());
    }

    private IEnumerator PerformMassHeal()
    {
        GameManager.Instance.SetPlayerInput(false);
        InfoPanel.instance.ShowMessage("Performing mass healing...");

        // Launch visual projectiles from heroes who match required elements
        yield return GameManager.Instance.StartCoroutine(
            PerformElementalLaunches(
                elementsLib,
                requiredElements,
                elementalRiseHeight,
                elementalLaunchDuration,
                mergeDelay
            )
        );

        List<CardInstance> allUnits = GameManager.Instance.GetEnemies();
        List<CardInstance> friendlyUnits = new List<CardInstance>();

        foreach (CardInstance card in allUnits)
        {
            //CardInstance card = unitObj.GetComponent<CardInstance>();
            if (card != null)
                friendlyUnits.Add(card);
        }

        foreach (var target in friendlyUnits)
        {
            if (damageEffectPrefab != null)
            {
                GameObject healFx = GameObject.Instantiate(
                    damageEffectPrefab,
                    target.transform.position,
                    Quaternion.identity
                );
                GameObject.Destroy(healFx, damageEffectDuration);
            }

            HeroInstance hero = GameManager.Instance.GetHeroOfelement(damageElement);
            int damageDealth = target.TakeDamage(Mathf.RoundToInt(baseDamage * (hero.spellPower / 100f)), damageElement, baseAccuracy);

            if (statusEffect != null && damageDealth > 0)
            {
                int roll = Random.Range(0, 100);
                if (roll < chanceToProc)
                {
                    Debug.Log($"Applying {statusEffect.effectName} to {target.name} ({roll}% < {chanceToProc}%)");
                    target.AddStatusEffect(statusEffect, hero.spellPower);
                }
                else
                {
                    Debug.Log($"Effect {statusEffect.effectName} did not proc ({roll}% >= {chanceToProc}%)");
                }
            }

            // Small stagger for visual pacing
            yield return new WaitForSeconds(0.1f);
        }

        InfoPanel.instance.Hide();

        GameManager.Instance.SetPlayerInput(true);
        GameManager.Instance.RegisterActionUse();
    }
}
