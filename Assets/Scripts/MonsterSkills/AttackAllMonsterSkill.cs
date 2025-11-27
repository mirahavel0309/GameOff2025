using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackAllMonsterSkill : BaseMonsterSkill
{
    [Header("Base Settings")]
    public int damage;
    public ElementType element = ElementType.Physical;
    public GameObject selfEffectPrefab;
    public GameObject attackEffetPrefab;
    public int baseAccuracy = 85;
    private bool castEventTriggered = false;

    [Header("Status effect Settings")]
    public StatusEffect statusEffect;   // The effect to apply
    [Range(0, 100)]
    public int chanceToProc = 0;        // % chance to apply

    public override IEnumerator Execute(CardInstance target)
    {
        cardInstance = this.GetComponent<CardInstance>();
        yield return StartCoroutine(PerformAttack(target));
    }
    public IEnumerator PerformAttack(CardInstance target)
    {
        if (target == null) yield break;
        castEventTriggered = false;

        // Play casting animation
        animator.SetTrigger("StartCast");

        float maxWait = 1.35f;
        float timer = 0f;
        GameObject selfEffect = Instantiate(selfEffectPrefab, transform.position, Quaternion.identity);
        Destroy(selfEffect, 1);

        while (!castEventTriggered && timer < maxWait)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        foreach (var hero in GameManager.Instance.PlayerHeroes)
        {
            int accuracy = baseAccuracy;
            LowerAccuracyStatus accStatus = GetComponent<LowerAccuracyStatus>();
            if (accStatus != null)
                accuracy -= accStatus.accuracyPenalty;
            // Deal damage
            hero.TakeDamage(Mathf.RoundToInt(damage * cardInstance.attackPower * 0.01f), element, accuracy);

            if (statusEffect != null)
            {
                int roll = Random.Range(0, 100);
                if (roll < chanceToProc)
                {
                    target.AddStatusEffect(statusEffect, target.attackPower);
                }
            }

            GameObject targetEffect = Instantiate(attackEffetPrefab, hero.transform.position, Quaternion.identity);
            Destroy(targetEffect, 1);
        }

        yield return new WaitForSeconds(0.2f);
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
