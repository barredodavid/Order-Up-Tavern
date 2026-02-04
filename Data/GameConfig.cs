using UnityEngine;

/// <summary>
/// Configuration globale du jeu (ScriptableObject)
/// Créer via : Right Click > Create > Tavern/Game Config
/// Permet de tweaker le gameplay sans toucher au code !
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Tavern/Game Config", order = 0)]
public class GameConfig : ScriptableObject
{
    [Header("🎮 Gameplay Général")]
    [Tooltip("Argent de départ")]
    public int startingMoney = 0;
    
    [Tooltip("Objectif d'argent pour finir la journée")]
    public int dailyGoal = 100;
    
    [Tooltip("Difficulté (1 = facile, 5 = difficile)")]
    [Range(1, 5)]
    public int difficultyLevel = 1;
    
    [Header("👥 Clients")]
    [Tooltip("Temps minimum entre deux spawns de clients")]
    public float minSpawnInterval = 5f;
    
    [Tooltip("Temps maximum entre deux spawns de clients")]
    public float maxSpawnInterval = 10f;
    
    [Tooltip("Nombre maximum de clients en même temps")]
    [Range(1, 10)]
    public int maxSimultaneousCustomers = 3;
    
    [Tooltip("Multiplicateur de patience global")]
    [Range(0.5f, 2f)]
    public float globalPatienceMultiplier = 1f;
    
    [Header("🧹 Nettoyage")]
    [Tooltip("Nombre de coups de balai pour nettoyer une tache")]
    [Range(1, 5)]
    public int hitsToCleanDirt = 3;
    
    [Tooltip("Rayon de nettoyage du balai")]
    public float cleaningRadius = 1.5f;
    
    [Tooltip("Vitesse de disparition des taches")]
    public float dirtFadeSpeed = 0.3f;
    
    [Header("🍺 Service")]
    [Tooltip("Durée de remplissage d'un verre")]
    public float beerFillDuration = 2f;
    
    [Tooltip("Durée de lavage d'un verre")]
    public float washDuration = 1.5f;
    
    [Tooltip("Durée pendant laquelle le client boit")]
    public float drinkingDuration = 4f;
    
    [Header("💰 Économie")]
    [Tooltip("Multiplicateur de prix global")]
    [Range(0.5f, 2f)]
    public float priceMultiplier = 1f;
    
    [Tooltip("Pourboire bonus si service rapide (%)")]
    [Range(0f, 50f)]
    public float quickServiceBonus = 20f;
    
    [Tooltip("Pénalité si client part mécontent")]
    public int angryCustomerPenalty = 5;
    
    [Header("📚 Tutoriel")]
    [Tooltip("Activer le tutoriel au démarrage ?")]
    public bool enableTutorial = true;
    
    [Tooltip("Passer le tutoriel automatiquement (pour le debug)")]
    public bool skipTutorial = false;
    
    [Tooltip("Vitesse d'écriture du texte (secondes par caractère)")]
    [Range(0.01f, 0.1f)]
    public float dialogueTypingSpeed = 0.05f;
    
    [Header("🎨 UI")]
    [Tooltip("Durée d'affichage des messages temporaires")]
    public float uiMessageDuration = 3f;
    
    [Tooltip("Distance d'interaction du joueur")]
    public float interactionDistance = 3f;
    
    [Header("🎵 Audio")]
    [Tooltip("Volume général des effets sonores")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    
    [Tooltip("Volume de la musique")]
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    
    [Header("⚙️ Debug")]
    [Tooltip("Afficher les Gizmos dans la scène")]
    public bool showDebugGizmos = true;
    
    [Tooltip("Mode God (argent infini, patience infinie)")]
    public bool godMode = false;
    
    [Tooltip("Logs verbeux dans la console")]
    public bool verboseLogs = false;

    /// <summary>
    /// Obtenir le temps d'attente d'un client avec les modificateurs
    /// </summary>
    public float GetCustomerWaitTime(float baseWaitTime)
    {
        float difficulty = 1f - (difficultyLevel - 1) * 0.15f; // Plus dur = moins de patience
        return baseWaitTime * globalPatienceMultiplier * difficulty;
    }

    /// <summary>
    /// Obtenir le prix final d'une boisson
    /// </summary>
    public int GetFinalPrice(int basePrice)
    {
        return Mathf.RoundToInt(basePrice * priceMultiplier);
    }

    /// <summary>
    /// Calculer le pourboire selon le temps de service
    /// </summary>
    public int CalculateTip(int basePrice, float serviceTime, float maxWaitTime)
    {
        // Si servi dans les 30% du temps max = pourboire
        if (serviceTime < maxWaitTime * 0.3f)
        {
            return Mathf.RoundToInt(basePrice * (quickServiceBonus / 100f));
        }
        return 0;
    }

    /// <summary>
    /// Validation des valeurs dans l'éditeur
    /// </summary>
    void OnValidate()
    {
        // S'assurer que les valeurs sont cohérentes
        if (minSpawnInterval > maxSpawnInterval)
            maxSpawnInterval = minSpawnInterval;
        
        if (startingMoney < 0)
            startingMoney = 0;
        
        if (dailyGoal < startingMoney)
            dailyGoal = startingMoney + 50;
    }
}