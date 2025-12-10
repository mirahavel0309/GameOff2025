using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class DialogModel
{
    [TextArea(4,4)]
    public string text;
    public bool isThatMeSpeaking;

}
