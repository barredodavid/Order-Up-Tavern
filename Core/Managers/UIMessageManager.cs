using UnityEngine;
using TMPro;

public class UIMessageManager : MonoBehaviour
{
    public static UIMessageManager Instance { get; private set; }

    [Header("UI Elements")]
    [Tooltip("Texte pour les messages d'interaction (Appuie sur E...)")]
    public TMP_Text interactionText;

    [Header("Configuration")]
    [Tooltip("Temps d'affichage du message temporaire")]
    public float messageDuration = 3f;

    void Awake()
    {
        // Singleton
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
        // Masquer tous les textes au départ
        HideInteractionMessage();
    }

    /// <summary>
    /// Affiche un message d'interaction (ex: "Appuie sur E...")
    /// </summary>
    public void ShowInteractionMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            HideInteractionMessage();
            return;
        }

        if (interactionText != null)
        {
            interactionText.text = message;
            interactionText.enabled = true;
        }
        else
        {
            Debug.LogWarning("⚠️ InteractionText n'est pas assigné dans UIMessageManager !");
        }
    }

    /// <summary>
    /// Cache le message d'interaction
    /// </summary>
    public void HideInteractionMessage()
    {
        if (interactionText != null)
        {
            interactionText.enabled = false;
            interactionText.text = "";
        }
    }

    /// <summary>
    /// Affiche un message temporaire qui disparaît après un délai
    /// </summary>
    public void ShowTemporaryMessage(string message, float duration = -1f)
    {
        if (duration < 0)
        {
            duration = messageDuration;
        }

        ShowInteractionMessage(message);
        
        // Annuler la coroutine précédente si elle existe
        StopAllCoroutines();
        StartCoroutine(HideMessageAfterDelay(duration));
    }

    System.Collections.IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideInteractionMessage();
    }

    /// <summary>
    /// Vérifie si un message est actuellement affiché
    /// </summary>
    public bool IsMessageVisible()
    {
        return interactionText != null && interactionText.enabled;
    }
}