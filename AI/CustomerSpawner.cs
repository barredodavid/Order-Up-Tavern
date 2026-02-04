using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Configuration Prefabs")]
    public GameObject[] customerPrefabs; 
    
    [Header("Configuration Spawn")]
    public Transform spawnPoint;
    
    // Ces valeurs seront écrasées par le DayManager au lancement de la journée
    public float timeBetweenSpawns = 5f; 

    [Header("Les Places (Slots)")]
    public Transform[] customerSlots;
    
    private bool[] isSeatTaken; 
    
    // --- NOUVELLES VARIABLES POUR LA GESTION PAR JOUR ---
    private int clientsLeftToSpawn = 0; // Le stock du jour
    private bool isSpawningActive = false;
    private bool hasSpawnedTutorialCustomer = false;

    void Awake()
    {
        hasSpawnedTutorialCustomer = false;
    }

    void Start()
    {
        if (customerSlots == null || customerSlots.Length == 0)
        {
            Debug.LogError("❌ Aucun slot de client assigné dans CustomerSpawner !");
            enabled = false;
            return;
        }

        isSeatTaken = new bool[customerSlots.Length];
        isSpawningActive = false;
    }

    // --- CETTE FONCTION EST APPELÉE PAR LE DAY MANAGER AU DÉBUT DU JOUR ---
    public void StartDailyWave(int quantity, float speed)
    {
        clientsLeftToSpawn = quantity;
        timeBetweenSpawns = speed;
        isSpawningActive = true;
        
        Debug.Log($"🌊 VAGUE LANCEE : {quantity} clients, vitesse {speed}s");
        
        // On lance la routine de spawn
        StopAllCoroutines(); // Sécurité pour ne pas avoir 2 vagues en même temps
        StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        isSpawningActive = false;
        StopAllCoroutines();
    }

    // La boucle principale qui fait arriver les clients un par un
    IEnumerator SpawnRoutine()
    {
        // Petite pause au démarrage pour ne pas spawn pendant le fondu noir
        yield return new WaitForSeconds(0.5f);

        while (isSpawningActive && clientsLeftToSpawn > 0)
        {
            // On essaie de faire entrer un client
            bool success = TrySpawnCustomer("Wave");
            
            if (success)
            {
                clientsLeftToSpawn--; // On déduit du stock SEULEMENT si le spawn a réussi
            }
            
            // On attend avant le prochain (même si échec, pour ne pas spammer)
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
        
        Debug.Log("🏁 Tous les clients du jour sont envoyés !");
    }

    // --- LE TUTORIEL (Reste inchangé) ---
    public void SpawnTutorialCustomer()
    {
        if (hasSpawnedTutorialCustomer) return;
        Debug.Log("🎓 TUTO : Création du client test UNIQUE !");
        hasSpawnedTutorialCustomer = true;
        TrySpawnCustomer("Tutoriel");
    }

    // --- LA LOGIQUE DE SPAWN (Adaptée pour renvoyer true/false) ---
    bool TrySpawnCustomer(string source)
    {
        // Vérifications de base
        if (customerPrefabs == null || customerPrefabs.Length == 0) return false;
        if (spawnPoint == null) return false;

        // 1. Trouver un slot libre
        int freeSlotIndex = -1;
        for (int i = 0; i < isSeatTaken.Length; i++)
        {
            if (isSeatTaken[i] == false)
            {
                freeSlotIndex = i;
                break; 
            }
        }

        // Si pas de place, on annule (le client attendra le prochain cycle)
        if (freeSlotIndex == -1)
        {
            // Optionnel : Debug.Log("⏳ Pas de place, le client attend...");
            return false;
        }

        // 2. Choix Aléatoire du personnage
        int randomIndex = Random.Range(0, customerPrefabs.Length);
        GameObject selectedPrefab = customerPrefabs[randomIndex];

        // 3. Instanciation
        GameObject newClient = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
        
        if (newClient == null) return false;

        // 4. Configuration
        newClient.tag = "Customer"; 
        foreach (Transform child in newClient.transform) child.tag = "Customer";

        CustomerAI customerScript = newClient.GetComponent<CustomerAI>();
        if (customerScript != null)
        {
            customerScript.AssignSeat(customerSlots[freeSlotIndex], freeSlotIndex, this);
        }

        // 5. Verrouillage du siège
        isSeatTaken[freeSlotIndex] = true;
        
        return true; // SUCCÈS
    }

    public void FreeSeat(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < isSeatTaken.Length)
        {
            isSeatTaken[slotIndex] = false;
        }
    }

    // Ancienne méthode StartService (Gardée pour compatibilité mais vide)
    public void StartService() 
    {
        // Ne fait rien maintenant, c'est StartDailyWave qui gère tout
    }
}