using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FormChangePassive : PassiveSkill
{
    [Header("Transformation Settings")]
    [Range(0f, 1f)]
    public float hpThreshold = 0.5f;   // transform when health < 50%
    public GameObject secondFormPrefab;
    public float shakeDuration = 0.4f;
    public float shakeIntensity = 0.1f;

    public float scaleUpDuration = 0.6f;
    public float finalScaleMultiplier = 1.3f;

    private bool transformed = false;
    private Animator animator;

    public override void Initialize()
    {
        base.Initialize();
        animator = owner.GetComponentInChildren<Animator>();
    }

    public override IEnumerator OnTurnStart()
    {
        if (transformed) yield break;
        if (owner == null) yield break;

        float hpPercent = (float)owner.CurrentHealth / owner.maxHealth;

        if (hpPercent <= hpThreshold)
        {
             yield return owner.StartCoroutine(TransformRoutine());
        }
    }

    private IEnumerator TransformRoutine()
    {
        transformed = true;

        if (animator != null)
            animator.SetTrigger("Transform");

        yield return Shake(owner.gameObject, shakeDuration, shakeIntensity);

        yield return ScaleUp(owner.transform, scaleUpDuration, finalScaleMultiplier);

        yield return ReplaceWithSecondForm();
    }

    private IEnumerator Shake(GameObject obj, float duration, float intensity)
    {
        Vector3 originalPos = obj.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float offsetX = Random.Range(-1f, 1f) * intensity;
            float offsetY = Random.Range(-1f, 1f) * intensity;

            obj.transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            yield return null;
        }

        obj.transform.localPosition = originalPos;
    }

    private IEnumerator ScaleUp(Transform target, float duration, float multiplier)
    {
        Vector3 initialScale = target.localScale;
        Vector3 finalScale = initialScale * multiplier;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            target.localScale = Vector3.Lerp(initialScale, finalScale, t);

            yield return null;
        }

        target.localScale = finalScale;
    }

    private IEnumerator ReplaceWithSecondForm()
    {
        if (secondFormPrefab == null)
        {
            Debug.LogError("Second form prefab is missing!");
            yield break;
        }

        // Store info from first form
        Vector3 oldPos = owner.transform.position;
        Quaternion oldRot = owner.transform.rotation;
        Vector3 oldScale = owner.transform.localScale;
        int oldCurrent = owner.CurrentHealth;
        int oldMax = owner.maxHealth;
        float hpFraction = (float)oldCurrent / oldMax;

        // Remove first form
        GameObject oldObj = owner.gameObject;

        // Spawn new form
        GameObject newObj = Instantiate(secondFormPrefab, oldPos, oldRot);
        CardInstance newCard = newObj.GetComponent<CardInstance>();
        newCard.speedCount = -50;

        // Apply proportional HP transfer
        newCard.SetHealth(Mathf.Max(1, Mathf.RoundToInt(newCard.maxHealth * hpFraction)));
        newCard.UpdateVisuals();

        // Inherit status effects (optional, but recommended)
        CopyStatusEffects(owner, newCard);

        // Maintain field slot location by re-registering in troops field
        TroopsField field = GameManager.Instance.enemyField;
        if (field != null)
        {
            field.RemoveCard(owner);
            yield return field.AddCard(newCard, false);
        }

        // Destroy original
        Destroy(oldObj);
    }

    private void CopyStatusEffects(CardInstance from, CardInstance to)
    {
        //foreach (var effect in from.statusEffects)
        //{
        //    // Create new instance of status effect
        //    StatusEffect newEffect = Instantiate(effect);
        //    to.AddStatusEffect(newEffect, 0); // power argument unused for passive transfer
        //}
    }
}
