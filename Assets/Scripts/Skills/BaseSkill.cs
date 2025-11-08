using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class BaseSkill : MonoBehaviour
{
    public string skillName;
    public string description;
    public List<ElementType> requiredElements = new List<ElementType>();

    public abstract void Execute();

    public bool Matches(List<ElementType> elements)
    {
        if (requiredElements.Count != elements.Count)
            return false;

        foreach (var elem in requiredElements)
            if (!elements.Contains(elem))
                return false;

        return true;
    }
}
