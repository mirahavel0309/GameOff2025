using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackMonsterSkill : BaseMonsterSkill
{
    public int damage;
    public bool isProjectileAttack;

    public override IEnumerator Execute(CardInstance target)
    {
        cardInstance = this.GetComponent<CardInstance>();
        if (isProjectileAttack)
        {
            yield return StartCoroutine(PerformPhysicalAttack(target));
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
        target.TakeDamage(Mathf.RoundToInt(damage * cardInstance.attackPower * 0.01f), ElementType.Fire, accuracy);

        // Return (0.25s)
        yield return MoveToPosition(originalPosition, 0.25f);

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
}
