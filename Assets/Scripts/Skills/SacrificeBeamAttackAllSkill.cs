using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SacrificeBeamAttackAllSkill : BaseSkill
{
    [Header("Visual Settings")]
    public ElementIconLibrary elementsLib;
    public GameObject originEffectPrefab;
    public GameObject beamPrefab;

    public float elementalRiseHeight = 2f;
    public float elementalLaunchDuration = 0.4f;
    public float mergeDelay = 0.3f;

    [Header("Damage Settings")]
    [Tooltip("damageLevels[n] = damage dealt when n minions were sacrificed")]
    public int[] damageLevels;

    [Tooltip("Optional elemental type for damage")]
    public ElementType damageType = ElementType.Spirit;

    private List<CardInstance> enemyUnits;
    private List<CardInstance> playerUnits;

    public override IEnumerator Execute()
    {
        yield return GameManager.Instance.StartCoroutine(ExecuteRoutine());
    }
    public override string UpdatedDescription()
    {
        playerUnits = GameManager.Instance.playerField.GetCards().ToList();

        List<CardInstance> minions = new List<CardInstance>();

        foreach (var unit in playerUnits)
        {
            if (unit != null && !(unit is HeroInstance)) // minion
                minions.Add(unit);
        }

        int sacrificeCount = minions.Count;

        HeroInstance hero = GameManager.Instance.GetHeroOfelement(damageType);
        return description.Replace("<damage>", Mathf.RoundToInt(damageLevels[sacrificeCount] * (hero.spellPower / 100f)).ToString());
    }

    private IEnumerator ExecuteRoutine()
    {
        GameManager.Instance.SetPlayerInput(false);
        InfoPanel.instance.ShowMessage("Performing Sacrifice Beam...");

        yield return GameManager.Instance.StartCoroutine(
            PerformElementalLaunches(elementsLib, requiredElements,
                elementalRiseHeight, elementalLaunchDuration, mergeDelay)
        );

        Vector3 mergePoint = ComputeMergePoint();

        GameObject originFx = null;
        if (originEffectPrefab != null)
        {
            originFx = Instantiate(originEffectPrefab, mergePoint, Quaternion.identity);
        }

        playerUnits = GameManager.Instance.playerField.GetCards().ToList();

        List<CardInstance> minions = new List<CardInstance>();

        foreach (var unit in playerUnits)
        {
            if (unit != null && !(unit is HeroInstance)) // minion
                minions.Add(unit);
        }

        int sacrificeCount = minions.Count;

        // Pull animation
        float pullDuration = 0.35f;

        foreach (var minion in minions)
        {
            if (minion == null)
                continue;

            Vector3 startPos = minion.transform.position;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / pullDuration;
                if (minion != null)
                    minion.transform.position = Vector3.Lerp(startPos, mergePoint, t);

                yield return null;
            }

            if (minion != null)
                yield return minion.StartCoroutine(minion.Despawn());
        }

        int dmg = 0;

        if (damageLevels != null && damageLevels.Length > 0)
        {
            int index = Mathf.Clamp(sacrificeCount, 0, damageLevels.Length - 1);
            dmg = damageLevels[index];
        }

        enemyUnits = GameManager.Instance.enemyField.GetCards().ToList();

        foreach (var enemy in enemyUnits)
        {
            if (enemy == null) continue;

            GameObject beam = Instantiate(beamPrefab);
            var beamController = beam.GetComponent<BeamController>();

            Vector3 startPos = mergePoint;
            Vector3 endPos = enemy.transform.position + Vector3.up * 1.2f;

            beamController.PositionBeam(startPos, endPos);

            // beams persist briefly
            Destroy(beam, 1.1f);
        }

        HeroInstance hero = GameManager.Instance.GetHeroOfelement(damageType);
        foreach (var enemy in enemyUnits)
        {
            if (enemy != null)
                enemy.TakeDamage(Mathf.RoundToInt(dmg * (hero.spellPower / 100f)), damageType);
        }

        if (originFx != null)
            Destroy(originFx, 0.6f);


        foreach (var enemy in enemyUnits)
            yield return StartCoroutine(enemy.ResolveDeathIfNeeded());
        InfoPanel.instance.Hide();
        GameManager.Instance.SetPlayerInput(true);
    }

    private Vector3 ComputeMergePoint()
    {
        var heroes = GameManager.Instance.PlayerHeroes;

        if (heroes == null || heroes.Count == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (var h in heroes)
        {
            if (h != null)
            {
                sum += h.transform.position;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        return sum / count;
    }
}
