using GameOff2025.Assets.Scripts.UI.HeroSelection;
using UnityEngine;
using UnityEngine.UI;

public class UIHeroSelectedSlot : UIHeroSelectionSlot
{
    protected override void Awake()
    {
        base.Awake();
    }
    public override void ClickHeroSelection()
    {
        if (isSelected)
        {
            base.ClickHeroSelection();
        }
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void MarkAsSelected(HeroInfoModel hero)
    {
        isSelected = true;
        iconImage.color = hero.heroIcon.color;
        heroName = hero.heroName;
        descriptionText = hero.heroDescription;

    }
    public void UnmarkAsSelected()
    {
        isSelected = false;
        iconImage.color = Color.white;
        heroName = string.Empty;
        descriptionText = string.Empty;
    }
    public bool IsSelected()
    {
        return isSelected;
    }
    public string GetHeroName()
    {
        return heroName;
    }
}
