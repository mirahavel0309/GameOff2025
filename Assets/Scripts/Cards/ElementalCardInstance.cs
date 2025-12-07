using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum ElementType { Fire, Water, Nature, Wind, Physical, Spirit }
public class ElementalCardInstance : MonoBehaviour, IPointerClickHandler
{
    public ElementType elementType;

    [Header("UI Components")]
    public Image elementIcon;
    public TextMeshProUGUI elementNameText;
    public Image selectionHighlight;
    public ElementIconLibrary iconLibrary;
    public AudioClip useSound;
    private bool isSelected = false;

    public void Initialize(ElementType type)
    {
        elementType = type;

        if (elementNameText != null)
            elementNameText.text = type.ToString();

        if (elementIcon != null && iconLibrary != null)
            elementIcon.sprite = iconLibrary.GetIcon(type);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!GameManager.Instance.PlayerInputEnabled)
            return;


        isSelected = !isSelected;

        if (selectionHighlight != null)
            selectionHighlight.enabled = isSelected;

        Vector3 pos = transform.localPosition;
        transform.localPosition = isSelected ? new Vector3(pos.x, 50, pos.z) : new Vector3(pos.x, 0, pos.z);
        
        if (isSelected)
        {
            EffectsManager.instance.CreateSoundEffect(useSound, Vector3.zero);
            GameManager.Instance.AddElementToCombo(elementType, this);
        }
        else
            GameManager.Instance.RemoveElementFromCombo(elementType, this);
    }
    public void Consume()
    {
        PlayerHand.instance.RemoveCard(this);
        Destroy(gameObject, 0.1f);
    }
}
