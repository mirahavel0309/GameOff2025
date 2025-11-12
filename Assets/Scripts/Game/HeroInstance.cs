using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeroInstance : CardInstance
{
    public HeroCard heroData;
    public ElementType mainElement;
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
}

public enum HeroActionType
{
    Attack,
    Cast,
    Defend
}
