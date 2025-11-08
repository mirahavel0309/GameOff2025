using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum CardState { InHand, OnField, InDeck, Destroyed }
[RequireComponent(typeof(SpriteRenderer))]
public class CardInstance : MonoBehaviour
{

    [Header("Card Data")]
    public Card baseCard;
    public int currentAttack;
    public int currentHealth;
    public CardState state;

    [Header("References")]
    [SerializeField] protected TextMeshPro attackText;
    [SerializeField] protected TextMeshPro healthText;
    [SerializeField] private SpriteRenderer cardSprite;
    [SerializeField] private SpriteRenderer highlightSprite;
    public TroopsField troopsField;

    // New: track what container this card currently belongs to
    public MonoBehaviour currentContainer; // could be PlayerHand or TroopsField

    public bool HasActedThisTurn { get; set; } = false;

    private bool isSelected = false;
    private Vector3 originalPosition;
    protected static CardInstance selectedAttacker;
    private void Awake()
    {

    }
    void Start()
    {
        Initialize();
    }
    public virtual void Initialize()
    {
        if (baseCard != null)
        {
            currentAttack = baseCard.attack;
            currentHealth = baseCard.health;
            UpdateVisuals();
        }

        if (highlightSprite != null)
            highlightSprite.enabled = false;
    }

    public void SetCardData(Card cardData)
    {
        baseCard = cardData;
        currentAttack = cardData.attack;
        currentHealth = cardData.health;
        UpdateVisuals();
    }

    public void ChangeState(CardState newState)
    {
        state = newState;
    }

    void OnMouseDown()
    {

        //if (!GameManager.Instance.PlayerInputEnabled) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick();
        }

        // If player right-clicks, cancel
        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }
    protected virtual void HandleLeftClick()
    {
        GameManager.Instance.SelectTarget(this);

        if (state == CardState.OnField)
        {
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
                if (selectedAttacker != null)
                {
                    if(GameManager.Instance.IsActionPending("Attack"))
                        StartCoroutine(selectedAttacker.PerformAttack(this));
                    if(GameManager.Instance.IsActionPending("Cast"))
                        GameManager.Instance.GetSelectedHero().CastOnEnemy(this);
                }
            }
        }
    }
    protected virtual void HandleRightClick()
    {
        if (state == CardState.OnField)
        {
            // If player right-clicks, cancel
            if (Input.GetMouseButtonDown(1))
            {
                ClearSelection();
                return;
            }
        }
    }

    private void PlayCard()
    {
        //// Make sure to remove this card from its current container first
        //if (currentContainer is PlayerHand hand)
        //{
        //    hand.RemoveCard(this);
        //}

        // Add card to the troops field
        if (troopsField != null)
        {
            troopsField.AddCard(this);
            currentContainer = troopsField;
        }

        ChangeState(CardState.OnField);
    }

    public virtual void UpdateVisuals()
    {
        if (attackText != null) attackText.text = currentAttack.ToString();
        if (healthText != null) healthText.text = currentHealth.ToString();

        if (cardSprite != null && baseCard != null && baseCard.artwork != null)
            cardSprite.sprite = baseCard.artwork;
    }
    private void OnMouseEnter()
    {
        isSelected = true;
        ToggleSelection();
    }
    private void OnMouseExit()
    {
        isSelected = false;
        ToggleSelection();
    }

    private void ToggleSelection()
    {
        if (highlightSprite != null)
        {
            if (isSelected)
            {
                highlightSprite.color = new Color(0f, 1f, 0f, 0.5f);
                highlightSprite.enabled = true;
            }
            else
            {
                highlightSprite.enabled = false;
            }
        }
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
    private void ToggleHighlight(bool active)
    {
        if (highlightSprite != null)
        {
            highlightSprite.color = active ? new Color(0f, 1f, 0f, 0.5f) : Color.clear;
            highlightSprite.enabled = active;
        }
    }
    public IEnumerator PerformAttack(CardInstance target)
    {
        if (target == null || target.state == CardState.Destroyed) yield break;

        GameManager.Instance.SetPlayerInput(false);
        ToggleHighlight(false);
        selectedAttacker = null;
        GameManager.Instance.SelectHero(null);
        HasActedThisTurn = true;

        originalPosition = transform.position;

        // Move toward target (0.25s)
        yield return MoveToPosition(target.transform.position, 0.25f);

        // Deal damage
        target.TakeDamage(currentAttack);

        // Return (0.25s)
        yield return MoveToPosition(originalPosition, 0.25f);

        GameManager.Instance.SetPlayerInput(true);
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        UpdateVisuals();

        if (currentHealth <= 0)
        {
            StartCoroutine(HandleDestruction());
        }
    }
    private IEnumerator HandleDestruction()
    {
        state = CardState.Destroyed;
        GameManager.Instance.SetPlayerInput(false);

        yield return new WaitForSeconds(0.5f);

        if (currentContainer is TroopsField field)
        {
            field.RemoveCard(this);
        }

        Destroy(gameObject);
        GameManager.Instance.SetPlayerInput(true);
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
}
