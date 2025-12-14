using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LifeDrainSkill : BaseSkill
{
    [Header("Skill Visuals")]
    public GameObject formedProjectilePrefab; // projectile that spawns AT target
    public GameObject drainExplosionPrefab;   // explosion at midpoint of heroes

    [Header("Projectile Motion")]
    public float projectileSpeed = 6f;
    public float explosionDuration = 0.6f;

    [Header("Damage / Healing")]
    public int baseDamage = 8;

    public override IEnumerator Execute()
    {
        yield return GameManager.Instance.StartCoroutine(WaitForTargetAndDrain());
    }
    public override string UpdatedDescription()
    {
        HeroInstance hero = GameManager.Instance.GetHeroOfelement(ElementType.Nature);
        return description.Replace("<damage>", Mathf.RoundToInt(baseDamage * (hero.spellPower / 100f)).ToString());
    }

    private IEnumerator WaitForTargetAndDrain()
    {
        GameManager.Instance.SetPlayerInput(false);

        Debug.Log("Select enemy target to drain life...");
        GameManager.Instance.SelectedTarget = null;
        InfoPanel.instance.ShowMessage("Select enemy as target...");

        // Wait for target
        yield return new WaitUntil(() => GameManager.Instance.SelectedTarget != null);

        InfoPanel.instance.Hide();
        var target = GameManager.Instance.SelectedTarget;
        GameManager.Instance.SelectTarget(null);

        // Validate target
        if (target == null || !target.CompareTag("Enemy"))
        {
            Debug.Log("Invalid target for Life Drain.");
            GameManager.Instance.SetPlayerInput(true);
            yield break;
        }

        // Damage step
        HeroInstance mainHero = GameManager.Instance.GetHeroOfelement(ElementType.Nature);
        int damageToDeal = Mathf.RoundToInt(baseDamage * (mainHero.spellPower / 100f));
        int realDamageDone =  target.TakeDamage(damageToDeal, ElementType.Nature);

        // Spawn formed projectile at target
        GameObject projectile = Instantiate(
            formedProjectilePrefab,
            target.transform.position,
            Quaternion.identity
        );

        // Now find midpoint of all friendly heroes
        List<HeroInstance> heroes = GameManager.Instance.PlayerHeroes;

        if (heroes == null || heroes.Count == 0)
        {
            Debug.LogWarning("No heroes found to receive life drain healing.");
            Destroy(projectile);
            GameManager.Instance.SetPlayerInput(true);
            yield break;
        }

        Vector3 midPoint = Vector3.zero;
        foreach (var hero in heroes)
            midPoint += hero.transform.position;

        midPoint /= heroes.Count;

        // Move projectile to midpoint
        yield return MoveProjectile(projectile, midPoint, projectileSpeed);

        // Explosion effect
        if (drainExplosionPrefab != null)
        {
            GameObject boom = Instantiate(drainExplosionPrefab, midPoint, Quaternion.identity);
            Destroy(boom, explosionDuration);
        }

        Destroy(projectile);

        // Distribute healing
        int totalHeal = Mathf.Max(1, realDamageDone); // minimum heal = 1 total
        DistributeHealing(heroes, totalHeal);

        yield return StartCoroutine(target.ResolveDeathIfNeeded());
        GameManager.Instance.SetPlayerInput(true);
        GameManager.Instance.RegisterActionUse();
    }

    private IEnumerator MoveProjectile(GameObject proj, Vector3 targetPos, float speed)
    {
        if (proj == null) yield break;

        while (Vector3.Distance(proj.transform.position, targetPos) > 0.1f)
        {
            proj.transform.position = Vector3.MoveTowards(
                proj.transform.position,
                targetPos,
                speed * Time.deltaTime
            );
            yield return null;
        }

        proj.transform.position = targetPos;
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

        if(healPerHero > 0)
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