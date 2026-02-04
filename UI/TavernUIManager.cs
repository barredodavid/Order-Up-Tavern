using UnityEngine;
using TMPro;

public class TavernUIManager : MonoBehaviour
{
    public static TavernUIManager Instance { get; private set; }

    [Header("UI Commande Client")]
    [Tooltip("Texte qui affiche ce que le client veut")]
    public TMP_Text customerOrderText;

    private CustomerAI currentCustomer;

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
        HideCustomerOrder();
    }

    /// <summary>
    /// Affiche la commande d'un client
    /// </summary>
    public void ShowCustomerOrder(CustomerAI customer, string orderText)
    {
        if (customer == null)
        {
            Debug.LogWarning("⚠️ Tentative d'afficher la commande d'un client null");
            return;
        }

        currentCustomer = customer;

        if (customerOrderText != null)
        {
            customerOrderText.text = orderText;
            customerOrderText.enabled = true;
        }
        else
        {
            Debug.LogWarning("⚠️ customerOrderText n'est pas assigné dans TavernUIManager");
        }
    }

    /// <summary>
    /// Cache la commande
    /// </summary>
    public void HideCustomerOrder()
    {
        currentCustomer = null;

        if (customerOrderText != null)
        {
            customerOrderText.enabled = false;
            customerOrderText.text = "";
        }
    }

    /// <summary>
    /// Récupère le client qui attend actuellement
    /// </summary>
    public CustomerAI GetWaitingCustomer()
    {
        return currentCustomer;
    }

    /// <summary>
    /// Vérifie si un client est actuellement affiché
    /// </summary>
    public bool HasActiveOrder()
    {
        return currentCustomer != null;
    }
}