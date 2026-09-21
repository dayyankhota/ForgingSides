using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI displayText;

    [Header("Story Dialogue")]
    [TextArea(3, 5)]
    public string[] dialogueMessages;

    [Header("Tutorial Instructions")]
    [TextArea(3, 5)]
    public string[] tutorialMessages;

    private int currentDialogueIndex = 0;
    private int currentTutorialIndex = 0;
    private bool isShowingDialogue = true;

    void Start()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowNextMessage();
    }

    public void ShowNextMessage()
    {
        if (isShowingDialogue)
        {
            if (currentDialogueIndex < dialogueMessages.Length)
            {
                displayText.text = dialogueMessages[currentDialogueIndex];
                currentDialogueIndex++;
            }
            else
            {
                isShowingDialogue = false;
                ShowNextMessage();
            }
        }
        else
        {
            if (currentTutorialIndex < tutorialMessages.Length)
            {
                displayText.text = tutorialMessages[currentTutorialIndex];
                currentTutorialIndex++;
            }
            else
            {
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                gameObject.SetActive(false);
            }
        }
    }
}