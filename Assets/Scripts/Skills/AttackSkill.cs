using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public override void Execute()
    {
        GameManager.Instance.StartCoroutine(WaitForTargetAndAttack());
    }

    private IEnumerator WaitForTargetAndAttack()
    {
        GameManager.Instance.SetPlayerInput(false);

        Debug.Log("Select enemy target...");
        GameManager.Instance.SelectedTarget = null;
        InfoPanel.instance.ShowMessage("Select enemy as target...");

        yield return new WaitUntil(() => GameManager.Instance.SelectedTarget != null);

        InfoPanel.instance.Hide();
        var target = GameManager.Instance.SelectedTarget;
        GameManager.Instance.SelectTarget(null);

        // Example: perform attack animation
        Debug.Log($"Attacking {target.name}!");

        yield return GameManager.Instance.StartCoroutine(PerformAttackVisuals(target));

        target.TakeDamage(baseDamage, damageType);
        if (statusEffect != null)
        {
            int roll = Random.Range(0, 100);
            if (roll < chanceToProc)
            {
                Debug.Log($"Applying {statusEffect.effectName} to {target.name} ({roll}% < {chanceToProc}%)");
                target.AddStatusEffect(statusEffect);
            }
            else
            {
                Debug.Log($"Effect {statusEffect.effectName} did not proc ({roll}% >= {chanceToProc}%)");
            }
        }

        if (onHitEffect)
            Instantiate(onHitEffect, target.transform.position, Quaternion.identity);

        GameManager.Instance.SetPlayerInput(true);
        GameManager.Instance.RegisterActionUse();
    }
    private IEnumerator PerformAttackVisuals(CardInstance target)
    {
        List<HeroInstance> contributingHeroes = new List<HeroInstance>();
        foreach (var hero in GameManager.Instance.PlayerHeroes)
        {
            if (requiredElements.Contains(hero.mainElement))
                contributingHeroes.Add(hero);
        }

        if (contributingHeroes.Count == 0)
        {
            Debug.LogWarning("No heroes found for AttackSkill: " + skillName);
            yield break;
        }

        List<GameObject> elementalProjectiles = new List<GameObject>();

        foreach (var hero in contributingHeroes)
        {
            GameObject projectilePrefab = elementsLib.GetElementProjectilePrefab(hero.mainElement);

            if (projectilePrefab == null)
            {
                Debug.LogWarning($"No projectile prefab found for element {hero.mainElement}");
                continue;
            }

            GameObject proj = Instantiate(projectilePrefab, hero.transform.position, Quaternion.identity);
            elementalProjectiles.Add(proj);

            Vector3 riseTarget = hero.transform.position + Vector3.up * elementalRiseHeight;
            StartCoroutine(MoveProjectile(proj, riseTarget, elementalLaunchDuration));
        }

        yield return new WaitForSeconds(elementalLaunchDuration + mergeDelay);

        Vector3 mergePoint = Vector3.zero;
        foreach (var p in elementalProjectiles)
            mergePoint += p.transform.position;
        mergePoint /= elementalProjectiles.Count;

        if (formedProjectilePrefab != null)
        {
            GameObject formedProjectile = Instantiate(formedProjectilePrefab, mergePoint, Quaternion.identity);

            // Clean up old projectiles (simulate merge)
            foreach (var p in elementalProjectiles)
                Destroy(p);

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

    private IEnumerator MoveProjectile(GameObject projectile, Vector3 targetPos, float duration)
    {
        Vector3 startPos = projectile.transform.position;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            projectile.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }
        projectile.transform.position = targetPos;
    }
}
