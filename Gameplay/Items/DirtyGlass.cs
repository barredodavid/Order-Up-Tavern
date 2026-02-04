using UnityEngine;

public class DirtyGlass : MonoBehaviour
{
    [Header("Transformation")]
    [Tooltip("Le Prefab du verre PROPRE qui apparaîtra une fois lavé")]
    public GameObject cleanGlassPrefab;

    private bool isQuitting = false;

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    void OnDestroy()
    {
        if (isQuitting) return;

        // 1. Créer un verre propre à la même position
        if (cleanGlassPrefab != null)
        {
            Instantiate(cleanGlassPrefab, transform.position, transform.rotation);
            Debug.Log("✨ Verre propre créé");
        }
        else
        {
            Debug.LogWarning("⚠️ cleanGlassPrefab est null ! Aucun verre propre ne sera créé.");
        }

        // 2. Notifier le GameManager (si en tutoriel)
        if (GameManager.Instance != null && 
            GameManager.Instance.currentState == GameState.TutorialServing)
        {
            Debug.Log("✨ VERRE LAVÉ ! Le joueur a récupéré un verre propre.");
            
            // Débloquer la fin du tutoriel
            GameManager.Instance.UnlockTutorialEnd();
        }
    }
}