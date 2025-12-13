using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class StageTreeLayout
{
    public static void AssignThreeLayerLayout(
        StageNode current,
        float xSpacing = 280f,
        float ySpacing = 180f
    )
    {
        // Layer 0 — current node
        current.uiPosition = new Vector2 (0, 180f);

        // Layer 1 — children
        int childCount = current.children.Count;
        float totalWidth = (childCount - 1) * xSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < childCount; i++)
        {
            StageNode child = current.children[i];
            float childX = startX + i * xSpacing;
            float childY = current.uiPosition.y - ySpacing;
            child.uiPosition = new Vector2(childX, childY);

            // Layer 2 — grandchildren (centered under each child)
            int grandCount = child.children.Count;
            if (grandCount == 0)
                continue;

            float gcWidth = (grandCount - 1) * (xSpacing * 0.4f);
            float gcStartX = childX - gcWidth / 2f;

            for (int j = 0; j < grandCount; j++)
            {
                StageNode grandChild = child.children[j];
                float gcX = gcStartX + j * (xSpacing * 0.7f);
                float gcY = current.uiPosition.y - 2f * ySpacing;
                grandChild.uiPosition = new Vector2(gcX, gcY);
            }
        }
    }
}

