using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [Header("UI Interface")]
    [Tooltip("Le Panel Noir (doit avoir un CanvasGroup)")]
    public CanvasGroup blackScreenGroup; 
    public TextMeshProUGUI dayTitleText;   // "JOUR 1"
    public TextMeshProUGUI objectiveText;  // "Objectifs..."
    public TextMeshProUGUI statsText;      // "Clients : 0/10"

    [Header("Configuration Reset")]
    [Tooltip("Crée un objet vide dans la scène là où le joueur doit apparaître le matin")]
    public Transform playerSpawnPoint; 

    [Header("Objectifs")]
    public int targetClients = 10;
    public int maxAngryClients = 5;

    // Compteurs
    private int clientsServed = 0;
    private int clientsAngry = 0;
    private bool isDayActive = false;

    [Header("Respawn Objets")]
    public GameObject glassPrefab;      // Le modèle du verre
    public Transform[] glassSlots;      // Les 9 emplacements dans le placard

    [Header("Connexions")]
    public CustomerSpawner spawner;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Au lancement du jeu, on s'assure que l'écran noir est caché
        if (blackScreenGroup != null)
        {
            blackScreenGroup.alpha = 0;
            blackScreenGroup.gameObject.SetActive(false);
        }
        UpdateStatsUI();
    }

    // Appelée par GameManager quand on appuie sur F
    public void StartDaySequence()
    {
        StartCoroutine(SequenceRoutine());
    }

    IEnumerator SequenceRoutine()
    {
        // 1. FONDU AU NOIR (Fade In)
        if (blackScreenGroup != null)
        {
            blackScreenGroup.gameObject.SetActive(true);
            float t = 0;
            while (t < 1)
            {
                // On utilise 'unscaledDeltaTime' car le jeu est en pause (TimeScale = 0)
                t += Time.unscaledDeltaTime * 2; 
                blackScreenGroup.alpha = t;
                yield return null;
            }
            blackScreenGroup.alpha = 1; // On force le noir total
        }

        // --- 🧹 LE MÉNAGE INSTANTANÉ ---
        ResetTavernState();
        // -------------------------------

        // 2. AFFICHER LES TEXTES
        if (dayTitleText) 
        { 
            dayTitleText.text = "JOUR 1"; 
            dayTitleText.gameObject.SetActive(true); 
        }

        if (objectiveText) 
        { 
            objectiveText.text = $"Objectif : Servir {targetClients} clients.\nCondition : Ne laisse pas {maxAngryClients} clients partir sans être servis.";
            objectiveText.gameObject.SetActive(true); 
        }

        // 3. PAUSE (On laisse le joueur lire)
        yield return new WaitForSecondsRealtime(1.5f);

        // 4. CACHER LES TEXTES
        if (dayTitleText) dayTitleText.gameObject.SetActive(false);
        if (objectiveText) objectiveText.gameObject.SetActive(false);

        // 5. LANCER LE GAMEPLAY (Réactive le temps)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartDayOne();
        }

        if (spawner != null)
        {
            // On lance la vague immédiatement : 10 clients, toutes les 3 secondes
            spawner.StartDailyWave(targetClients, 3.0f);
        }

        isDayActive = true;

        // 6. FONDU VERS LE JEU (Fade Out)
        if (blackScreenGroup != null)
        {
            float t = 1;
            while (t > 0)
            {
                t -= Time.unscaledDeltaTime * 1.5f;
                blackScreenGroup.alpha = t;
                yield return null;
            }
            blackScreenGroup.gameObject.SetActive(false);
        }
        
        Debug.Log("🌞 LA JOURNÉE COMMENCE !");
    }

    // --- LA FONCTION QUI REMET TOUT À ZÉRO ---
    void ResetTavernState()
    {
        // 1. On vide les mains du joueur 
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerInteraction interaction = player.GetComponentInChildren<PlayerInteraction>();
            if (interaction != null && interaction.currentHeldObject != null)
            {
                // On détruit l'objet tenu immédiatement
                Destroy(interaction.currentHeldObject);
                interaction.currentHeldObject = null;
                if (interaction.handAnimator) 
                {
                    interaction.handAnimator.SetBool("HoldGlass", false);
                    interaction.handAnimator.SetBool("HoldBroom", false);
                }
            }
        }

        // 2. SUPPRIMER LES VERRES SALES 
        DirtyGlass[] dirtyGlasses = FindObjectsOfType<DirtyGlass>();
        foreach (var glass in dirtyGlasses) Destroy(glass.gameObject);

        // 3. SUPPRIMER LES VERRES PROPRES QUI TRAÎNENT
        GlassController[] cleanGlasses = FindObjectsOfType<GlassController>();
        foreach (var glass in cleanGlasses) 
        {
            // RÈGLE DE SÉCURITÉ :
            // Si le verre a un parent (ex: Etagère, Placard), ON LE GARDE.
            // Si le verre n'a PAS de parent (il est à la racine de la scène car spawn ou lâché), ON LE DÉTRUIT.
            if (glass.transform.parent == null)
            {
                Destroy(glass.gameObject);
            }
        }

        // 4. RANGER LE BALAI
        BroomHolder holder = FindObjectOfType<BroomHolder>();
        if (holder != null)
        {
            // On demande au support de se débrouiller pour tout remettre en ordre
            holder.ResetBroom();
        }

        // 5. TÉLÉPORTER LE JOUEUR
        if (player != null && playerSpawnPoint != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; // On coupe la physique
            
            player.transform.position = playerSpawnPoint.position;
            player.transform.rotation = playerSpawnPoint.rotation;
            
            if (cc != null) cc.enabled = true; // On remet la physique
        }

        // 6. REMPLIR LE PLACARD (SLOTS)
        if (glassPrefab != null && glassSlots != null)
        {
            foreach (Transform slot in glassSlots)
            {
                if (slot != null)
                {
                    // On vérifie s'il y a déjà un verre à cet endroit (rayon de 10cm)
                    Collider[] hits = Physics.OverlapSphere(slot.position, 0.1f);
                    bool glassFound = false;
                    
                    foreach(var hit in hits)
                    {
                        if(hit.GetComponent<GlassController>()) 
                        {
                            glassFound = true;
                            break; 
                        }
                    }

                    // Si l'emplacement est vide, on fait apparaître un verre !
                    if (!glassFound)
                    {
                        // 1. On stocke le verre créé dans une variable
                        GameObject newGlass = Instantiate(glassPrefab, slot.position, slot.rotation);
                        
                        // 2. IMPORTANT : On coupe la physique pour qu'il ne tombe pas
                        Rigidbody rb = newGlass.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.isKinematic = true;  // Il ne bouge plus
                            rb.useGravity = false;  // Il ignore la gravité
                            rb.linearVelocity = Vector3.zero; // On stop tout mouvement résiduel
                        }

                        // Optionnel : On le met comme enfant de l'étagère pour que ce soit rangé
                        newGlass.transform.SetParent(slot);
                    }
                }
            }
        }
    }

    // --- GESTION SCORE ---
    public void RegisterServedClient()
    {
        if (!isDayActive) return;
        clientsServed++;
        UpdateStatsUI();
        if (clientsServed >= targetClients) { isDayActive = false; Debug.Log("VICTOIRE !"); }
    }

    public void RegisterAngryClient()
    {
        if (!isDayActive) return;
        clientsAngry++;
        UpdateStatsUI();
        if (clientsAngry >= maxAngryClients) { isDayActive = false; Debug.Log("PERDU !"); }
    }

    void UpdateStatsUI()
    {
        if (statsText) statsText.text = $"Servis : {clientsServed}/{targetClients} \nÉchecs : {clientsAngry}/{maxAngryClients}";
    }
}