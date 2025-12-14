using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StormSkill : BaseSkill
{
    [Header("Storm Prefab")]
    public GameObject stormPrefab;

    [Header("Damage Settings")]
    public int baseDamage = 15;
    public ElementType damageType;
    public int baseAccuracy = 85;
    public StatusEffect statusEffect;   // The effect to apply
    [Range(0, 100)]
    public int chanceToProc = 0;        // % chance to apply

    [Header("Elemental Visuals")]
    public ElementIconLibrary elementsLib;
    public float elementalRiseHeight = 2.0f;
    public float elementalLaunchDuration = 0.4f;
    public float mergeDelay = 0.3f;

    [Header("Motion Settings")]
    public float travelDuration = 1.2f;
    public float returnDuration = 0.8f;

    [Header("SFX")]
    public AudioClip soundStormStart;
    public AudioClip soundStormEnd;

    public override IEnumerator Execute()
    {
        yield return GameManager.Instance.StartCoroutine(ExecuteRoutine());
    }
    public override string UpdatedDescription()
    {
        HeroInstance hero = GameManager.Instance.GetHeroOfelement(damageType);
        return description.Replace("<damage>", Mathf.RoundToInt(baseDamage * (hero.spellPower / 100f)).ToString());
    }

    private IEnumerator ExecuteRoutine()
    {
        GameManager.Instance.SetPlayerInput(false);

        // STEP 1: elemental launches
        yield return GameManager.Instance.StartCoroutine(
            PerformElementalLaunches(
                elementsLib,
                requiredElements,
                elementalRiseHeight,
                elementalLaunchDuration,
                mergeDelay
            )
        );

        List<CardInstance> enemies = GameManager.Instance.GetEnemies().ToList();
        if (enemies.Count == 0)
        {
            GameManager.Instance.SetPlayerInput(true);
            yield break;
        }

        Vector3 enemyMid = CalculateMidpoint(enemies);

        List<HeroInstance> heroes = GameManager.Instance.PlayerHeroes;
        Vector3 heroMid = CalculateMidpoint(heroes.Select(h => h as CardInstance).ToList());

        Vector3 spawnMid = heroMid;// (heroMid) * 0.5f;

        
        GameObject storm = Instantiate(stormPrefab, spawnMid, Quaternion.identity);

        if (soundStormStart)
            EffectsManager.instance.CreateSoundEffect(soundStormStart, spawnMid);

        storm.transform.localScale = Vector3.one * 0.1f;

        yield return StartCoroutine(MoveAndGrow(storm, spawnMid, enemyMid));

        HeroInstance hero = GameManager.Instance.GetHeroOfelement(damageType);
        int damage = Mathf.RoundToInt(baseDamage * (hero.spellPower / 100f));

        foreach (var enemy in enemies)
        {
            if (enemy != null)
                enemy.TakeDamage(damage, damageType, baseAccuracy);
            if (statusEffect != null)
            {
                int roll = Random.Range(0, 100);
                if (roll < chanceToProc)
                {
                    enemy.AddStatusEffect(statusEffect, hero.spellPower);
                }
            }
        }

        Vector3 beyondPoint = enemyMid + (enemyMid - spawnMid); // same distance past the enemy
        yield return StartCoroutine(MoveAndShrink(storm, enemyMid, beyondPoint));

        if (soundStormEnd)
            EffectsManager.instance.CreateSoundEffect(soundStormEnd, beyondPoint);

        Destroy(storm);

        foreach (var enemy in enemies)
            yield return StartCoroutine(enemy.ResolveDeathIfNeeded());
        GameManager.Instance.SetPlayerInput(true);
        GameManager.Instance.RegisterActionUse();
    }

    // ---------------------------------------------------------------------
    // MOTION HELPERS
    // ---------------------------------------------------------------------

    private IEnumerator MoveAndGrow(GameObject obj, Vector3 start, Vector3 end)
    {
        float t = 0f;
        Vector3 startScale = Vector3.one * 0.1f;
        Vector3 endScale = Vector3.one * 1.0f;

        while (t < travelDuration)
        {
            float lerp = t / travelDuration;
            obj.transform.position = Vector3.Lerp(start, end, lerp);
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, lerp);

            t += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = end;
        obj.transform.localScale = endScale;
    }


    private IEnumerator MoveAndShrink(GameObject obj, Vector3 start, Vector3 end)
    {
        float t = 0f;
        Vector3 startScale = Vector3.one * 1.0f;
        Vector3 endScale = Vector3.one * 0.1f;

        while (t < returnDuration)
        {
            float lerp = t / returnDuration;
            obj.transform.position = Vector3.Lerp(start, end, lerp);
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, lerp);

            t += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = end;
        obj.transform.localScale = endScale;
    }

    private Vector3 CalculateMidpoint(List<CardInstance> cards)
    {
        Vector3 sum = Vector3.zero;

        foreach (var c in cards)
            sum += c.transform.position;

        return sum / cards.Count;
    }
}
