using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum CardState { InHand, OnField, InDeck, Destroyed }
[RequireComponent(typeof(SpriteRenderer))]
public class CardInstance : MonoBehaviour
{

    public string visibleName;
    [Header("Card Data")]
    //public Card baseCard;
    public int attackPower = 100;
    protected int currentHealth = 0;
    public int maxHealth;
    public int speed = 100;
    public int speedCount = 500;

    [Header("References")]
    [SerializeField] public SpriteRenderer cardSprite;
    [SerializeField] private SpriteRenderer highlightSprite;
    public Animator animator;
    public TroopsField troopsField;
    public bool HasActedThisTurn { get; set; } = false;
    public List<StatusEffect> activeEffects = new List<StatusEffect>();


    private bool isSelected = false;
    private Vector3 originalPosition;
    private CharacterResistances resistances;
    // Public accessors for UI / external systems
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public CharacterResistances Resistances => resistances;
    public IReadOnlyList<StatusEffect> ActiveEffects => activeEffects.AsReadOnly();
    protected ProgressBar hpBar;
    private List<PassiveSkill> passiveSkills;
    protected static CardInstance selectedAttacker;
    private void Awake()
    {
        resistances = GetComponent<CharacterResistances>();
    }
    public void ScalePower(float hpScale, float attackScale)
    {
        maxHealth = Mathf.RoundToInt(hpScale * maxHealth);
        attackPower = Mathf.RoundToInt(attackPower * attackScale);
    }
    void Start()
    {
        if (currentHealth == 0)
            currentHealth = maxHealth;
        Initialize();
        hpBar = GetComponentInChildren<ProgressBar>();
        hpBar.SetValue(currentHealth, maxHealth);
        passiveSkills = GetComponents<PassiveSkill>().ToList();
    }
    public virtual void Initialize()
    {
        UpdateVisuals();

        if (highlightSprite != null)
            highlightSprite.enabled = false;
    }

