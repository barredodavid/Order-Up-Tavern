using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.UI;

public class CustomerAI : MonoBehaviour
{
    [Header("États")]
    public CustomerState currentState = CustomerState.WaitingForCleaning;

    [Header("Destinations")]
    public Transform barPosition;
    public Transform spawnPosition;
    public float facingAngleOffset = 0f;

    [Header("Commande")]
    public string drinkOrder = "Bière";
    public int drinkPrice = 5;

    [Header("Patience & UI")]
    public Image patienceFill;
    
    public float patienceTime = 20.0f; 
    // On garde maxWaitTime comme variable interne pour la logique
    private float maxWaitTime; 

    [Header("Animation")]
    public Animator animator;

    [Header("Bulle de dialogue")]
    public TextMeshProUGUI bubbleText;
    public GameObject bubbleCanvas;

    [Header("Visuel")]
    public GameObject handGlassVisual;
    public GameObject dirtyGlassPrefab;

    private NavMeshAgent agent;
    private float waitTimer = 0f;
    private bool hasOrdered = false;
    private bool hasReceivedDrink = false;
    private int mySeatIndex = -1; 
    private CustomerSpawner mySpawner;
    private bool isDrinkingAnimationStarted = false;

    public enum CustomerState
    {
        WaitingForCleaning, Walking, Waiting, Drinking, Leaving
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) { Destroy(gameObject); return; }

        if (spawnPosition == null)
        {
            GameObject spawnPoint = new GameObject(gameObject.name + "_SpawnPoint");
            spawnPoint.transform.position = transform.position;
            spawnPoint.transform.rotation = transform.rotation;
            spawnPosition = spawnPoint.transform;
        }

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (patienceFill != null) patienceFill.transform.parent.gameObject.SetActive(false);

        // On prend la valeur de base
        maxWaitTime = patienceTime;
        
        // Si le GameManager a un multiplicateur de patience (tu pourras l'ajouter plus tard)
        if (GameManager.Instance != null)
        {
            maxWaitTime = patienceTime * GameManager.Instance.patienceMultiplier;
        }
        // ---------------------------------------------

