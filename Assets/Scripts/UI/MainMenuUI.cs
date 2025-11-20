using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuUI : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private CanvasGroup mainMenu;
    [SerializeField] private CanvasGroup credits;
    [SerializeField] private CanvasGroup settings;

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private CanvasGroup currentMenu;

    void Start()
    {
        mainMenu.gameObject.SetActive(true);
        credits.gameObject.SetActive(false);
        settings.gameObject.SetActive(false);

        mainMenu.alpha = 1;
        credits.alpha = 0;
        settings.alpha = 0;

        currentMenu = mainMenu;
    }

    public void PlayGame()
    {
        SceneController.ToHeroSelection();
    }

    public void ShowMainMenu()
    {
        StartCoroutine(SwitchMenu(mainMenu));
    }

    public void ShowCredits()
    {
        StartCoroutine(SwitchMenu(credits));
    }

    public void ShowSettings()
    {
        StartCoroutine(SwitchMenu(settings));
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator SwitchMenu(CanvasGroup newMenu)
    {
        if (newMenu == currentMenu) yield break;
        newMenu.gameObject.SetActive(true);
        newMenu.alpha = 0;
        newMenu.interactable = false;
        newMenu.blocksRaycasts = false;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            currentMenu.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        currentMenu.alpha = 0f;
        currentMenu.interactable = false;
        currentMenu.blocksRaycasts = false;
        currentMenu.gameObject.SetActive(false);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            newMenu.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        newMenu.alpha = 1f;
        newMenu.interactable = true;
        newMenu.blocksRaycasts = true;

        currentMenu = newMenu;
    }
}
