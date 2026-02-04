using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CleaningManager : MonoBehaviour
{
    public static CleaningManager Instance { get; private set; }

    [Header("UI Interface")]
    public Image cleanlinessBarFill; 

    [Header("Réglages")]
    public int maxDirtCapacity = 10; 

    private List<DirtSpot> activeDirtSpots = new List<DirtSpot>();

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        UpdateUI();
    }

    public void RegisterDirt(DirtSpot spot)
    {
        if (spot != null && !activeDirtSpots.Contains(spot))
        {
            activeDirtSpots.Add(spot);
            UpdateUI();
        }
    }

    public void RemoveDirt(DirtSpot spot)
    {
        if (spot != null && activeDirtSpots.Contains(spot))
        {
            activeDirtSpots.Remove(spot);
            UpdateUI();

            // Si tout est propre (0 tache) ET qu'on est dans le tuto
            if (activeDirtSpots.Count == 0)
            {
                if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.TutorialCleaning)
                {
                    Debug.Log("✨ Tout est propre ! Lancement de la suite...");
                    
                    // 1. On change l'état du jeu pour faire venir le client
                    GameManager.Instance.ChangeState(GameState.TutorialServing);

                    // 2. ON LANCE LE DIALOGUE
                    TutorialManager tuto = FindAnyObjectByType<TutorialManager>();
                    if (tuto != null)
                    {
                        tuto.ShowFirstCustomerTutorial();
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ Impossible de trouver le TutorialManager pour lancer le dialogue !");
                    }
                }
            }
        }
    }
    
    // Fonction de secours
    public void RemoveDirt()
    {
       if(activeDirtSpots.Count > 0)
       {
           activeDirtSpots.RemoveAt(0); 
           UpdateUI();
       }
    }

    void UpdateUI()
    {
        if (cleanlinessBarFill != null)
        {
            int currentDirtCount = activeDirtSpots.Count;
            float cleanlinessRatio = 1.0f - ((float)currentDirtCount / maxDirtCapacity);
            cleanlinessRatio = Mathf.Clamp01(cleanlinessRatio);

            cleanlinessBarFill.fillAmount = cleanlinessRatio;
            cleanlinessBarFill.color = Color.Lerp(Color.red, Color.green, cleanlinessRatio);
        }
    }

    public bool IsTavernClean()
    {
        return activeDirtSpots.Count < (maxDirtCapacity / 2);
    }
    
    public float GetCleanlinessRatio()
    {
         if (cleanlinessBarFill != null) return cleanlinessBarFill.fillAmount;
         return 1.0f - ((float)activeDirtSpots.Count / maxDirtCapacity);
    }
}