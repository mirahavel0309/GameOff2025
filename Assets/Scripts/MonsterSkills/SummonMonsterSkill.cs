using System.Collections;
using UnityEngine;

public class SummonMonsterSkill : BaseMonsterSkill
{
    [Header("Summoning Settings")]
    public GameObject monsterPrefab;
    public float summonAnimationDelay = 0.4f;
    public int count = 1;

    public override IEnumerator Execute(CardInstance targetIgnored)
    {
        if (monsterPrefab == null)
        {
            Debug.LogError("SummonMonsterSkill: Monster prefab NOT assigned!");
            yield break;
        }

        // Check if there is room on enemy field
        var field = GameManager.Instance.enemyField;

        if (field.GetCards().Count > 4)
        {
            Debug.Log("Summon failed: No space!");
            EffectsManager.instance.CreateFloatingText(
                transform.position + Vector3.up * 2f,
                "No space!",
                Color.yellow,
                1.2f,
                1.8f
            );

            yield break;
        }

        if (animator != null)
        {
            animator.Play("Summon");
        }

        // Wait for animation windup 
        yield return new WaitForSeconds(summonAnimationDelay);
        for (int i = 0; i < count; i++)
        {
            GameObject newMonsterGO = Instantiate(monsterPrefab);
            CardInstance summonedCard = newMonsterGO.GetComponent<CardInstance>();
            summonedCard.speedCount = 100;
            summonedCard.ScalePower(WaveSpawner.instance.healthScale, WaveSpawner.instance.damageScale);
            if (summonedCard == null)
            {
                Debug.LogError("Summoned prefab does not contain CardInstance!");
                Destroy(newMonsterGO);
                yield break;
            }

            yield return field.AddCard(summonedCard);
        }

        //if (soundOnCast != null)
        //    EffectsManager.Instance.CreateSoundEffect(soundOnCast, cardOwner.transform.position);


        yield return new WaitForSeconds(0.3f);
    }
}
