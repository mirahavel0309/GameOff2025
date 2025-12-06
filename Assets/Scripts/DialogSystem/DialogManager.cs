using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    
    [Tooltip("Text component to display the dialog content.")]
    [SerializeField] private GameObject dialogPanel;

    void OnEnable()
    {
        DialogEvents.OnDialogRequested += StartDialog;
    }

    void OnDisable()
    {
        DialogEvents.OnDialogRequested -= StartDialog;
    }

    private void StartDialog(List<DialogModel> dialogList, Image characterImage)
    {
        dialogPanel.SetActive(true);
        DialogPanel panelScript = dialogPanel.GetComponent<DialogPanel>();
        panelScript.SetDialog(dialogList, characterImage);
    }
}
