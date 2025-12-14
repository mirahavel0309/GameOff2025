using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class StageNode
{
    public int stageIndex;
    public StageNode parent;
    public List<StageNode> children = new();

    public int depth;

    // UI position (local space)
    public Vector2 uiPosition;

    public StageNode(int stageIndex, int depth, StageNode parent = null)
    {
        this.stageIndex = stageIndex;
        this.depth = depth;
        this.parent = parent;
    }
}
