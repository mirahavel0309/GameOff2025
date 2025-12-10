using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDialog : MonoBehaviour
{
    public List<DialogModel> dialog;
    public Image characterImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void TriggerDialog()
    {
        DialogEvents.OnDialogRequested?.Invoke(dialog, characterImage);
    }
}
