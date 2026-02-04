using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("UI Interface")]
    public GameObject shopPanel;      
    public TextMeshProUGUI moneyText; 

    [Header("Les Boutons")]
    public Button tapButton;    
    public Button yeastButton;  
    public Button washButton;   
    public Button broomButton;  

    [Header("Les Textes des Boutons")]
    public TextMeshProUGUI tapText;   
    public TextMeshProUGUI yeastText;  
    public TextMeshProUGUI washText;  
    public TextMeshProUGUI broomText; 

    [Header("Prix")]
    public int tapPrice = 100;
    public int yeastPrice = 200;
    public int washPrice = 150;
    public int broomPrice = 100;

    // État des achats
    private bool hasTapUpgrade = false;
    private bool hasYeastUpgrade = false;
    private bool hasWashUpgrade = false;
    private bool hasBroomUpgrade = false;

    void Awake()
    {
        Instance = this;
        if (shopPanel) shopPanel.SetActive(false);
    }

    void Update()
    {     
        if (shopPanel.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseShop();
        }
    }

    public void OpenShop()
    {
        if (shopPanel)
        {
            shopPanel.SetActive(true);
            UpdateUI(); // On met à jour l'affichage dès l'ouverture
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SimpleFPSController player = FindAnyObjectByType<SimpleFPSController>();
            if (player != null) player.enabled = false;
        }
    }

    public void CloseShop()
    {
        if (shopPanel)
        {
            shopPanel.SetActive(false);
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            SimpleFPSController player = FindAnyObjectByType<SimpleFPSController>();
            if (player != null) player.enabled = true; 

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnShopClosed();
            }
        }
    }

    void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        int money = GameManager.Instance.currentMoney;
        if (moneyText) moneyText.text = "Bourse : " + money + " Or";
        
        // --- BOUTON 1 : ROBINET ---
        if (hasTapUpgrade)
        {
            if (tapButton) tapButton.interactable = false; // On ne peut plus cliquer
            if (tapText) tapText.text = "ACHETÉ";          // On change le texte
        }
        else
        {
            // On peut acheter seulement si on a assez d'argent
            if (tapButton) tapButton.interactable = (money >= tapPrice);
            if (tapText) tapText.text = $"({tapPrice} Or)";
        }

        // --- BOUTON 2 : LEVURE (PRIX) ---
        if (hasYeastUpgrade)
        {
            if (yeastButton) yeastButton.interactable = false;
            if (yeastText) yeastText.text = "ACHETÉ";
        }
        else
        {
            if (yeastButton) yeastButton.interactable = (money >= yeastPrice);
            if (yeastText) yeastText.text = $"({yeastPrice} Or)";
        }

        // --- BOUTON 3 : LAVAGE ---
        if (hasWashUpgrade)
        {
            if (washButton) washButton.interactable = false;
            if (washText) washText.text = "ACHETÉ";
        }
        else
        {
            if (washButton) washButton.interactable = (money >= washPrice);
            if (washText) washText.text = $"({washPrice} Or)";
        }

        // --- BOUTON 4 : BALAI ---
        if (hasBroomUpgrade)
        {
            if (broomButton) broomButton.interactable = false;
            if (broomText) broomText.text = "ACHETÉ";
        }
        else
        {
            if (broomButton) broomButton.interactable = (money >= broomPrice);
            if (broomText) broomText.text = $"({broomPrice} Or)";
        }
    }

    // --- FONCTIONS D'ACHAT ---
    public void BuyFastTap()
    {
        if (GameManager.Instance.currentMoney >= tapPrice && !hasTapUpgrade)
        {
            GameManager.Instance.RemoveMoney(tapPrice);
            hasTapUpgrade = true;
            GameManager.Instance.beerFillSpeedMultiplier = 2.0f; 
            UpdateUI();
        }
    }

    public void BuyQualityBeer()
    {
        if (GameManager.Instance.currentMoney >= yeastPrice && !hasYeastUpgrade)
        {
            GameManager.Instance.RemoveMoney(yeastPrice);
            hasYeastUpgrade = true;
            GameManager.Instance.beerQualityPriceBonus = 5; 
            UpdateUI();
        }
    }

    public void BuyFastWash()
    {
        if (GameManager.Instance.currentMoney >= washPrice && !hasWashUpgrade)
        {
            GameManager.Instance.RemoveMoney(washPrice);
            hasWashUpgrade = true;
            GameManager.Instance.washSpeedMultiplier = 3.0f; 
            UpdateUI();
        }
    }

    public void BuyStrongBroom()
    {
        if (GameManager.Instance.currentMoney >= broomPrice && !hasBroomUpgrade)
        {
            GameManager.Instance.RemoveMoney(broomPrice);
            hasBroomUpgrade = true;
            GameManager.Instance.broomPower = 5;
            UpdateUI();
        }
    }
}