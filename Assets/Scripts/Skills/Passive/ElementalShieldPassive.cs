using UnityEngine;

public class ElementalShieldPassive : PassiveSkill
{
    [Header("Shield Settings")]
    public int shieldAmount = 10;
    public int maxShieldAmount = 10;

    [Header("Elemental Modifiers")]
    public ElementType strongAgainst;
    public ElementType weakAgainst;

    [Header("Visual Prefabs")]
    public GameObject visualPrefab;
    public GameObject shieldBarPrefab;

    private GameObject visualInstance;
    private GameObject shieldBarInstance;

    private ProgressBar shieldBar;
    private CardInstance ownerInstance;

    [Header("Offsets")]
    public Vector3 worldVisualOffset = new Vector3(0, 2.0f, 0);
    public Vector3 uiBarOffset = new Vector3(0, 40f, 0);

    private void Start()
    {
        ownerInstance = GetComponent<CardInstance>();
        if (ownerInstance)
        {
            maxShieldAmount = shieldAmount;

            SpawnVisualIndicator();
            SpawnShieldBarUI();
            UpdateVisuals();
        }
    }
    public override string GetDescription(int spellPower)
    {
        return castDescription.Replace("shield_value", Mathf.RoundToInt(maxShieldAmount * (spellPower / 100f)).ToString());
    }
    public override void InitializeFromCaster(HeroInstance mainHero)
    {
        maxShieldAmount = Mathf.RoundToInt(maxShieldAmount * (mainHero.spellPower / 100f));
        shieldAmount = maxShieldAmount;
    }

    private void SpawnVisualIndicator()
    {
        if (visualPrefab != null)
        {
            visualInstance = Instantiate(
                visualPrefab,
                ownerInstance.transform.position,// + worldVisualOffset,
                Quaternion.identity
            );

            // follow character
            visualInstance.transform.SetParent(ownerInstance.transform);
            visualInstance.transform.localScale = Vector3.one;
        }
    }

    private void SpawnShieldBarUI()
    {
        if (shieldBarPrefab == null) return;

        // Look for canvas under the character
        Canvas canvas = ownerInstance.GetComponentInChildren<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning($"[ElementalShield] No Canvas found on {ownerInstance.name}");
            return;
        }

        shieldBarInstance = Instantiate(shieldBarPrefab, canvas.transform);
        shieldBar = shieldBarInstance.GetComponentInChildren<ProgressBar>();

        // Position inside the canvas (UI space)
        RectTransform rt = shieldBarInstance.GetComponent<RectTransform>();
        if (rt != null)
            rt.anchoredPosition += (Vector2)uiBarOffset;
    }

    public override void OnReceiveDamage(ref int dmg, ElementType dmgType, CardInstance owner)
    {
        if (shieldAmount <= 0) return;

        int modifiedDamage = dmg;

        // Element type adjustments
        if (dmgType == weakAgainst)
            modifiedDamage *= 2;
        else if (dmgType == strongAgainst)
            modifiedDamage = Mathf.CeilToInt(modifiedDamage * 0.5f);

        // Shield absorbs damage
        if (shieldAmount >= modifiedDamage)
        {
            shieldAmount -= modifiedDamage;
            dmg = 0;
            UpdateVisuals();
            CheckShieldBreak();
            return;
        }

        // Shield breaks and loses all remaining points, leftover hits HP
        int leftover = modifiedDamage - shieldAmount;
        shieldAmount = 0;
        if (dmgType == weakAgainst)
            leftover = Mathf.CeilToInt(leftover * 0.5f);
        dmg = leftover;

        UpdateVisuals();
        CheckShieldBreak();
    }

    private void UpdateVisuals()
    {
        if (shieldBar != null)
            shieldBar.SetValue(shieldAmount, maxShieldAmount);
    }

    private void CheckShieldBreak()
    {
        if (shieldAmount <= 0)
        {
            RemoveShield();
        }
    }

    private void RemoveShield()
    {
        // Clean visuals
        if (visualInstance != null)
            Destroy(visualInstance);

        if (shieldBarInstance != null)
            Destroy(shieldBarInstance);

        // Remove passive
        Destroy(this);
    }

    private void OnDestroy()
    {
        // Cleanup failsafe
        if (visualInstance != null) Destroy(visualInstance);
        if (shieldBarInstance != null) Destroy(shieldBarInstance);
    }
}
