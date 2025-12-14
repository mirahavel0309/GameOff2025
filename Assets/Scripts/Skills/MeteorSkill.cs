using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MeteorSkill : BaseSkill
{
    [Header("Meteor Prefabs & Effects")]
    public GameObject meteorProjectilePrefab;
    public GameObject meteorImpactVFX;

    [Header("Meteor Damage")]
    public int baseDamage = 20;
    public ElementType damageType;
    public float meteorHeight = 12f; 
    public float travelDuration = 0.6f; 

    [Header("Area Damage")]
    public float damageRadius = 0f;
    public int areaDamage = 0;

    [Header("Visual/Audio")]
    public float elementalRiseHeight = 2.0f;
    public float elementalLaunchDuration = 0.4f;
    public float mergeDelay = 0.3f;
    public float gatherDuration = 0.3f;
    public AudioClip soundLaunch;
    public AudioClip soundHit;
    public ElementIconLibrary elementsLib;
    public GameObject BeamPrefab;
    [Header("Status Effect")]
    public StatusEffect statusEffect;   // The effect to apply
    [Range(0, 100)]
    public int chanceToProc = 0;        // % chance to apply

    public override IEnumerator Execute()
    {
        yield return GameManager.Instance.StartCoroutine(WaitForTargetAndCastMeteor());
    }
    public override string UpdatedDescription()
    {
        HeroInstance hero = GameManager.Instance.GetHeroOfelement(damageType);
        return description.Replace("<damage>", Mathf.RoundToInt(baseDamage * (hero.spellPower / 100f)).ToString());
    }

    private IEnumerator WaitForTargetAndCastMeteor()
    {
        GameManager.Instance.SetPlayerInput(false);

        Debug.Log("Select enemy target...");
        GameManager.Instance.SelectedTarget = null;
        InfoPanel.instance.ShowMessage("Select enemy as target...");

        yield return new WaitUntil(() => GameManager.Instance.SelectedTarget != null);

        InfoPanel.instance.Hide();
        CardInstance target = GameManager.Instance.SelectedTarget;
        GameManager.Instance.SelectTarget(null);

        yield return StartCoroutine(PerformMeteorVisuals(target));

        HeroInstance hero = GameManager.Instance.GetHeroOfelement(damageType);
        int dmg = Mathf.RoundToInt(baseDamage * (hero.spellPower / 100f));

        // Primary hit
        target.TakeDamage(dmg, damageType);

        // AoE
        if (damageRadius > 0f && areaDamage > 0)
            yield return ApplyAreaDamage(target);

        if (soundHit)
            EffectsManager.instance.CreateSoundEffect(soundHit, target.transform.position);

        if (meteorImpactVFX)
            Instantiate(meteorImpactVFX, target.transform.position, Quaternion.identity);

        yield return StartCoroutine(target.ResolveDeathIfNeeded());
        GameManager.Instance.SetPlayerInput(true);
        GameManager.Instance.RegisterActionUse();
    }

    private IEnumerator PerformMeteorVisuals(CardInstance target)
    {
        yield return GameManager.Instance.StartCoroutine(
            PerformElementalLaunches(
                elementsLib,
                requiredElements,
                elementalRiseHeight,
                elementalLaunchDuration,
                mergeDelay,
                false
            )
        );
        yield return StartCoroutine(MoveElementalEffectsToTargetArea(target, gatherDuration));

        List<GameObject> beams = new();
        foreach (var item in elementalEffects)
        {
            BeamController beam = Instantiate(BeamPrefab).GetComponent<BeamController>();
            beams.Add(beam.gameObject);
        }
        for (int i = 0; i < elementalEffects.Count; i++)
        {
            BeamController beam = beams[i].GetComponent<BeamController>() ;
            if (i == elementalEffects.Count - 1)
                beam.PositionBeam(elementalEffects[i].transform.position, elementalEffects[0].transform.position);
            else
                beam.PositionBeam(elementalEffects[i].transform.position, elementalEffects[i + 1].transform.position);
        }
        yield return new WaitForSeconds(.75f);

        Vector3 spawnPos = mergePoint + Vector3.up * meteorHeight;
        GameObject meteor = Instantiate(meteorProjectilePrefab, spawnPos, Quaternion.identity);

        if (soundLaunch)
            EffectsManager.instance.CreateSoundEffect(soundLaunch, spawnPos);

        yield return MoveProjectile(meteor, target.transform.position, travelDuration);

        foreach (var item in beams)
        {
            Destroy(item);
        }
        foreach (var item in elementalEffects)
        {
            item.GetComponent<ElementalEffect>().EndEffect();
        }
        Destroy(meteor);
    }
    private IEnumerator MoveElementalEffectsToTargetArea(CardInstance target, float duration)
    {
        if (elementalEffects == null || elementalEffects.Count == 0)
            yield break;

        Vector3 targetCenter = target.transform.position;

        // Precompute random positions around target
        List<(GameObject obj, Vector3 startPos, Vector3 endPos)> moves =
            new List<(GameObject, Vector3, Vector3)>();

        int count = elementalEffects.Count;
        float angleStep = 360f / count;
        float angle = 0f;

        foreach (var eff in elementalEffects)
        {
            if (eff == null)
            {
                angle += angleStep;
                continue;
            }

            Vector3 start = eff.transform.position;

            // compute circle position (x,z plane)
            float rad = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * damageRadius;
            Vector3 end = targetCenter + offset;

            moves.Add((eff, start, end));

            angle += angleStep;
        }

        float t = 0f;
        while (t < duration)
        {
            float lerp = t / duration;

            foreach (var m in moves)
            {
                if (m.obj != null)
                    m.obj.transform.position = Vector3.Lerp(m.startPos, m.endPos, lerp);
            }

            t += Time.deltaTime;
            yield return null;
        }

        // Snap to final position
        foreach (var m in moves)
        {
            if (m.obj != null)
                m.obj.transform.position = m.endPos;
        }
    }


    private IEnumerator ApplyAreaDamage(CardInstance mainTarget)
    {
        List<CardInstance> enemies = GameManager.Instance.GetEnemies();

        foreach (var enemy in enemies)
        {
            if (enemy == null || enemy == mainTarget)
                continue;

            float dist = Vector3.Distance(enemy.transform.position, mainTarget.transform.position);
            if (dist <= damageRadius)
            {
                enemy.TakeDamage(areaDamage, damageType);
                yield return StartCoroutine(enemy.ResolveDeathIfNeeded());
            }
        }
    }
}
