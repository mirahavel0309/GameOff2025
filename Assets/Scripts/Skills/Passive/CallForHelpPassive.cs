using System.Collections;
using UnityEngine;

public class CallForHelpPassive : PassiveSkill
{
    [Header("Call For Help Settings")]
    public GameObject enemyPrefab;     // prefab to spawn
    public int spawnCount = 1;         // how many allies to summon

    private bool activated = false;
    public override void Initialize()
    {
        base.Initialize();
        StartCoroutine(CallHelp());
        // Safety check
    }
    public IEnumerator CallHelp()
    {
        if (enemyPrefab == null || spawnCount <= 0)
        {
            Debug.LogWarning("CallForHelpPassive: No prefab or invalid spawn count.");
        }

        // Summon enemies
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject obj = GameObject.Instantiate(enemyPrefab);
            CardInstance newCard = obj.GetComponent<CardInstance>();

            if (newCard == null)
            {
                Debug.LogError("CallForHelpPassive: Spawned prefab has no CardInstance component!");
                Destroy(obj);
                continue;
            }

            yield return GameManager.Instance.enemyField.AddCard(newCard);

        }
        Destroy(this);
    }

}
