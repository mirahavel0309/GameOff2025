using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackMonsterSkill : BaseMonsterSkill
{
    [Header("Base Settings")]
    public int damage;
    public ElementType element = ElementType.Physical;
    public bool isProjectileAttack;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;     // Assign in Inspector
    public float projectileSpeed = 12f;
    private bool castEventTriggered = false;
    public Transform projectileSpawnPoint;

    [Header("Status effect Settings")]
    public StatusEffect statusEffect;   // The effect to apply
    [Range(0, 100)]
    public int chanceToProc = 0;        // % chance to apply

    public override IEnumerator Execute(CardInstance target)
    {
        cardInstance = this.GetComponent<CardInstance>();
        if (isProjectileAttack)
        {
            yield return StartCoroutine(PerformProjectileAttack(target));
        }
        else
        {
            yield return StartCoroutine(PerformPhysicalAttack(target));
        }
    }
    
    public IEnumerator PerformPhysicalAttack(CardInstance target)
    {

        if (target == null) yield break;

        animator.SetTrigger("Action");

        GameManager.Instance.SetPlayerInput(false);
        cardInstance.ToggleHighlight(false);
        GameManager.Instance.SelectHero(null);

        Vector3 originalPosition = transform.position;

        // Move toward target (0.25s)
        yield return MoveToPosition(target.transform.position, 0.25f);
        int accuracy = 100;
        LowerAccuracyStatus accStatus = GetComponent<LowerAccuracyStatus>();
        if (accStatus != null)
            accuracy -= accStatus.accuracyPenalty;
        // Deal damage
        target.TakeDamage(Mathf.RoundToInt(damage * cardInstance.attackPower * 0.01f), element, accuracy);
        if (statusEffect != null)
        {
            int roll = Random.Range(0, 100);
            if (roll < chanceToProc)
            {
                target.AddStatusEffect(statusEffect, target.attackPower);
            }
        }

        yield return MoveToPosition(originalPosition, 0.25f);

        GameManager.Instance.SetPlayerInput(true);
    }
    public IEnumerator PerformProjectileAttack(CardInstance target)
    {
        if (target == null) yield break;
        castEventTriggered = false;

        // Play casting animation
        animator.SetTrigger("StartCast");

        GameManager.Instance.SetPlayerInput(false);

        // Wait for animation event OR fallback timeout
        float maxWait = 1.35f;   // fallback
        float timer = 0f;

        while (!castEventTriggered && timer < maxWait)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Spawn projectile
        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

            Vector3 start = projectileSpawnPoint.position;
            Vector3 end = target.transform.position;

            float travelTime = Vector3.Distance(start, end) / projectileSpeed;
            float elapsed = 0f;

            while (elapsed < travelTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / travelTime;
                proj.transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            // Hit target
            Destroy(proj);

            int accuracy = 100;
            LowerAccuracyStatus accStatus = GetComponent<LowerAccuracyStatus>();
            if (accStatus != null)
                accuracy -= accStatus.accuracyPenalty;

            target.TakeDamage(Mathf.RoundToInt(damage * cardInstance.attackPower * 0.01f), element, accuracy);

            // Proc status effects
            if (statusEffect != null)
            {
                int roll = Random.Range(0, 100);
                if (roll < chanceToProc)
                {
                    target.AddStatusEffect(statusEffect, cardInstance.attackPower);
                }
            }
        }

        yield return new WaitForSeconds(0.2f);

        GameManager.Instance.SetPlayerInput(true);
    }

    private IEnumerator MoveToPosition(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
    }
    public void OnProjectileCastEvent()
    {
        castEventTriggered = true;
    }
}
