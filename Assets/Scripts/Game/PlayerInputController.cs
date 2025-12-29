using UnityEngine;
using UnityEngine.UI;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button drawCardButton;

    public bool InputEnabled { get; private set; } = false;
    public bool EndTurnPressed { get; set; } = false;

    void Start()
    {
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnPressed);
        if (drawCardButton != null)
            drawCardButton.onClick.AddListener(OnDrawCardPressed);
    }

    public void SetInputEnabled(bool enabled)
    {
        InputEnabled = enabled;

        // Optionally disable visual interaction
        if (endTurnButton != null)
            endTurnButton.interactable = enabled;
    }
    private void Update()
    {
        UpdateDrawButtonState();
    }

    private void OnEndTurnPressed()
    {
        if (!InputEnabled)
            return;

        EndTurnPressed = true;
    }
    private void OnDrawCardPressed()
    {
        if (!InputEnabled)
            return;

        // Safety checks
        if (GameManager.Instance.actionsThisTurn <= 0)
            return;

        if (PlayerHand.instance.GetCards().Count >= GameManager.Instance.maxCardsInHand)
            return;

        GameManager.Instance.DrawCard();
    }
    private void UpdateDrawButtonState()
    {
        if (drawCardButton == null)
            return;

        bool canDraw =
            InputEnabled &&
            GameManager.Instance.actionsThisTurn > 0 &&
            PlayerHand.instance.GetCards().Count < GameManager.Instance.maxCardsInHand;

        drawCardButton.interactable = canDraw;
    }
}
