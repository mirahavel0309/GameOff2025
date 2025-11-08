using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeroActionMenu : MonoBehaviour
{
    public Button attackButton;
    public Button castButton;
    public Button defendButton;

    private HeroInstance hero;
    private Vector3[] targetPositions;
    private float radius = 1.2f;
    private float animationTime = 0.2f;

    public void Initialize(HeroInstance attachedHero)
    {
        hero = attachedHero;
        transform.position = hero.transform.position + Vector3.up * 2f;

        // Assign button callbacks
        attackButton.onClick.AddListener(() => OnActionSelected("Attack"));
        castButton.onClick.AddListener(() => OnActionSelected("Cast"));
        defendButton.onClick.AddListener(() => OnActionSelected("Defend"));

        //// Define target positions (120° apart)
        //targetPositions = new Vector3[3];
        //for (int i = 0; i < 3; i++)
        //{
        //    float angle = (120 * i) * Mathf.Deg2Rad;
        //    targetPositions[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
        //}

        //StartCoroutine(AnimateButtonsOutward());
    }

    private IEnumerator AnimateButtonsOutward()
    {
        Vector3 center = Vector3.zero;
        Vector3[] startPositions = new Vector3[] { center, center, center };
        Button[] buttons = new Button[] { attackButton, castButton, defendButton };

        float elapsed = 0;
        while (elapsed < animationTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationTime);

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].transform.localPosition = Vector3.Lerp(startPositions[i], targetPositions[i], t);
            }

            yield return null;
        }
    }

    private void OnActionSelected(string action)
    {
        GameManager.Instance.OnHeroActionChosen(hero, action);
        Destroy(gameObject);
    }
}
