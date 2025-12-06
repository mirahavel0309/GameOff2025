using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogPanel : MonoBehaviour
{
    [Header("Panel UI Elements")]
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Image character1;
    [SerializeField] private Image character2;

    [Header("Dialog Settings")]
    [SerializeField] private float charsPerSecond = 40f;

    private Coroutine typingRoutine;
    private int currentDialogIndex = 0;
    private List<DialogModel> dialogModels;


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            NextLine();
        }

    }
    public void SetDialog(List<DialogModel> dialogs, Image characterImage)
    {
        if (character1 == null)
        {
            //character1 = characterImage; ----------Just Idea
        }
        dialogModels = dialogs;
        currentDialogIndex = 0;

        gameObject.SetActive(true);
        ShowCurrentLine();
    }

    public void NextLine()
    {
        if (typingRoutine != null)
        {

            FinishTypingInstantly();
            return;
        }

        currentDialogIndex++;

        if (currentDialogIndex >= dialogModels.Count)
        {
            gameObject.SetActive(false);
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (dialogModels == null || dialogModels.Count == 0) return;

        var line = dialogModels[currentDialogIndex];


        if (line.isThatMeSpeaking)
        {
            character1.color = Color.white;
            character2.color = Color.gray;
        }
        else
        {
            character1.color = Color.gray;
            character2.color = Color.white;
        }

        StartTyping(line.text);
    }

    public void StartTyping(string fullText)
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(TypeText(fullText));
    }

    private IEnumerator TypeText(string fullText)
    {
        dialogText.text = fullText;
        dialogText.maxVisibleCharacters = 0;

        float delay = 1f / charsPerSecond;
        int total = GetVisibleCharacterCount(fullText);

        int count = 0;
        while (count < total)
        {
            count++;
            dialogText.maxVisibleCharacters = count;
            yield return new WaitForSeconds(delay);
        }

        typingRoutine = null;
    }

    private void FinishTypingInstantly()
    {
        StopCoroutine(typingRoutine);
        typingRoutine = null;
        dialogText.maxVisibleCharacters = dialogText.text.Length;
    }

    private int GetVisibleCharacterCount(string text)
    {
        int count = 0;
        bool insideTag = false;

        foreach (char c in text)
        {
            if (c == '<') { insideTag = true; continue; }
            if (c == '>') { insideTag = false; continue; }
            if (!insideTag) count++;
        }

        return count;
    }
}
