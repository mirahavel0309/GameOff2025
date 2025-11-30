using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHoverUI : MonoBehaviour
{
    [Header("References")]
    public RectTransform root;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI strongText;
    public TextMeshProUGUI weakText;
    public TextMeshProUGUI spellPowerText;
    public Image characterImage;

    [Header("Status Effects")]
    public Transform statusContainer;
    public TextMeshProUGUI statusTextPrefab;

    private void Reset()
    {
        if (root == null)
            root = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (root == null)
            root = GetComponent<RectTransform>();

        if (root != null)
            root.gameObject.SetActive(false);
    }

    public void Show(CardInstance card)
    {
        if (card == null) return;
        if (root == null) return;

        root.gameObject.SetActive(true);

        // Basic info
        nameText.text = card.visibleName;
        hpText.text = $"{card.CurrentHealth} / {card.MaxHealth}";
        characterImage.sprite = card.GetCardVisual();
        // Resistances / weaknesses
        var res = card.Resistances;
        if (res != null)
        {
            string strong = string.Empty;
            string weak = string.Empty;
            if (res.strong != null && res.strong.Count > 0)
            {
                foreach (var element in res.strong)
                {
                    strong += element.ToString() + ", ";
                }
            }
            if (res.wakness != null && res.wakness.Count > 0)
            {
                foreach (var element in res.wakness)
                {
                    weak += element.ToString() + ", ";
                }
            }
            strongText.text = $"{strong.TrimEnd(' ', ',')}";
            weakText.text = $"{weak.TrimEnd(' ', ',')}";
        }
        else
        {
            strongText.text = "None";
            weakText.text = "None";
        }

        // Spell power for heroes
        //var hero = card as HeroInstance;
        //if (hero != null)
        //{
        //    spellPowerText.gameObject.SetActive(true);
        //    spellPowerText.text = $"Spell: {hero.spellPower}";
        //}
        //else
        //{
        //    if (spellPowerText != null)
        //        spellPowerText.gameObject.SetActive(false);
        //}

        // Status effects list
        if (statusContainer != null && statusTextPrefab != null)
        {
            foreach (Transform t in statusContainer)
                Destroy(t.gameObject);

            if (card.activeEffects != null && card.activeEffects.Count > 0)
            {
                foreach (var eff in card.activeEffects.Where(e => e != null))
                {
                    var go = Instantiate(statusTextPrefab.gameObject, statusContainer);
                    var tmp = go.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {

                        string display = eff.GetType().Name;
                        tmp.text = display;
                    }
                }
            }
        }
    }

    public void Hide()
    {
        if (root != null)
            root.gameObject.SetActive(false);
    }

    public void SetScreenPosition(Vector2 screenPos)//Maybe can be useful later
    {
        if (root == null || root.parent == null) return;

        RectTransform parentRect = root.parent as RectTransform;
        if (parentRect == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, null, out Vector2 localPoint);


        localPoint += new Vector2(12f, -12f);

        Rect pr = parentRect.rect;
        Rect rr = root.rect;

        float halfW = rr.width * 0.5f;
        float halfH = rr.height * 0.5f;

        Vector2 clamped = localPoint;
        clamped.x = Mathf.Clamp(clamped.x, pr.xMin + halfW, pr.xMax - halfW);
        clamped.y = Mathf.Clamp(clamped.y, pr.yMin + halfH, pr.yMax - halfH);

        root.anchoredPosition = clamped;
    }
}
