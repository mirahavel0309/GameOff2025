using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum NodeClickMode
{
    Current,
    Selectable,
    Preview
}





public class StageSelectMenu : MonoBehaviour
{
    private List<GameObject> activeLines = new();

    public static StageSelectMenu Instance;
    public Transform buttonParent;
    public GameObject stageButtonPrefab;
    [Header("Connections")]
    public GameObject connectionLinePrefab;
    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Open(StageNode currentNode)
    {
        gameObject.SetActive(true);

        foreach (Transform child in buttonParent)
            Destroy(child.gameObject);

        // Layout positions
        StageTreeLayout.AssignThreeLayerLayout(currentNode);

        // Current node
        SpawnNode(currentNode, NodeClickMode.Current);

        // Children (clickable)
        foreach (StageNode child in currentNode.children)
        {
            SpawnNode(child, NodeClickMode.Selectable);

            // Grandchildren (preview only)
            foreach (StageNode grandChild in child.children)
                SpawnNode(grandChild, NodeClickMode.Preview);
        }

        DrawConnections(currentNode);
    }

    private void DrawConnections(StageNode current)
    {
        // Clear old lines
        foreach (var line in activeLines)
            Destroy(line);
        activeLines.Clear();

        // Current ? children
        foreach (StageNode child in current.children)
            DrawLine(current.uiPosition, child.uiPosition);

        // Children ? grandchildren
        foreach (StageNode child in current.children)
        {
            foreach (StageNode grandChild in child.children)
                DrawLine(child.uiPosition, grandChild.uiPosition);
        }
    }
    private void DrawLine(Vector2 from, Vector2 to)
    {
        GameObject line = Instantiate(connectionLinePrefab, buttonParent);
        RectTransform rt = line.GetComponent<RectTransform>();

        Vector2 dir = to - from;
        float distance = dir.magnitude;

        rt.sizeDelta = new Vector2(6f, distance);
        rt.anchoredPosition = from + dir / 2f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle - 90f);

        activeLines.Add(line);
    }



    private void SpawnNode(StageNode node, NodeClickMode mode)
    {
        GameObject btn = Instantiate(stageButtonPrefab, buttonParent);
        StageButton stageButton = btn.GetComponent<StageButton>();
        stageButton.Initialize(node);

        RectTransform rt = btn.GetComponent<RectTransform>();
        rt.anchoredPosition = node.uiPosition;

        Button uiButton = btn.GetComponent<Button>();

        switch (mode)
        {
            case NodeClickMode.Preview:
                uiButton.interactable = false;
                rt.localScale = Vector3.one * 0.9f;
                btn.GetComponent<CanvasGroup>().alpha = 0.6f;
                break;

            case NodeClickMode.Current:
                rt.localScale = Vector3.one * 1.2f;
                uiButton.interactable = false;
                break;
        }
    }





    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void SelectNode(StageNode node)
    {
        GameManager.Instance.OnStageNodeSelected(node);
    }
}
