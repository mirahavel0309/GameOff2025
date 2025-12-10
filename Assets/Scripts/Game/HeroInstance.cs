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

        PlayerHand.instance.RemoveCardsOfType(mainElement);
        GameManager.Instance.RemoveElementFromDeck(mainElement);

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
