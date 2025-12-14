using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RapidAttackMonsterSkill : BaseMonsterSkill
{
    [System.Serializable]
    public class RapidAttackEntry
    {
        public GameObject projectilePrefab;
        public int damage;
        public ElementType element;
    }

    public List<RapidAttackEntry> attacks = new List<RapidAttackEntry>();
    public float projectileSpeed = 12f;
    public float timeBetweenShots = 0.25f;
    public int attacksCount = 5;
    public Transform attackOrigin;

    private CardInstance card;

    public override IEnumerator Execute(CardInstance target)
    {
        card = GetComponent<CardInstance>();
        if (card == null)
        {
            Debug.LogError("RapidAttackMonsterSkill: No CardInstance found!");
            yield break;
        }

        // monster plays cast animation
        animator.SetTrigger("Cast");

        yield return new WaitForSeconds(0.15f);

        // Prepare potential player targets — only alive heroes
        List<CardInstance> playerTargets = GameManager.Instance.playerField.GetCards();
        playerTargets.RemoveAll(x => x == null || (x as HeroInstance)?.isDefeated == true);

        CardInstance chosen = null;
        for (int i = 0; i < attacksCount; i++)
        {
            if (playerTargets.Count == 0) break;

            // pick a random target
            if (playerTargets.Count > 1)
            {
                if(chosen == null)
                    chosen = playerTargets[Random.Range(0, playerTargets.Count)];
                else
                {
                    CardInstance[] filteredArray = playerTargets.Where(x => x != chosen).ToArray();
                    chosen = filteredArray[Random.Range(0, filteredArray.Length)];
                }
            }
            else
                chosen = playerTargets[0];
            // pick a random attack
            RapidAttackEntry entry = attacks[Random.Range(0, attacks.Count)];
            yield return StartCoroutine(FireProjectile(entry, chosen));

            yield return new WaitForSeconds(timeBetweenShots);
        }
        animator.SetTrigger("Idle");
    }

    private IEnumerator FireProjectile(RapidAttackEntry entry, CardInstance target)
    {
        if (entry.projectilePrefab == null || target == null)
            yield break;

        // Spawn projectile
        GameObject proj = Instantiate(entry.projectilePrefab, attackOrigin.position, Quaternion.identity);

        Vector3 start = attackOrigin.position;
        Vector3 end = target.transform.position;
        float distance = Vector3.Distance(start, end);
        float travelTime = distance / projectileSpeed;
        float elapsed = 0f;

        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / travelTime;
            proj.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        Destroy(proj);

        // Apply damage
        int accuracy = 100;
        LowerAccuracyStatus accStatus = card.GetComponent<LowerAccuracyStatus>();
        if (accStatus != null) accuracy -= accStatus.accuracyPenalty;

        int finalDamage = Mathf.RoundToInt(entry.damage * card.attackPower * 0.01f);

        target.TakeDamage(finalDamage, entry.element, accuracy);
        yield return StartCoroutine(target.ResolveDeathIfNeeded());
    }
}
