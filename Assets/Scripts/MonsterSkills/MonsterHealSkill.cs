using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterHealSkill : BaseMonsterSkill
{
    [Header("Heal Settings")]
    public int healAmount = 10;
    private CardInstance selectedTarget;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public GameObject hitPrefab;
    public float projectileSpeed = 8f;
    public Transform origin;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public override IEnumerator Execute(CardInstance target)
    {
        var allUnitsOnfield = GetComponent<CardInstance>().troopsField.GetCards();

        List<CardInstance> validTargets = allUnitsOnfield
            .Where(e =>
                e != null &&
                e != this.cardInstance && // cannot heal itself
                e.GetComponent<MonsterHealSkill>() == null // cannot heal other healers
                )
            .ToList();

        if (validTargets.Count == 0)
        {
            yield break;
        }

        // Pick a random ally to heal
        selectedTarget = validTargets[Random.Range(0, validTargets.Count)];

        if (animator)
            animator.SetTrigger("Cast");

        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(PerformHealProjectile(target));

        selectedTarget.Heal(healAmount);
        GameObject projectile = Instantiate(hitPrefab, selectedTarget.transform.position, Quaternion.identity);
    }

    private IEnumerator PerformHealProjectile(CardInstance target)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("MonsterHealSkill: projectilePrefab missing!");
            yield break;
        }

        Vector3 start = origin.position;
        Vector3 end = selectedTarget.transform.position + Vector3.up * 1.2f;

        GameObject projectile = Instantiate(projectilePrefab, start, Quaternion.identity);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * projectileSpeed;
            projectile.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        Destroy(projectile);
    }
}
