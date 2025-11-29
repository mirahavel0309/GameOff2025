using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class HeroInstance : CardInstance
{
    public ElementType mainElement;
    public int spellPower = 100;
    
    public bool isDefeated = false;
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
    protected override IEnumerator HandleDestruction()
    {
        base.HandleDestruction();
        GameManager.Instance.SetPlayerInput(false);

        yield return new WaitForSeconds(0.5f);

        //if (troopsField != null)
        //{
        //    troopsField.RemoveCard(this);
        //}
        PlayerHand.instance.RemoveCardsOfType(mainElement);
        GameManager.Instance.RemoveElementFromDeck(mainElement);

        //Destroy(gameObject);
        isDefeated = true;
        animator.Play("DefeatFall");
        GameManager.Instance.SetPlayerInput(true);
    }

    public void Revive()
    {
        animator.Play("Idle");
        isDefeated = false;
        currentHealth = maxHealth / 2;
        GameManager.Instance.AddElementToDeck(mainElement);
    }
}