    void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick();
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }
    protected virtual void HandleLeftClick()
    {
        GameManager.Instance.SelectTarget(this);

        // Player card clicked
        if (troopsField.CompareTag("PlayerField"))
        {
            if (HasActedThisTurn)
            {
                Debug.Log("This card already attacked this turn.");
                return;
            }

            SelectAsAttacker();
        }
        else // Enemy card clicked
        {

        }
    }
    protected virtual void HandleRightClick()
    {
        // If player right-clicks, cancel
        if (Input.GetMouseButtonDown(1))
        {
            ClearSelection();
            return;
        }
    }
    public virtual void UpdateVisuals()
    {
        if (hpBar)
            hpBar.SetValue(currentHealth, maxHealth);

        // I think I'll reomve baseCard. Right sprite will come from prefab of the object
        //if (cardSprite != null && baseCard != null && baseCard.artwork != null)
        //    cardSprite.sprite = baseCard.artwork;
    }

    internal void SetHealth(int value)
    {
        currentHealth = value;
        UpdateVisuals();
    }

    private void OnMouseEnter()
    {
        isSelected = true;
        ToggleSelection();
        // Show hover UI immediately via HoverManager
        if (HoverManager.Instance != null)
            HoverManager.Instance.ShowNow(this);
    }
    private void OnMouseExit()
    {
        isSelected = false;
        ToggleSelection();
        // Hide hover UI
        if (HoverManager.Instance != null)
            HoverManager.Instance.HideNow(this);
    }

    private void ToggleSelection()
    {
    }
    public virtual void SelectAsAttacker()
    {
        if (selectedAttacker != null)
            selectedAttacker.ToggleHighlight(false);

        selectedAttacker = this;
        ToggleHighlight(true);
    }

    public static void ClearSelection()
    {
        if (selectedAttacker != null)
        {
            selectedAttacker.ToggleHighlight(false);
            selectedAttacker = null;
        }
    }
    public void ToggleHighlight(bool active)
    {
        if (highlightSprite != null)
        {
            highlightSprite.color = active ? new Color(0f, 1f, 0f, 0.5f) : Color.clear;
            highlightSprite.enabled = active;
        }
    }

    public int TakeDamage(int dmg, ElementType element, int accuracy = 95)
    {
        if (accuracy < 100)
        {
            if (Random.Range(1, 100) > accuracy)
            {
                EffectsManager.instance.CreateFloatingText(transform.position, "Miss", Color.black);
                return 0;
            }
        }

        ElementalProtectionEffect protection = GetComponent<ElementalProtectionEffect>();
        if (protection != null)
        {
            if (protection.element == element)
            {
                Destroy(protection);
                EffectsManager.instance.CreateFloatingText(transform.position, "protected", Color.white);
                return 0;
            }
        }


        int finalDamage = dmg;
        if (resistances != null)
        {
            Color textColor = Color.black;
            float resistance = 0;
            switch (element)
            {
                case ElementType.Fire:
                    resistance = resistances.GetResistanceValue(ElementType.Fire);
                    textColor = Color.red;
                    break;
                case ElementType.Water:
                    resistance = resistances.GetResistanceValue(ElementType.Water);
                    textColor = Color.cyan;
                    break;
                case ElementType.Nature:
                    resistance = resistances.GetResistanceValue(ElementType.Nature);
                    textColor = Color.green;
                    break;
                case ElementType.Wind:
                    resistance = resistances.GetResistanceValue(ElementType.Wind);
                    textColor = Color.gray;
                    break;
                case ElementType.Physical:
                    resistance = resistances.GetResistanceValue(ElementType.Physical);
                    break;
                case ElementType.Spirit:
                    resistance = resistances.GetResistanceValue(ElementType.Spirit);
                    textColor = Color.blue;
                    break;
            }

            // Apply resistance (e.g., 50 = 50% reduction, -20 = +20% damage)
            finalDamage = Mathf.RoundToInt(dmg * (1f - (resistance / 100f)));


            foreach (var passive in GetComponents<PassiveSkill>())
            {
                passive.OnReceiveDamage(ref finalDamage, element, this);
            }

            EffectsManager.instance.CreateFloatingText(transform.position, finalDamage.ToString(), textColor);
        }


        int realDamageDone = Mathf.Min(finalDamage, currentHealth); // needed for life drain skill
        currentHealth -= finalDamage;

        if (hpBar)
            hpBar.SetValue(currentHealth, maxHealth);

        StartCoroutine(Shake(0.35f, 0.25f));
        UpdateVisuals();

        if (currentHealth <= 0)
        {
            StartCoroutine(HandleDestruction());
        }
        return realDamageDone;
    }
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateVisuals();
        if (amount > 0)
        {
            // Optional floating text feedback
            EffectsManager.instance.CreateFloatingText(
                transform.position + Vector3.up * 1.5f,
                "+" + amount,
                Color.green,
                1.2f,
                0.8f,
                1.2f
            );
        }
    }
    protected virtual IEnumerator HandleDestruction()
    {
        //GameManager.Instance.SetPlayerInput(false);
        Dissolve dissolveEffect = GetComponentInChildren<Dissolve>();
        if (dissolveEffect != null)
        {
            dissolveEffect.DissolveVanish();
        }
        yield return new WaitForSeconds(1.5f);
        if (troopsField != null)
        {
            troopsField.RemoveCard(this);
        }

        Destroy(gameObject);
        //GameManager.Instance.SetPlayerInput(true);
    }
    private IEnumerator MoveToPosition(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
    }
    public void AddStatusEffect(StatusEffect effectPrefab, int power)
    {
        // Check if an effect of the same type already exists
        StatusEffect existingEffect = activeEffects.Find(e => e != null && e.GetType() == effectPrefab.GetType());
        if (existingEffect != null)
        {
            existingEffect.Reapply(effectPrefab, power);
            return;
        }

        StatusEffect newEffect = gameObject.AddComponent(effectPrefab.GetType()) as StatusEffect;
        newEffect.Initialize(this, effectPrefab, power);
        activeEffects.Add(newEffect);
    }

    public IEnumerator ProcessStartOfTurnEffects()
    {
        if (activeEffects == null || activeEffects.Count == 0)
            yield break;
        foreach (var effect in new List<StatusEffect>(activeEffects))
        {
            if (effect != null)
                yield return effect.OnTurnStartCoroutine();
        }

        activeEffects.RemoveAll(e => e == null);
    }
    public IEnumerator Shake(float duration = 0.25f, float magnitude = 0.1f)
    {
        Transform cam = Camera.main.transform;
        Vector3 originalPos = transform.position;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // This vector is "left-right" relative to camera
            Vector3 cameraRight = cam.right;

            // Random offset left/right
            float offset = Random.Range(-1f, 1f) * magnitude;

            transform.position = originalPos + cameraRight * offset;

            yield return null;
        }

        transform.position = originalPos;
    }
    public Sprite GetCardVisual()
    {
        if (cardSprite != null)
            return cardSprite.sprite;
        return null;
    }
    public IEnumerator ProcessPassivesTurnStart()
    {
        foreach (var passive in passiveSkills)
        {
            yield return passive.OnTurnStart();
        }
    }
}
