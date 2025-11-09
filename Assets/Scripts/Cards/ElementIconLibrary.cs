using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ElementIconLibrary", menuName = "Card Game/Element Icon Library")]
public class ElementIconLibrary : ScriptableObject
{
    [System.Serializable]
    public class ElementIconPair
    {
        public ElementType element;
        public Sprite icon;
        public GameObject elementalProjectilePrefab; // <-- NEW
    }

    public List<ElementIconPair> icons = new List<ElementIconPair>();

    private Dictionary<ElementType, Sprite> iconLookup;
    public Sprite GetIcon(ElementType element)
    {
        if (iconLookup == null)
        {
            iconLookup = new Dictionary<ElementType, Sprite>();
            foreach (var pair in icons)
                iconLookup[pair.element] = pair.icon;
        }

        return iconLookup.TryGetValue(element, out Sprite result) ? result : null;
    }
    public GameObject GetElementProjectilePrefab(ElementType element)
    {
        var pair = icons.Find(e => e.element == element);
        return pair?.elementalProjectilePrefab;
    }
}
