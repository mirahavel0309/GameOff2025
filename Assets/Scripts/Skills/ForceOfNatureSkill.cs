using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ForceOfNatureSkill : BaseSkill
{
    [Header("Skill Visuals")]
    public GameObject formedProjectilePrefab;   // projectile created at each poisoned enemy
    public GameObject explosionPrefab;          // final explosion at midpoint
    public PoisonEffect poisonEffect;

    [Header("Projectile Motion")]
    public float projectileSpeed = 6f;
    public float explosionDuration = 0.7f;
    public int baseDamage;
    public ElementIconLibrary elementsLib;
    public float elementalRiseHeight = 4.0f;
    public float elementalLaunchDuration = 0.8f;
    public float mergeDelay = 0.5f;

    public override IEnumerator Execute()
    {
        yield return GameManager.Instance.StartCoroutine(DoForceOfNature());
    }
    public override string UpdatedDescription()
    {
        HeroInstance hero = GameManager.Instance.GetHeroOfelement(ElementType.Nature);
        return description.Replace("<damage>", Mathf.RoundToInt(baseDamage * (hero.spellPower / 100f)).ToString()).Replace("<poison>", Mathf.RoundToInt(poisonEffect.damagePerTurn * (hero.spellPower / 100f)).ToString());
    }

    private IEnumerator DoForceOfNature()
    {
        GameManager.Instance.SetPlayerInput(false);
        InfoPanel.instance.ShowMessage("Unleashing Force of Nature...");

        List<CardInstance> enemyUnits = GameManager.Instance.GetEnemies().ToList();
        List<CardInstance> poisonedEnemies = new List<CardInstance>();
        List<PoisonEffect> poisonEffects = new List<PoisonEffect>();
        List<int> poisonDamageValues = new List<int>();
        HeroInstance hero = GameManager.Instance.GetHeroOfelement(ElementType.Nature);

        yield return GameManager.Instance.StartCoroutine(
            PerformElementalLaunches(
                elementsLib,
                requiredElements,
                elementalRiseHeight,
                elementalLaunchDuration,
                mergeDelay
            )
        );

        foreach (var enemy in enemyUnits)
        {
            if (enemy == null) continue;

            PoisonEffect poison = enemy.activeEffects
                .OfType<PoisonEffect>()
                .FirstOrDefault();

            if (poison != null)
            {
                poisonedEnemies.Add(enemy);
                poisonEffects.Add(poison);

                int damage = poison.duration * Mathf.RoundToInt(Mathf.RoundToInt(baseDamage * (hero.spellPower / 100f)));
                poisonDamageValues.Add(damage);
            }
            else
            {
                enemy.TakeDamage(Mathf.RoundToInt(Mathf.RoundToInt(poisonEffect.damagePerTurn * (hero.spellPower / 100f))), ElementType.Nature);
            }

            enemy.AddStatusEffect(poisonEffect, hero.spellPower);
            yield return StartCoroutine(enemy.ResolveDeathIfNeeded());
        }

        if (poisonedEnemies.Count == 0)
        {
            InfoPanel.instance.Hide();
            GameManager.Instance.SetPlayerInput(true);
            yield break;
        }

        int totalDamage = 0;
        List<GameObject> spawnedProjectiles = new List<GameObject>();

        for (int i = 0; i < poisonedEnemies.Count; i++)
        {
            var enemy = poisonedEnemies[i];
            var poison = poisonEffects[i];
            int dmg = poisonDamageValues[i];

            // Apply damage
            enemy.TakeDamage(dmg, ElementType.Nature);
            totalDamage += dmg;

            // Remove poison effect
            enemy.activeEffects.Remove(poison);
            Destroy(poison);

            if (formedProjectilePrefab != null && enemy != null)
            {
                GameObject proj = Instantiate(
                    formedProjectilePrefab,
                    enemy.transform.position,
                    Quaternion.identity
                );
                spawnedProjectiles.Add(proj);
            }

            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(enemy.ResolveDeathIfNeeded());
        }

        Debug.Log($"Force of Nature total damage dealt: {totalDamage}");

        List<HeroInstance> heroes = GameManager.Instance.PlayerHeroes;

        if (heroes.Count == 0)
        {
            Debug.LogWarning("No heroes found — cannot distribute healing.");
            foreach (var p in spawnedProjectiles) Destroy(p);
            InfoPanel.instance.Hide();
            GameManager.Instance.SetPlayerInput(true);
            yield break;
        }

        Vector3 midPoint = Vector3.zero;
        foreach (var h in heroes)
            midPoint += h.transform.position;
        midPoint /= heroes.Count;

        foreach (var proj in spawnedProjectiles)
        {
            if (proj != null)
                yield return MoveProjectile(proj, midPoint, projectileSpeed);
        }

        if (explosionPrefab != null)
        {
            GameObject boom = Instantiate(explosionPrefab, midPoint, Quaternion.identity);
            Destroy(boom, explosionDuration);
        }

        // Clean up projectiles
        foreach (var proj in spawnedProjectiles)
            if (proj != null) Destroy(proj);

        DistributeHealing(heroes, totalDamage);

        InfoPanel.instance.Hide();
        GameManager.Instance.SetPlayerInput(true);
    }

    private IEnumerator MoveProjectile(GameObject proj, Vector3 target, float speed)
    {
        if (proj == null) yield break;

        while (Vector3.Distance(proj.transform.position, target) > 0.1f)
        {
            proj.transform.position = Vector3.MoveTowards(
                proj.transform.position,
                target,
                speed * Time.deltaTime
            );
            yield return null;
        }

        proj.transform.position = target;
    }

    private void DistributeHealing(List<HeroInstance> heroes, int totalHeal)
    {
        if (totalHeal <= 0 || heroes.Count == 0)
            return;

        Dictionary<CardInstance, int> healAmounts = new Dictionary<CardInstance, int>();
        foreach (var hero in heroes)
            healAmounts.Add(hero, 0);

        int heroCount = heroes.Count;
        int healPerHero = totalHeal / heroCount;
        int leftover = totalHeal % heroCount;

        if (healPerHero > 0)
        {
            foreach (var hero in heroes)
            {
                healAmounts[hero] = healPerHero;
            }
        }
        if (leftover > 0)
        {
            List<HeroInstance> shuffled = heroes.OrderBy(h => Random.value).ToList();

            for (int i = 0; i < leftover; i++)
                healAmounts[shuffled[i]] += 1;
        }

        foreach (var row in healAmounts)
            row.Key.Heal(row.Value);
    }
}
