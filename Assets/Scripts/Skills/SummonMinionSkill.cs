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

    public override IEnumerator Execute()
    {
        yield return GameManager.Instance.StartCoroutine(SummonRoutine());
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


        Dictionary<ElementType, HeroInstance> contributingHeroes = new Dictionary<ElementType, HeroInstance>();
        foreach (var hero in GameManager.Instance.PlayerHeroes)
        {
            if (requiredElements.Contains(hero.mainElement))
                contributingHeroes.Add(hero.mainElement, hero);
        }

        Vector3 spawnPoint = Vector3.zero;
        if (contributingHeroes.Count > 0)
        {
            spawnPoint = Vector3.zero;
            foreach (var p in contributingHeroes.Values)
                spawnPoint += p.transform.position;
            spawnPoint /= contributingHeroes.Count;
        }

        Vector3 spawnPos = FindValidSummonPosition(spawnPoint);

        // Step 3: Spawn the minion
        GameObject minionGO = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
        CardInstance minionCard = minionGO.GetComponent<CardInstance>();
        minionCard.speedCount = 0;

        if (minionCard == null)
        {
            Debug.LogError("Summoned minion prefab does not contain CardInstance!");
            Destroy(minionGO);
            GameManager.Instance.SetPlayerInput(true);
            yield break;
        }

        GameManager.Instance.playerField.AddSummonedCard(minionCard);

        //Debug.Log("Summoned minion at: " + spawnPos);

        GameManager.Instance.SetPlayerInput(true);
        GameManager.Instance.RegisterActionUse();
    }
        
    private Vector3 FindValidSummonPosition(Vector3 mergepoint)
    {
        Vector3 casterPos = mergepoint;

        // Try multiple evenly spaced angles around a circle
        for (int j = 1; j <= 3; j++)
        {
            for (int i = 0; i < maxSearchPoints; i++)
            {
                float angle = (360f / maxSearchPoints) * i;
                Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector3 testPos = casterPos + offset * searchRadius * j;

                if (IsPositionFree(testPos))
                    return testPos;
            }
        }

        // If all else fails, return caster position offset slightly
        return casterPos + Vector3.right * searchRadius;
    }

    private bool IsPositionFree(Vector3 testPos)
    {
        float checkRadius = minSeparation * 0.5f;

        Collider[] cols = Physics.OverlapSphere(testPos, checkRadius, LayerMask.GetMask("Character"));

        // If any unit overlaps this position, it's not free
        return cols.Length == 0;
    }
}
