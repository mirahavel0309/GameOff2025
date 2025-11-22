using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Image fillImage;
    public void SetValue(float current, float max)
    {
        if (max <= 0)
        {
            fillImage.fillAmount = 0f;
            return;
        }

        float amount = current / max;
        fillImage.fillAmount = Mathf.Clamp01(amount);
    }
}
