using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSoulBeamSkill : BaseMonsterSkill
{
    [Header("Soul Beam Settings")]
    public GameObject startEffectPrefab;
    public GameObject beamPrefab;                 // Prefab containing BeamController
    public Transform beamOrigin;                  // Optional: override origin point
    public float suckInDuration = 0.4f;
    public float beamDelay = 0.2f;
    public int[] damageLevels;

    public override IEnumerator Execute(CardInstance ignoredTarget)
    {
        CardInstance cardOwner = GetComponent<CardInstance>();
        var gm = GameManager.Instance;
        var enemyField = gm.enemyField;
        var playerHeroes = gm.PlayerHeroes;

        Vector3 originPos = beamOrigin != null ? beamOrigin.position : transform.position;
        GameObject fx = null;

        if (startEffectPrefab != null)
        {
            fx = Instantiate(startEffectPrefab, originPos, Quaternion.identity);
        }
        animator.Play("Cast");
        yield return new WaitForSeconds(0.2f);

        List<CardInstance> enemies = enemyField.GetCards();
        List<CardInstance> absorbed = new List<CardInstance>();

        foreach (var enemy in new List<CardInstance>(enemies))
        {
            if (enemy == null) continue;
            if (enemy == cardOwner) continue; // Don't absorb yourself

            absorbed.Add(enemy);

            StartCoroutine(MoveToAndDestroy(enemy));
        }

        yield return new WaitForSeconds(suckInDuration);

        int count = absorbed.Count;
        int dmg = damageLevels != null && damageLevels.Length > 0 ?
                  damageLevels[Mathf.Clamp(count, 0, damageLevels.Length - 1)] :
                  0; // fallback
        dmg = Mathf.RoundToInt(dmg * cardOwner.attackPower * 0.01f);

        foreach (var hero in playerHeroes)
        {
            if (hero == null || hero.isDefeated) continue;

            GameObject beamGO = Instantiate(beamPrefab);
            BeamController beam = beamGO.GetComponent<BeamController>();

            Vector3 start = originPos;
            Vector3 end = hero.transform.position + Vector3.up * 1.2f;

            beam.PositionBeam(start, end);

            // Deal damage after beam shows
            yield return new WaitForSeconds(beamDelay);

            hero.TakeDamage(dmg, ElementType.Physical);
            EffectsManager.instance.CreateFloatingText(hero.transform.position + Vector3.up * 2f, "-" + dmg, Color.red, 1.5f, 1.3f);

            Destroy(beamGO, 0.25f);
        }

        foreach (var hero in playerHeroes)
        {
            yield return StartCoroutine(hero.ResolveDeathIfNeeded());
        }

        if (fx != null)
            Destroy(fx, 0.8f);

        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator MoveToAndDestroy(CardInstance enemy)
    {
        Vector3 target = transform.position;
        Vector3 start = enemy.transform.position;

        float t = 0;
        while (t < suckInDuration)
        {
            if (enemy == null) yield break;

            enemy.transform.position = Vector3.Lerp(start, target, t / suckInDuration);
            t += Time.deltaTime;
            yield return null;
        }

        // Remove enemy from field
        GameManager.Instance.enemyField.RemoveCard(enemy);

        Destroy(enemy.gameObject);
    }
}
