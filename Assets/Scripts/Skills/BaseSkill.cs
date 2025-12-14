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
    protected Vector3 mergePoint; // some skill need this point
    protected List<GameObject> elementalEffects; // for use in some skills
    public AudioClip soundCharge;

    public abstract IEnumerator Execute();

    public IEnumerator PostExecute()
    {
        yield return SpiritLinkManager.Instance.ResolveAll();
    }

    public bool Matches(List<ElementType> elements)
    {
        if (requiredElements.Count != elements.Count)
            return false;

        foreach (var elem in requiredElements)
            if (!elements.Contains(elem))
                return false;

        return true;
    }
    public IEnumerator PerformElementalLaunches(ElementIconLibrary elementsLib, List<ElementType> requiredElements, float riseHeight, float launchDuration, float mergeDelay, bool effectCleanup = true)
    {
        Dictionary<ElementType, HeroInstance> contributingHeroes = new Dictionary<ElementType, HeroInstance>();
        foreach (var hero in GameManager.Instance.PlayerHeroes)
        {
            if (requiredElements.Contains(hero.mainElement))
                contributingHeroes.Add(hero.mainElement, hero);
        }

        if (contributingHeroes.Count == 0)
        {
            Debug.LogWarning("No heroes found for " + skillName);
            yield break;
        }

        List<GameObject> elementalProjectiles = new List<GameObject>();

        foreach (var element in requiredElements)
        {
            HeroInstance hero = contributingHeroes[element];

            hero.spellPower += 1;
            GameObject projectilePrefab = elementsLib.GetElementProjectilePrefab(hero.mainElement);

            if (projectilePrefab == null)
            {
                Debug.LogWarning($"No projectile prefab found for element {hero.mainElement}");
                continue;
            }

            GameObject proj = GameObject.Instantiate(projectilePrefab, hero.transform.position, Quaternion.identity);
            elementalProjectiles.Add(proj);
            if (soundCharge)
                EffectsManager.instance.CreateSoundEffect(soundCharge, transform.position);
            //EffectsManager.instance.CreateSoundEffect(elementsLib.GetElementSound(hero.mainElement), transform.position);

            Vector3 riseTarget = hero.transform.position + Vector3.up * riseHeight;
            GameManager.Instance.StartCoroutine(MoveProjectile(proj, riseTarget, launchDuration));
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(launchDuration + mergeDelay);


        mergePoint = Vector3.zero;
        foreach (var p in elementalProjectiles)
            mergePoint += p.transform.position;
        mergePoint /= elementalProjectiles.Count;

        // cleanup visual projectiles
        if (effectCleanup)
        {
            foreach (var p in elementalProjectiles)
                GameObject.Destroy(p);
        }
        else
        {
            elementalEffects = elementalProjectiles;
        }
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
    public virtual string UpdatedDescription()
    {
        return description;
    }
}
