using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillCardUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI castText;
    public Button useButton;

    private BaseSkill skill;
    private int skillCost = 1;

    public void Initialize(BaseSkill skill, int actionCost)
    {
        this.skill = skill;
        skillCost = actionCost;

        if (nameText != null)
            nameText.text = skill.skillName;

        if (descriptionText != null)
            descriptionText.text = skill.UpdatedDescription();

        if (iconImage != null && skill.skillIcon != null)
            iconImage.sprite = skill.skillIcon;

        if (castText != null)
        {
            if (actionCost <= GameManager.Instance.actionsThisTurn)
                castText.text = $"Cast <color=#009999>{actionCost} AP</color>";
            else
                castText.text = $"Cast <color=#990000>{actionCost} AP</color>";
        }

        if (useButton != null)
        {
            useButton.onClick.AddListener(OnUsePressed);
            useButton.enabled = actionCost <= GameManager.Instance.actionsThisTurn;
        }
    }

    private void OnUsePressed()
    {
        // Hide all skill cards and execute the chosen one
        GameManager.Instance.OnSkillCardChosen(skill, skillCost);
    }
}
