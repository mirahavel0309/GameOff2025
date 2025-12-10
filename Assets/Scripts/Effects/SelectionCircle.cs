using UnityEngine;

public enum SelectionState {Active, Inactive, Red, Green, Yellow };
public class SelectionCircle : MonoBehaviour
{
    public ParticleSystem circle;
    public Color colorInactive;
    public Color colorActive;
    public Color colorRed;
    public Color colorGreen;
    public Color colorYellow;
    void Awake()
    {
        circle = GetComponentInChildren<ParticleSystem>();
        Hide(); // hide by default
    }
    public void Hide()
    {
        circle.gameObject.SetActive(false);
    }
    public void Show(SelectionState state)
    {
        circle.gameObject.SetActive(true);
        ParticleSystem.MainModule mm = circle.main;
        switch (state)
        {
            case SelectionState.Active:
                mm.startColor = colorActive;
                break;
            case SelectionState.Inactive:
                mm.startColor = colorInactive;
                break;
            case SelectionState.Red:
                mm.startColor = colorRed;
                break;
            case SelectionState.Green:
                mm.startColor = colorGreen;
                break;
            case SelectionState.Yellow:
                mm.startColor = colorYellow;
                break;
            default:
                mm.startColor = colorInactive;
                break;
        }
        circle.Clear();
        circle.Play();
    }
}
