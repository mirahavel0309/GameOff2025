using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI label;

    private StageNode node;

    public void Initialize(StageNode node)
    {
        this.node = node;

        if (label != null)
            label.text = $"Stage {node.stageIndex}";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        GameManager.Instance.OnStageNodeSelected(node);
    }
}
