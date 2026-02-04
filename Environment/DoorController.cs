using UnityEngine;
using UnityEngine.AI;

public class DoorController : MonoBehaviour
{
    [Header("Paramètres")]
    [Tooltip("Angle d'ouverture de la porte")]
    public float openAngle = 90f;
    
    [Tooltip("Vitesse d'ouverture/fermeture")]
    public float openSpeed = 2f;
    
    [Tooltip("La porte s'ouvre automatiquement quand on approche ?")]
    public bool autoOpen = true;
    
    [Tooltip("Distance pour ouvrir automatiquement")]
    public float openDistance = 3f;
    
    [Tooltip("S'ouvre aussi pour les NPCs ?")]
    public bool openForNPCs = true;

    [Header("Direction")]
    [Tooltip("Ouvrir vers l'intérieur (false) ou l'extérieur (true)")]
    public bool openOutward = false;

    [Header("Audio (Optionnel)")]
    public AudioClip openSound;
    public AudioClip closeSound;

    private bool isOpen = false;
    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private Quaternion closedRotation;
    private AudioSource audioSource;
    private Transform player;

    void Start()
    {
        // Sauvegarder la rotation initiale (porte fermée)
        closedRotation = transform.localRotation;

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (openSound != null || closeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }

        // Trouver le joueur
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("⚠️ Aucun objet avec le tag 'Player' trouvé");
        }
    }

    void Update()
    {
        // Ouverture automatique
        if (autoOpen)
        {
            bool shouldOpen = false;
            
            // Vérifier le joueur
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance <= openDistance)
                {
                    shouldOpen = true;
                }
            }
            
            // Vérifier les NPCs avec NavMeshAgent
            if (openForNPCs)
            {
                NavMeshAgent[] agents = FindObjectsOfType<NavMeshAgent>();
                foreach (NavMeshAgent agent in agents)
                {
                    if (agent != null && agent.enabled && agent.isActiveAndEnabled)
                    {
                        float distance = Vector3.Distance(transform.position, agent.transform.position);
                        if (distance <= openDistance)
                        {
                            shouldOpen = true;
                            break;
                        }
                    }
                }
            }
            
            // Ouvrir ou fermer selon la proximité
            if (shouldOpen && !isOpen)
            {
                Open();
            }
            else if (!shouldOpen && isOpen)
            {
                Close();
            }
        }

        // Animation fluide
        if (!Mathf.Approximately(currentAngle, targetAngle))
        {
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * openSpeed);

            // Rotation selon la direction
            float angleToApply = openOutward ? currentAngle : -currentAngle;
            transform.localRotation = closedRotation * Quaternion.Euler(0, angleToApply, 0);
        }
    }

    /// <summary>
    /// Ouvrir la porte
    /// </summary>
    public void Open()
    {
        if (isOpen) return;

        isOpen = true;
        targetAngle = openAngle;

        // Son d'ouverture
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }
        
        Debug.Log($"🚪 Porte {gameObject.name} ouverte");
    }

    /// <summary>
    /// Fermer la porte
    /// </summary>
    public void Close()
    {
        if (!isOpen) return;

        isOpen = false;
        targetAngle = 0f;

        // Son de fermeture
        if (audioSource != null && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }
        
        Debug.Log($"🚪 Porte {gameObject.name} fermée");
    }

    /// <summary>
    /// Toggle (ouvrir/fermer)
    /// </summary>
    public void Toggle()
    {
        if (isOpen)
            Close();
        else
            Open();
    }

    // Afficher la zone de détection dans l'éditeur
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, openDistance);
    }
}