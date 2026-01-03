using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    // Start is called before the first frame update
    public RectTransform dialogueBox;
    public TextMeshProUGUI dialogueText;
    public GameObject nextKeyHint;

    public Transform topPos;
    public Transform bottomPos;

    public void ShowDialogue(string text)
    {
        dialogueBox.gameObject.SetActive(true);
        dialogueText.text = text;
        nextKeyHint.SetActive(true);
    }

    public void HideDialogue()
    {
        dialogueBox.gameObject.SetActive(false);
        nextKeyHint.SetActive(false);
    }

    public void MoveToTop()
    {
        dialogueBox.position = topPos.position;
    }
    public void MoveToBottom()
    {
        dialogueBox.position = bottomPos.position;
    }
}
