using UnityEngine;

public class GlassController : MonoBehaviour
{
    [Header("État")]
    [Tooltip("Le verre est-il rempli de bière ?")]
    public bool isFilled = false;
    
    [Tooltip("Type de boisson dans le verre")]
    public string drinkType = "Bière";
    
    [Tooltip("Le verre est-il équipé par le joueur ?")]
    public bool isEquipped = false;

    [Header("Visuel")]
    [Tooltip("Objet représentant le liquide (à activer quand rempli)")]
    public GameObject liquidVisual;

    private Rigidbody cachedRigidbody;
    private Collider cachedCollider;

    void Awake()
    {
        // Cache les composants pour éviter les GetComponent répétés
        cachedRigidbody = GetComponent<Rigidbody>();
        cachedCollider = GetComponent<Collider>();
    }

    void Start()
    {
        // Masquer le liquide au début si pas rempli
        if (liquidVisual != null)
        {
            liquidVisual.SetActive(isFilled);
        }
    }

    /// <summary>
    /// Équiper le verre dans la main du joueur
    /// </summary>
    public void Equip(Transform holder)
    {
        if (holder == null)
        {
            Debug.LogWarning("⚠️ Tentative d'équiper le verre avec un holder null");
            return;
        }

        isEquipped = true;

        // Attacher à la main
        transform.SetParent(holder);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Désactiver physique
        if (cachedRigidbody != null)
        {
            cachedRigidbody.isKinematic = true;
            cachedRigidbody.useGravity = false;
        }

        if (cachedCollider != null)
        {
            cachedCollider.enabled = false;
        }
    }

    /// <summary>
    /// Déséquiper le verre (poser)
    /// </summary>
    public void Unequip()
    {
        isEquipped = false;

        // Détacher de la main
        transform.SetParent(null);

        // Réactiver physique
        if (cachedRigidbody != null)
        {
            cachedRigidbody.isKinematic = false;
            cachedRigidbody.useGravity = true;
        }

        if (cachedCollider != null)
        {
            cachedCollider.enabled = true;
        }
    }

    /// <summary>
    /// Remplir le verre
    /// </summary>
    public void Fill(string drink = "Bière")
    {
        isFilled = true;
        drinkType = drink;

        // Afficher le liquide
        if (liquidVisual != null)
        {
            liquidVisual.SetActive(true);
        }

        Debug.Log($"🍺 Verre rempli de {drinkType}");
    }

    /// <summary>
    /// Vider le verre
    /// </summary>
    public void Empty()
    {
        isFilled = false;
        drinkType = "";

        // Masquer le liquide
        if (liquidVisual != null)
        {
            liquidVisual.SetActive(false);
        }

        Debug.Log("🥤 Verre vidé");
    }
}