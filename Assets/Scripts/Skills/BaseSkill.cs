using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class BaseSkill : MonoBehaviour
{
    public string skillName;
    [TextArea(4,4)]
    public string description;
    public Sprite skillIcon;
    public List<ElementType> requiredElements = new List<ElementType>();

    public abstract void Execute();

    public bool Matches(List<ElementType> elements)
    {
        if (requiredElements.Count != elements.Count)
            return false;

        foreach (var elem in requiredElements)
            if (!elements.Contains(elem))
                return false;

        return true;
    }
    public IEnumerator PerformElementalLaunches(ElementIconLibrary elementsLib, List<ElementType> requiredElements, float riseHeight, float launchDuration, float mergeDelay)
    {
        List<HeroInstance> contributingHeroes = new List<HeroInstance>();
        foreach (var hero in GameManager.Instance.PlayerHeroes)
        {
            if (requiredElements.Contains(hero.mainElement))
                contributingHeroes.Add(hero);
        }

        if (contributingHeroes.Count == 0)
        {
            Debug.LogWarning("No heroes found for " + skillName);
            yield break;
        }

        List<GameObject> elementalProjectiles = new List<GameObject>();

        foreach (var hero in contributingHeroes)
        {
            GameObject projectilePrefab = elementsLib.GetElementProjectilePrefab(hero.mainElement);

            if (projectilePrefab == null)
            {
                Debug.LogWarning($"No projectile prefab found for element {hero.mainElement}");
                continue;
            }

            GameObject proj = GameObject.Instantiate(projectilePrefab, hero.transform.position, Quaternion.identity);
            elementalProjectiles.Add(proj);

            Vector3 riseTarget = hero.transform.position + Vector3.up * riseHeight;
            GameManager.Instance.StartCoroutine(MoveProjectile(proj, riseTarget, launchDuration));
        }

        yield return new WaitForSeconds(launchDuration + mergeDelay);

        // cleanup visual projectiles
        foreach (var p in elementalProjectiles)
            GameObject.Destroy(p);
    }

    protected IEnumerator MoveProjectile(GameObject projectile, Vector3 targetPos, float duration)
    {
        Vector3 startPos = projectile.transform.position;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            projectile.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }
        projectile.transform.position = targetPos;
    }
}
