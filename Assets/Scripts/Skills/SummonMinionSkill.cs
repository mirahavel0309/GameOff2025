using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonMinionSkill : BaseSkill
{
    public float elementalRiseHeight = 4;
    public float elementalLaunchDuration = 1;
    public float mergeDelay = 0.5f;
    [Header("Minion Settings")]
    public GameObject minionPrefab;
    public float searchRadius = 2.0f;
    public float minSeparation = 1.0f;
    public int maxSearchPoints = 16;

    [Header("Effect Settings")]
    public float effectDelay = 0.2f;
    public ElementIconLibrary elementsLib;

    public override void Execute()
    {
        GameManager.Instance.StartCoroutine(SummonRoutine());
    }

    private IEnumerator SummonRoutine()
    {
        GameManager.Instance.SetPlayerInput(false);

        // Step 1: Launch elemental projectiles
        yield return GameManager.Instance.StartCoroutine(
            PerformElementalLaunches(
                elementsLib,
                requiredElements,
                elementalRiseHeight,
                elementalLaunchDuration,
                mergeDelay
            )
        );

        yield return new WaitForSeconds(effectDelay);

        Vector3 spawnPos = FindValidSummonPosition(mergePoint);

        // Step 3: Spawn the minion
        GameObject minionGO = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
        CardInstance minionCard = minionGO.GetComponent<CardInstance>();

        if (minionCard == null)
        {
            Debug.LogError("Summoned minion prefab does not contain CardInstance!");
            Destroy(minionGO);
            GameManager.Instance.SetPlayerInput(true);
            yield break;
        }

        GameManager.Instance.playerField.AddSummonedCard(minionCard);

        Debug.Log("Summoned minion at: " + spawnPos);

        GameManager.Instance.SetPlayerInput(true);
        GameManager.Instance.RegisterActionUse();
    }

    // --------------------------------------------------------------------
    // Finds nearest non-overlapping spot around caster for the minion.
    // --------------------------------------------------------------------
    private Vector3 FindValidSummonPosition(Vector3 mergepoint)
    {
        Vector3 casterPos = mergepoint;

        // Try multiple evenly spaced angles around a circle
        for (int i = 0; i < maxSearchPoints; i++)
        {
            float angle = (360f / maxSearchPoints) * i;
            Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 testPos = casterPos + offset * searchRadius;

            if (IsPositionFree(testPos))
                return testPos;
        }

        // If all else fails, return caster position offset slightly
        return casterPos + Vector3.right * searchRadius;
    }

    private bool IsPositionFree(Vector3 testPos)
    {
        float checkRadius = minSeparation * 0.5f;

        Collider[] cols = Physics.OverlapSphere(testPos, checkRadius, LayerMask.GetMask("Unit"));

        // If any unit overlaps this position, it's not free
        return cols.Length == 0;
    }
}
