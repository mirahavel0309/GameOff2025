using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MassHealSkill : BaseSkill
{
    [Header("Skill Visuals")]
    public GameObject healEffectPrefab;
    public ElementIconLibrary elementsLib;
    public AudioClip healSound;

    [Header("Projectile Timing")]
    public float elementalRiseHeight = 2.0f;
    public float elementalLaunchDuration = 0.4f;
    public float mergeDelay = 0.3f;
    public float healEffectDuration = 0.6f;

    [Header("Healing")]
    public int baseHeal = 8;

    public override void Execute()
    {
        GameManager.Instance.StartCoroutine(PerformMassHeal());
    }
    public override string UpdatedDescription()
    {
        HeroInstance hero = GameManager.Instance.GetHeroOfelement(ElementType.Nature);
        return description.Replace("<damage>", Mathf.RoundToInt(baseHeal * (hero.spellPower / 100f)).ToString());
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

        // Find all friendly units
        GameObject[] allUnits = GameObject.FindGameObjectsWithTag("Player");
        List<CardInstance> friendlyUnits = new List<CardInstance>();

        foreach (GameObject unitObj in allUnits)
        {
            CardInstance card = unitObj.GetComponent<CardInstance>();
            if (card != null)
                friendlyUnits.Add(card);
        }

        HeroInstance hero = GameManager.Instance.GetHeroOfelement(ElementType.Nature);
        // Apply healing to each friendly unit
        foreach (var target in friendlyUnits)
        {
            if (healEffectPrefab != null)
            {
                GameObject healFx = GameObject.Instantiate(
                    healEffectPrefab,
                    target.transform.position,
                    Quaternion.identity
                );
                GameObject.Destroy(healFx, healEffectDuration);
            }

            target.Heal(Mathf.RoundToInt(baseHeal * (hero.spellPower / 100f)));
            if (healSound)
                EffectsManager.instance.CreateSoundEffect(healSound, Vector3.zero);

            // Small stagger for visual pacing
            yield return new WaitForSeconds(0.1f);
        }

        InfoPanel.instance.Hide();

        GameManager.Instance.SetPlayerInput(true);
        GameManager.Instance.RegisterActionUse();
    }
}
