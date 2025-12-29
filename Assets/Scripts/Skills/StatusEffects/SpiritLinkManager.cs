using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritLinkManager : MonoBehaviour
{
    public static SpiritLinkManager Instance;

    private List<SpiritLinkEffect> activeLinks = new();
    private Dictionary<CardInstance, int> pendingDamage = new();
    public bool resolving = false;

    private void Awake()
    {
        Instance = this;
    }

    public void Register(SpiritLinkEffect effect)
    {
        if (!activeLinks.Contains(effect))
            activeLinks.Add(effect);
    }

    public void Unregister(SpiritLinkEffect effect)
    {
        activeLinks.Remove(effect);
    }

    public bool IsLinked(CardInstance unit)
    {
        foreach (var link in activeLinks)
            if (link.owner == unit)
                return true;
        return false;
    }
    public bool IsQueued(CardInstance unit)
    {
        return pendingDamage.ContainsKey(unit);
    }

    public void QueueDamage(CardInstance source, int dmg)
    {
        if (!pendingDamage.ContainsKey(source))
            pendingDamage[source] = 0;

        pendingDamage[source] += dmg;
    }

    public IEnumerator ResolveAll()
    {
        if (resolving) yield break;
        resolving = true;

        // Collect total damage
        int totalDamage = 0;
        foreach (var kvp in pendingDamage)
            totalDamage += kvp.Value;

        if (activeLinks.Count == 0)
        {
            resolving = false;
            yield break;
        }

        // Split damage between linked units
        int count = activeLinks.Count;
        int baseDmg = totalDamage / count;
        int remainder = totalDamage % count;

        // Apply damage ONCE to each linked unit
        for (int i = 0; i < activeLinks.Count; i++)
        {
            CardInstance unit = activeLinks[i].owner;

            int dmg = baseDmg;
            if (i < remainder)
                dmg += 1;

            if (unit != null && dmg > 0)
                unit.TakeDamage(dmg, ElementType.Spirit, 100);
        }

        foreach (var target in activeLinks)
            yield return StartCoroutine(target.owner.ResolveDeathIfNeeded());

        pendingDamage.Clear();
        resolving = false;
    }
}