        currentState = CustomerState.WaitingForCleaning;
        agent.isStopped = true;
    }

    void Update()
    {
        if (agent == null) return;

        // Animations
        if (animator != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsWalking", speed > 0.1f);
        }

        switch (currentState)
        {
            case CustomerState.WaitingForCleaning: HandleWaitingForCleaning(); break;
            case CustomerState.Walking: HandleWalking(); break;
            case CustomerState.Waiting: HandleWaiting(); break;
            case CustomerState.Drinking: HandleDrinking(); break;
            case CustomerState.Leaving: HandleLeaving(); break;
        }
    }

    void HandleWaitingForCleaning()
    {
        if (CleaningManager.Instance == null || !CleaningManager.Instance.IsTavernClean()) return;
        if (barPosition != null)
        {
            agent.isStopped = false;
            agent.SetDestination(barPosition.position);
            currentState = CustomerState.Walking;
        }
    }

    public void AssignSeat(Transform seatTarget, int index, CustomerSpawner spawner)
    {
        if (seatTarget == null) return;
        barPosition = seatTarget; 
        mySeatIndex = index;
        mySpawner = spawner;
        currentState = CustomerState.Walking; 
        if (agent != null) { agent.isStopped = false; agent.SetDestination(barPosition.position); }
    }

    void HandleWalking()
    {
        if (patienceFill != null) patienceFill.transform.parent.gameObject.SetActive(false);
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                currentState = CustomerState.Waiting;
                waitTimer = 0f;
                if (barPosition != null) transform.rotation = barPosition.rotation;
                if (patienceFill != null) patienceFill.transform.parent.gameObject.SetActive(true);
            }
        }
    }

    void HandleWaiting()
    {
        if (!hasOrdered) hasOrdered = true;

        if (barPosition != null)
        {
            Vector3 dir = barPosition.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, facingAngleOffset, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
            }
        }

        // Mode Tuto : Patience infinie
        bool isTutorial = GameManager.Instance != null && GameManager.Instance.currentState == GameState.TutorialServing;

        if (!isTutorial) waitTimer += Time.deltaTime;

        if (patienceFill != null)
        {
            // On utilise maxWaitTime ici
            float ratio = 1 - (waitTimer / maxWaitTime);
            patienceFill.fillAmount = Mathf.Clamp01(ratio);
            patienceFill.color = Color.Lerp(Color.red, Color.green, ratio);
        }

        if (!isTutorial && waitTimer >= maxWaitTime)
        {
            if (DayManager.Instance != null) DayManager.Instance.RegisterAngryClient();
            if (patienceFill != null) patienceFill.transform.parent.gameObject.SetActive(false);
            currentState = CustomerState.Leaving;
            if (TavernUIManager.Instance != null) TavernUIManager.Instance.HideCustomerOrder();
        }
    }

    void HandleDrinking()
    {
        if (patienceFill != null) patienceFill.transform.parent.gameObject.SetActive(false);
        
        if (!isDrinkingAnimationStarted)
        {
            isDrinkingAnimationStarted = true;
            if (animator != null) animator.SetBool("IsDrinking", true);
            if (bubbleText != null) bubbleText.text = "Mmmh ! 😊";
            if (bubbleCanvas != null) bubbleCanvas.SetActive(true);
        }
        
        waitTimer += Time.deltaTime;

        if (waitTimer >= 4f) 
        {
            if (animator != null) animator.SetBool("IsDrinking", false);
            if (handGlassVisual != null) handGlassVisual.SetActive(false);
            
            if (dirtyGlassPrefab != null)
            {
                Vector3 finalPos = transform.position + transform.forward * 0.6f + Vector3.up * 0.8f; 
                Quaternion finalRot = Quaternion.identity;
                if (barPosition != null)
                {
                    Transform spot = barPosition.Find("DirtyGlassSpot");
                    if (spot != null) { finalPos = spot.position; finalRot = spot.rotation; }
                }
                Instantiate(dirtyGlassPrefab, finalPos, finalRot);

                if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.TutorialServing)
                {
                    TutorialManager tuto = FindAnyObjectByType<TutorialManager>();
                    if (tuto != null) { tuto.ShowDirtyGlassTutorial(); GameManager.Instance.ChangeState(GameState.TutorialCleaning); }
                }
            }
            currentState = CustomerState.Leaving;
        }
    }

    void HandleLeaving()
    {
        if (agent == null || !agent.enabled || spawnPosition == null) { Destroy(gameObject); return; }

        if (!agent.hasPath)
        {
            agent.isStopped = false;
            if (!agent.SetDestination(spawnPosition.position)) { Destroy(gameObject, 2f); return; }
        }

        if (Vector3.Distance(transform.position, spawnPosition.position) <= agent.stoppingDistance + 0.5f)
        {
            Destroy(gameObject);
        }
    }

    public void ReceiveDrink()
    {
        if (currentState != CustomerState.Waiting || hasReceivedDrink) return;
        hasReceivedDrink = true;
        if (patienceFill != null) patienceFill.transform.parent.gameObject.SetActive(false);

        // PRIX DYNAMIQUE
        if (GameManager.Instance != null)
        {
            int finalPrice = GameManager.Instance.GetBeerPrice();
            GameManager.Instance.AddMoney(finalPrice);
        }
        
        if (TavernUIManager.Instance != null) TavernUIManager.Instance.HideCustomerOrder();
        if (handGlassVisual != null) handGlassVisual.SetActive(true);

        currentState = CustomerState.Drinking;
        waitTimer = 0f;
        isDrinkingAnimationStarted = false;

        if (DayManager.Instance != null) DayManager.Instance.RegisterServedClient();
    }

    public string GetOrder() { return drinkOrder; }
    public bool IsWaiting() { return currentState == CustomerState.Waiting && !hasReceivedDrink; }

    // Ajoute cette variable statique pour savoir si on quitte le jeu
    private static bool isQuitting = false;

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    void OnDestroy()
    {
        // 🛑 SÉCURITÉ : Si on quitte le jeu, on ne fait RIEN.
        // On laisse le siège "pris" car tout va être détruit de toute façon.
        if (isQuitting) return;

        if (mySpawner != null && mySeatIndex != -1)
        {
            mySpawner.FreeSeat(mySeatIndex);
        }
    }
}