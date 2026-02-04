using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Indicateur de Suite")]
    [Tooltip("L'objet (Texte ou Flèche) qui dit 'Espace pour continuer'")]
    public GameObject continueIndicator;

    [Header("Settings")]
    public float typingSpeed = 0.05f;

    private Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
    private bool isTyping = false;
    private string currentFullText = "";
    private Coroutine typingCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("⚠️ dialoguePanel est null dans DialogueManager");
        }

        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        
        // Appuyer sur Espace pour passer
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isTyping)
            {
                // CAS 1 : On tape encore -> On complète tout d'un coup
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                    typingCoroutine = null;
                }
                
                if (dialogueText != null)
                {
                    dialogueText.text = currentFullText;
                }
                
                isTyping = false;
                
                // On affiche l'indicateur car le texte est fini
                if (continueIndicator != null)
                {
                    continueIndicator.SetActive(true);
                }
            }
            else if (dialogueQueue.Count > 0)
            {
                // CAS 2 : Texte fini, il en reste d'autres -> On passe au suivant
                DisplayNextDialogue();
            }
            else
            {
                // CAS 3 : Tout est fini -> On ferme
                EndDialogue();
            }
        }
    }

    /// <summary>
    /// Fonction pratique pour afficher un dialogue simple
    /// </summary>
    public void ShowSimpleDialogue(string name, string text, Sprite portrait = null)
    {
        dialogueQueue.Clear();
        DialogueLine line = new DialogueLine(name, portrait, text);
        dialogueQueue.Enqueue(line);
        
        if (dialoguePanel != null && !dialoguePanel.activeSelf)
        {
            dialoguePanel.SetActive(true);
            DisplayNextDialogue();
        }
    }

    public void StartDialogue(List<DialogueLine> dialogues)
    {
        if (dialogues == null || dialogues.Count == 0)
        {
            Debug.LogWarning("⚠️ Tentative de démarrer un dialogue vide");
            return;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        
        dialogueQueue.Clear();

        foreach (DialogueLine line in dialogues)
        {
            if (line != null)
            {
                dialogueQueue.Enqueue(line);
            }
        }

        DisplayNextDialogue();
    }

    void DisplayNextDialogue()
    {
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = dialogueQueue.Dequeue();
        
        if (currentLine == null)
        {
            Debug.LogWarning("⚠️ DialogueLine null dans la queue");
            DisplayNextDialogue();
            return;
        }

        currentFullText = currentLine.text;

        if (nameText != null)
        {
            nameText.text = currentLine.characterName;
        }
        
        if (portraitImage != null)
        {
            if (currentLine.portrait != null)
            {
                portraitImage.gameObject.SetActive(true);
                portraitImage.sprite = currentLine.portrait;
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
            }
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        typingCoroutine = StartCoroutine(TypeText(currentLine.text));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
        
        // On cache la flèche pendant qu'on écrit
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }

        foreach (char letter in text.ToCharArray())
        {
            if (dialogueText != null)
            {
                dialogueText.text += letter;
            }
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        
        // C'EST FINI D'ÉCRIRE : ON MONTRE LA FLÈCHE
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(true);
        }
        
        typingCoroutine = null;
    }

    void EndDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        dialogueQueue.Clear();
        
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }
        
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        Debug.Log("✅ Dialogue terminé");
    }

    public void CloseDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        dialogueQueue.Clear();
        
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }
    }

    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }
}

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    public Sprite portrait;
    [TextArea(3, 10)]
    public string text;

    public DialogueLine(string name, Sprite sprite, string dialogue)
    {
        characterName = name;
        portrait = sprite;
        text = dialogue;
    }

    public DialogueLine() {}
}