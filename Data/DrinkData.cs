using UnityEngine;

/// <summary>
/// ScriptableObject pour définir les boissons
/// Créer via : Right Click > Create > Tavern/Drink
/// </summary>
[CreateAssetMenu(fileName = "NewDrink", menuName = "Tavern/Drink", order = 1)]
public class DrinkData : ScriptableObject
{
    [Header("🍺 Informations")]
    [Tooltip("Nom de la boisson")]
    public string drinkName = "Bière";
    
    [Tooltip("Description pour les dialogues")]
    [TextArea(2, 4)]
    public string description = "Une bière blonde rafraîchissante.";
    
    [Header("💰 Économie")]
    [Tooltip("Prix de vente")]
    public int price = 5;
    
    [Tooltip("Coût de production (pour des upgrades futures)")]
    public int cost = 2;
    
    [Header("🎨 Visuel")]
    [Tooltip("Icône pour l'UI")]
    public Sprite icon;
    
    [Tooltip("Couleur du liquide dans le verre")]
    public Color liquidColor = new Color(1f, 0.8f, 0.2f); // Jaune bière
    
    [Tooltip("Prefab du liquide (optionnel, si tu veux des modèles 3D)")]
    public GameObject liquidPrefab;
    
    [Header("⚙️ Gameplay")]
    [Tooltip("Temps de remplissage au tonneau")]
    public float fillDuration = 2.0f;
    
    [Tooltip("Est-ce une boisson alcoolisée ? (pour des effets futurs)")]
    public bool isAlcoholic = true;
    
    [Tooltip("Niveau de difficulté de préparation (1-5)")]
    [Range(1, 5)]
    public int difficultyLevel = 1;
    
    [Header("🎵 Audio")]
    [Tooltip("Son de remplissage")]
    public AudioClip fillSound;
    
    [Tooltip("Son quand le client boit")]
    public AudioClip drinkSound;

    /// <summary>
    /// Obtenir le prix avec d'éventuels modificateurs
    /// </summary>
    public int GetFinalPrice(float priceModifier = 1f)
    {
        return Mathf.RoundToInt(price * priceModifier);
    }

    /// <summary>
    /// Obtenir la description formatée pour l'UI
    /// </summary>
    public string GetFormattedDescription()
    {
        return $"<b>{drinkName}</b>\n{description}\n<color=yellow>{price} Or</color>";
    }
}