using UnityEngine;
using System.Collections;

public class BeamAttackSkill : BaseSkill
{
    [Header("Beam Prefab")]
    public GameObject beamPrefab;             // Prefab containing BeamController + LineRenderer

    [Header("Damage Settings")]
    public int baseDamage = 10;
    public ElementType damageType = ElementType.Fire;
    public LowerAccuracyStatus accuracyEffect;

    [Header("Visual Settings")]
    public float elementalRiseHeight = 2.0f;
    public float elementalLaunchDuration = 0.4f;
    public float mergeDelay = 0.3f;

    [Header("SFX")]
    public AudioClip soundCast;
    public AudioClip soundHit;
    public ElementIconLibrary elementsLib;

    public override IEnumerator Execute()
    {
        yield return GameManager.Instance.StartCoroutine(ExecuteRoutine());
    }
    public override string UpdatedDescription()
    {
        HeroInstance hero = GameManager.Instance.GetHeroOfelement(damageType);
        return description.Replace("<damage>", Mathf.RoundToInt(baseDamage * (hero.spellPower / 100f)).ToString());
    }

    private IEnumerator ExecuteRoutine()
    {
        GameManager.Instance.SetPlayerInput(false);
        InfoPanel.instance.ShowMessage("Select enemy as target...");

        // --- Select Target ---
        GameManager.Instance.SelectedTarget = null;
        yield return new WaitUntil(() => GameManager.Instance.SelectedTarget != null);

        InfoPanel.instance.Hide();

        CardInstance target = GameManager.Instance.SelectedTarget;
        GameManager.Instance.SelectTarget(null);

        // Validate
        if (target == null || !target.CompareTag("Enemy"))
        {
            Debug.LogWarning("BeamAttackSkill: Invalid target.");
            GameManager.Instance.SetPlayerInput(true);
            yield break;
        }

        // --- Elemental launch visuals ---
        yield return GameManager.Instance.StartCoroutine(
            PerformElementalLaunches(
                elementsLib,
                requiredElements,
                elementalRiseHeight,
                elementalLaunchDuration,
                mergeDelay
            )
        );

        // --- Beam visual ---
        yield return StartCoroutine(FireBeam(target));


        GameManager.Instance.SetPlayerInput(true);
    }

    private IEnumerator FireBeam(CardInstance target)
    {
        if (beamPrefab == null)
        {
            Debug.LogError("BeamAttackSkill: beamPrefab is not assigned!");
            yield break;
        }

        // Instantiate beam
        GameObject beam = Instantiate(beamPrefab);

        BeamController controller = beam.GetComponent<BeamController>();
        if (controller == null)
        {
            Debug.LogError("BeamAttackSkill: Beam prefab missing BeamController script!");
            Destroy(beam);
            yield break;
        }

        if (soundCast)
            EffectsManager.instance.CreateSoundEffect(soundCast, mergePoint);

        Vector3 start = mergePoint;
        Vector3 end = target.transform.position;

        controller.PositionBeam(start, end);

        // Keep beam visible briefly
        yield return new WaitForSeconds(.3f);

        // --- Damage ---
        HeroInstance hero = GameManager.Instance.GetHeroOfelement(damageType);
        int finalDamage = Mathf.RoundToInt(baseDamage * (hero.spellPower / 100f));

        target.TakeDamage(finalDamage, damageType);
        if (accuracyEffect)
            target.AddStatusEffect(accuracyEffect, 100);

        if (soundHit)
            EffectsManager.instance.CreateSoundEffect(soundHit, target.transform.position);

        yield return new WaitForSeconds(.9f);

        controller.EndEffect();

        Destroy(beam);
        yield return StartCoroutine(target.ResolveDeathIfNeeded());
    }
}
