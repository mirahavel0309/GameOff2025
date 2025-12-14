using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SacrificeAttackSkill : BaseSkill
{
    [Header("Sacrifice Attack Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
    public ElementType damageType;

    [Tooltip("Base and Bonus damage gained when a minion is sacrificed.")]
    public int baseDamage = 10;
    public int sacrificeBonusDamage = 10;

    [Header("Visual Settings")]
    public ElementIconLibrary elementsLib;
    public float elementalRiseHeight = 2f;
    public float elementalLaunchDuration = 0.4f;
    public float mergeDelay = 0.3f;

    private CardInstance selectedSacrifice = null;

    public override IEnumerator Execute()
    {
        yield return GameManager.Instance.StartCoroutine(ExecuteRoutine());
    }
    public override string UpdatedDescription()
    {
        HeroInstance hero = GameManager.Instance.GetHeroOfelement(damageType);
        return description.Replace("<damage>", Mathf.RoundToInt(baseDamage * (hero.spellPower / 100f)).ToString()).Replace("<full_damage>", Mathf.RoundToInt(sacrificeBonusDamage * (hero.spellPower / 100f)).ToString());
    }
    public void HighLightUnits(List<CardInstance> enemies, List<CardInstance> minions)
    {
        foreach (var e in enemies)
        {
            if (e != null)
                e.ShowSelector(SelectionState.Red);
        }

        // Show yellow selectors on minions
        foreach (var m in minions)
        {
            if (m != null)
                m.ShowSelector(SelectionState.Yellow);
        }

    }
    public void HideHighlight(List<CardInstance> enemies, List<CardInstance> friendlyUnits)
    {
        foreach (var e in enemies)
        {
            if (e != null)
                e.HideSelector();
        }

        foreach (var m in friendlyUnits)
        {
            if (m != null)
                m.HideSelector();
        }
    }

    private IEnumerator ExecuteRoutine()
    {
        GameManager.Instance.SetPlayerInput(false);

        List<CardInstance> enemies = GameManager.Instance.enemyField.GetCards();
        List<CardInstance> friendlyUnits = GameManager.Instance.playerField.GetCards(); 
        
        List<CardInstance> minions = new List<CardInstance>();
        foreach (var unit in friendlyUnits)
        {
            // Only non-heroes can be sacrificed
            if (!(unit is HeroInstance))
                minions.Add(unit);
        }

        HighLightUnits(enemies, minions);

        InfoPanel.instance.ShowMessage("Select minion to sacrifice and enemy target.");

        CardInstance target = null;
        selectedSacrifice = null;
        while (target == null)
        {
            yield return new WaitUntil(() => GameManager.Instance.SelectedTarget != null);

            CardInstance clicked = GameManager.Instance.SelectedTarget;
            GameManager.Instance.SelectTarget(null);  // reset right away

            if (clicked == null)
                continue;

            if (enemies.Contains(clicked))
            {
                target = clicked;
                Debug.Log("Enemy target selected: " + target.name);
                break;
            }

            if (minions.Contains(clicked) && !(clicked is HeroInstance))
            {
                selectedSacrifice = clicked;
                Debug.Log("Selected sacrifice: " + selectedSacrifice.name);
                InfoPanel.instance.ShowMessage("Sacrifice chosen. Now select enemy target.");
                foreach (var m in minions)
                    m.HideSelector();
                selectedSacrifice.ShowSelector(SelectionState.Green);
                continue; // remain in the loop, waiting for enemy
            }

            Debug.Log("Invalid target clicked.");
        }

        InfoPanel.instance.Hide();

        HideHighlight(enemies, minions);

        yield return GameManager.Instance.StartCoroutine(
            PerformElementalLaunches(
                elementsLib,
                requiredElements,
                elementalRiseHeight,
                elementalLaunchDuration,
                mergeDelay
            )
        );

        int dmg = baseDamage;

        if (selectedSacrifice != null)
        {
            yield return selectedSacrifice.Despawn();   // remove minion properly
            dmg = sacrificeBonusDamage;
        }

        // Fire projectile
        GameObject proj = Instantiate(projectilePrefab, mergePoint, Quaternion.identity);

        Vector3 startPos = mergePoint + Vector3.up * 1.2f;
        Vector3 endPos = target.transform.position + Vector3.up * 1.2f;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * projectileSpeed;
            proj.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        Destroy(proj);


        target.TakeDamage(dmg, damageType);

        yield return StartCoroutine(target.ResolveDeathIfNeeded());
        GameManager.Instance.SetPlayerInput(true);
        GameManager.Instance.RegisterActionUse();
    }
}
