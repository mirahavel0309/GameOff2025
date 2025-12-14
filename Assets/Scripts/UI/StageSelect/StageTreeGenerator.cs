using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class StageTreeGenerator
{
    public static StageNode GenerateTree(
        int maxDepth,
        int maxStageIndex,
        int seed = -1
    )
    {
        if (seed != -1)
            Random.InitState(seed);

        StageNode root = new StageNode(0, 0);
        GenerateChildren(root, maxDepth, maxStageIndex);
        return root;
    }

    private static void GenerateChildren(
        StageNode parent,
        int maxDepth,
        int maxStageIndex
    )
    {
        if (parent.depth >= maxDepth)
            return;

        int childCount = Random.Range(2, 4); // 2–3 children

        for (int i = 0; i < childCount; i++)
        {
            int stageIndex = Random.Range(0, maxStageIndex + 1);
            StageNode child = new StageNode(
                stageIndex,
                parent.depth + 1,
                parent
            );

            parent.children.Add(child);
            GenerateChildren(child, maxDepth, maxStageIndex);
        }
    }
}
