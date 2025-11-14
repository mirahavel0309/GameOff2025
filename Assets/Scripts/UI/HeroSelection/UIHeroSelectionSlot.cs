using GameOff2025.Assets.Scripts.UI.HeroSelection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHeroSelectionSlot : MonoBehaviour
{
    public delegate void HeroSelectedDelegate(HeroInfoModel info);
    public static event HeroSelectedDelegate OnHeroSelected;

    [SerializeField] protected string heroName;
    [SerializeField] protected Image iconImage;
    [SerializeField] protected string descriptionText;
    protected bool isSelected = false;
    private Button heroButton;
    protected virtual void Awake()
    {
        heroButton = GetComponent<Button>();
        heroButton.onClick.AddListener(ClickHeroSelection);
    }
    public virtual void ClickHeroSelection()
    {

        HeroInfoModel heroInfo = new HeroInfoModel(heroName, iconImage, descriptionText);
        if (isSelected) heroInfo.Recruit();
        else heroInfo.Dismiss();
        OnHeroSelected?.Invoke(heroInfo);

    }
    protected virtual void OnDestroy()
    {
        heroButton.onClick.RemoveListener(ClickHeroSelection);
    }
}
