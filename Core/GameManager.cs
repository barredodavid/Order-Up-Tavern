using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem; 

public enum GameState
{
    TutorialCleaning,   
    TutorialServing,    
    Preparation,
    Interlude,          
    Gameplay            
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("État du Jeu")]
    public GameState currentState;
    public bool canOpenTavern = false; 

    [Header("UI Tuto")]
    public GameObject dayStartPanel; 

    [HideInInspector] 
    public bool canStartDayOne = false; 

    [Header("Argent")]
    public int currentMoney = 0;
    public TMP_Text moneyText;
    public GameObject floatingTextPrefab;
    public AudioClip coinsSound;
    private AudioSource audioSource;
    private CustomerSpawner customerSpawner;

    [Header("Économie & Stats")]
    public int beerPrice = 5;           
    public float patienceMultiplier = 1.0f; 

    [Header("--- AMÉLIORATIONS (RPG) ---")]
    // 1. Bière plus vite
    public float beerFillSpeedMultiplier = 1.0f; // 1.0 = Vitesse normale, 2.0 = 2x plus vite

    // 2. Bière meilleure (Prix de vente)
    public int beerQualityPriceBonus = 0; // Ajoute +X or par bière vendue

    // 3. Laver plus vite
    public float washSpeedMultiplier = 1.0f; // 1.0 = Normal

    // 4. Balayer plus vite (Force du coup)
    public int broomPower = 1; // 1 = 1 PV en moins, 3 = One shot

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) 
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        UpdateMoneyUI();
        customerSpawner = FindAnyObjectByType<CustomerSpawner>();
        
        if (dayStartPanel != null) dayStartPanel.SetActive(false);
        if (UIMessageManager.Instance != null) UIMessageManager.Instance.HideInteractionMessage();

        ChangeState(GameState.TutorialCleaning);
    }

    void Update()
    {
        // QUAND ON APPUIE SUR F
        if (currentState == GameState.Preparation && canOpenTavern)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.fKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    // Appel au DayManager pour lancer la transition
                    if (DayManager.Instance != null)
                    {
                        DayManager.Instance.StartDaySequence();
                        canOpenTavern = false; 
                    }
                    else
                    {
                        Debug.LogError("❌ DayManager introuvable !");
                        StartDayOne(); // Secours
                    }
                }
            }
        }
        
        // --- CHEAT CODE ARGENT (Touche M) ---
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            Debug.Log("🤑 CHEAT CODE ACTIVÉ : +500 Or !");
            AddMoney(500);
        }
    }

    // --- GESTION DU TUTO ---
    public void UnlockTutorialEnd()
    {
        TutorialManager tuto = FindAnyObjectByType<TutorialManager>();
        if (tuto != null) tuto.ShowEndTutorialDialogue();
        StartCoroutine(WaitAndOpenShop());
    }

    IEnumerator WaitAndOpenShop()
    {
        yield return new WaitForSeconds(0.5f); 
        ChangeState(GameState.Preparation);
    }
    
    public void OnShopClosed()
    {
        if (currentState == GameState.Preparation && !canOpenTavern)
        {
            TutorialManager tuto = FindAnyObjectByType<TutorialManager>();
            if (tuto != null) tuto.ShowInspectorTutorial();
            canOpenTavern = true;
        }
    }

    // Appelée par DayManager à la fin de la transition
    public void StartDayOne()
    {
        ChangeState(GameState.Gameplay);
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"🔄 CHANGEMENT D'ÉTAT : {newState}");

        switch (currentState)
        {
            case GameState.TutorialCleaning:
                 if (UIMessageManager.Instance != null) UIMessageManager.Instance.ShowInteractionMessage("Utilisez le balai !");
                 break;
            case GameState.TutorialServing:
                 if (customerSpawner != null) customerSpawner.SpawnTutorialCustomer();
                 break;
            case GameState.Preparation:
                TutorialManager tuto = FindAnyObjectByType<TutorialManager>();
                if (tuto != null) tuto.ShowShopTutorial();
                if (UIMessageManager.Instance != null) UIMessageManager.Instance.ShowInteractionMessage("Consultez le Livre de Comptes");
                break;
            case GameState.Gameplay:
                Time.timeScale = 1f; 
                if (dayStartPanel != null) dayStartPanel.SetActive(false);
                if (UIMessageManager.Instance != null) UIMessageManager.Instance.HideInteractionMessage();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (customerSpawner != null) customerSpawner.StartService();
                break; 
            case GameState.Interlude:
                 break;
        }
    }

    /// <summary>
    /// Calcule le prix de vente d'une bière en fonction des améliorations
    /// </summary>
    public int GetBeerPrice()
    {
        return beerPrice + beerQualityPriceBonus;
    }

    // --- ARGENT ---
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        
        // 1. LE SON
        if (audioSource != null && coinsSound != null) 
        {
            audioSource.PlayOneShot(coinsSound);
        }

        UpdateMoneyUI();
        StartCoroutine(MoneyPopAnimation());
        ShowFloatingMoney(amount);
    }

    public void RemoveMoney(int amount) 
    { 
        currentMoney -= amount; 
        if(currentMoney < 0) currentMoney = 0; 
        UpdateMoneyUI(); 
    }

    void UpdateMoneyUI() 
    { 
        if (moneyText != null) moneyText.text = $"{currentMoney} Or"; 
    }

    IEnumerator MoneyPopAnimation() 
    { 
        if (moneyText == null) yield break;
        Vector3 originalScale = moneyText.transform.localScale;
        float duration = 0.15f;
        float elapsed = 0f;
        
        // Gros
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            moneyText.transform.localScale = originalScale * Mathf.Lerp(1f, 1.3f, t);
            yield return null;
        }
        
        // Normal
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            moneyText.transform.localScale = originalScale * Mathf.Lerp(1.3f, 1f, t);
            yield return null;
        }
        moneyText.transform.localScale = originalScale;
    }

    void ShowFloatingMoney(int amount) 
    { 
        // 2. LE TEXTE FLOTTANT
        if (floatingTextPrefab == null) 
        {
            CreateFloatingTextDynamically(amount);
        }
        else 
        { 
            if (moneyText == null) return;
            GameObject floatingObj = Instantiate(floatingTextPrefab, moneyText.transform.position, Quaternion.identity, moneyText.transform.parent); 
            TMP_Text floatingText = floatingObj.GetComponent<TMP_Text>(); 
            if (floatingText != null) floatingText.text = $"+{amount}";
            StartCoroutine(AnimateFloatingText(floatingObj)); 
        } 
    }

    void CreateFloatingTextDynamically(int amount) 
    { 
        if (moneyText == null) return; 
        
        GameObject floatingObj = new GameObject("FloatingMoney"); 
        floatingObj.transform.SetParent(moneyText.transform.parent, false); 
        
        TMP_Text floatingText = floatingObj.AddComponent<TextMeshProUGUI>(); 
        floatingText.text = $"+{amount}"; 
        floatingText.fontSize = 36; 
        floatingText.color = Color.yellow; 
        floatingText.fontStyle = FontStyles.Bold;
        
        // Ombre pour la lisibilité
        var outline = floatingObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        // Positionnement
        RectTransform rect = floatingObj.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, 50); // Juste au dessus de l'argent

        StartCoroutine(AnimateFloatingText(floatingObj)); 
    }

    IEnumerator AnimateFloatingText(GameObject obj) 
    { 
        float duration = 1.0f;
        float elapsed = 0f;
        Vector3 startPos = obj.transform.position;
        TMP_Text text = obj.GetComponent<TMP_Text>();

        while (elapsed < duration)
        {
            if (obj == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Monte vers le haut
            obj.transform.position = startPos + Vector3.up * (t * 50f); // Monte de 50 pixels
            
            // Disparait en fondu
            if (text != null) text.alpha = 1f - t;
            
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }
}