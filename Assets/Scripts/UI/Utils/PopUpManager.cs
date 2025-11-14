using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour
{
    public static PopUpManager Instance;
    public Button closeButton;
    public TextMeshProUGUI popUpText;
    public GameObject popUpPanel;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        closeButton.onClick.AddListener(ClosePopUp);
    }
    public void ShowMessage(string message)
    {

        popUpText.text = message;
        popUpPanel.SetActive(true);
    }
    private void ClosePopUp()
    {
        popUpPanel.SetActive(false);
    }
}
