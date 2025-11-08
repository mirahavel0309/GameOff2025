using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InfoPanel : MonoBehaviour
{
    public static InfoPanel instance;
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float autoHideDelay = 0f; // 0 = manual hide only

    private Coroutine fadeRoutine;

    private void Awake()
    {
        instance = this;
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideInstant();
    }

    public void ShowMessage(string message, float duration = 0f)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        infoText.text = message;
        fadeRoutine = StartCoroutine(FadeIn(message, duration));
    }

    public void Hide()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn(string message, float duration)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Auto-hide if duration provided or autoHideDelay set
        float waitTime = (duration > 0f) ? duration : autoHideDelay;
        if (waitTime > 0f)
        {
            yield return new WaitForSeconds(waitTime);
            yield return FadeOut();
        }
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

    public void HideInstant()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        canvasGroup.alpha = 0f;
    }
}
