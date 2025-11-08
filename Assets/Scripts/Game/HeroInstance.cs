using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeroInstance : CardInstance
{
    public HeroCard heroData;
    public ElementType mainElement;

    private List<EnchantmentCard> activeEnchantments = new();
    public override void Initialize()
    {
        base.Initialize();
        UpdateVisuals();
    }
    public override void UpdateVisuals()
    {
        base.UpdateVisuals();
    }
    protected override void HandleLeftClick()
    {
        GameManager.Instance.SelectTarget(this);
        //if(GameManager.Instance.GetSelectedHero() == null)
        //{
        //    if (!GameManager.Instance.PlayerInputEnabled || HasActedThisTurn)
        //        return;
        //    // If menu already exists, ignore
        //    if (FindObjectOfType<HeroActionMenu>() != null)
        //        return;
        //    // Open action selection
        //    var menuPrefab = GameManager.Instance.heroActionMenuPrefab;
        //    var menuInstance = Instantiate(menuPrefab, transform.position, Quaternion.identity);
        //    menuInstance.Initialize(this);
        //}
        //else
        //{
        //    if (state == CardState.OnField)
        //    {
        //        // Player card clicked
        //        if (troopsField.CompareTag("PlayerField"))
        //        {
        //            GameManager.Instance.GetSelectedHero().CastOnAlly(this);
        //        }
        //        else // Enemy card clicked
        //        {
        //            GameManager.Instance.GetSelectedHero().CastOnEnemy(this);
        //        }
        //    }
        //}
    }
    public override void SelectAsAttacker()
    {
        base.SelectAsAttacker();
        GameManager.Instance.SelectHero(this);
        Debug.Log("Hero Selected");
    }

    public void ApplyEnchantment(EnchantmentCard enchantment)
    {
        activeEnchantments.Add(enchantment);
        Debug.Log($"{heroData.cardName} received enchantment: {enchantment.cardName}");
        ApplyEffect(enchantment);
    }

    private void ApplyEffect(EnchantmentCard enchantment)
    {
        UpdateVisuals();
    }

    public void PerformAction(HeroActionType actionType)
    {
        // To be implemented next
        Debug.Log($"{heroData.cardName} performs {actionType}");
        HasActedThisTurn = true;
    }
    public void CastOnEnemy(CardInstance target)
    {
        StartCoroutine(CastProjectile(target, false));
    }

    public void CastOnAlly(CardInstance target)
    {
        StartCoroutine(CastProjectile(target, true));
    }

    private IEnumerator CastProjectile(CardInstance target, bool allyTarget)
    {
        HasActedThisTurn = true;
        GameManager.Instance.SetPlayerInput(false);

        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.transform.position = transform.position;
        projectile.transform.localScale = Vector3.one * 0.3f;
        float travelTime = 0.2f;
        float elapsed = 0;

        selectedAttacker = null;
        GameManager.Instance.SelectHero(null);
        base.HasActedThisTurn = true;

        Vector3 start = transform.position;
        Vector3 end = target.transform.position;

        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            projectile.transform.position = Vector3.Lerp(start, end, elapsed / travelTime);
            yield return null;
        }

        Destroy(projectile);

        if (allyTarget)
        {
            target.currentAttack += 3;
            target.UpdateVisuals();
            //Debug.Log($"{cardData.cardName} casted on ally {target.cardData.cardName}, increasing attack by 3!");
        }
        else
        {
            target.TakeDamage(currentAttack);
            //Debug.Log($"{cardData.cardName} casted spell on enemy {target.cardData.cardName}, dealing {currentAttack} damage!");
        }

        yield return new WaitForSeconds(0.2f);
        GameManager.Instance.SetPlayerInput(true);
    }
    public void DefendAlly(CardInstance target)
    {
        StartCoroutine(DefendRoutine(target));
    }

    private IEnumerator DefendRoutine(CardInstance target)
    {
        HasActedThisTurn = true;
        GameManager.Instance.SetPlayerInput(false);

        yield return new WaitForSeconds(0.3f);

        target.currentHealth += 3;
        target.UpdateVisuals();

        //Debug.Log($"{cardData.cardName} defended {target.cardData.cardName}, restoring 3 health!");

        GameManager.Instance.SetPlayerInput(true);
    }
}

public enum HeroActionType
{
    Attack,
    Cast,
    Defend
}
