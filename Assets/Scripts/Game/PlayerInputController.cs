using UnityEngine;
using UnityEngine.UI;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;

    public bool InputEnabled { get; private set; } = false;
    public bool EndTurnPressed { get; set; } = false;

    void Start()
    {
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnPressed);
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        InputEnabled = enabled;

        // Optionally disable visual interaction
        if (endTurnButton != null)
            endTurnButton.interactable = enabled;
    }

    private void OnEndTurnPressed()
    {
        if (!InputEnabled)
            return;

        EndTurnPressed = true;
        Debug.Log("End Turn Pressed!");
    }
}
