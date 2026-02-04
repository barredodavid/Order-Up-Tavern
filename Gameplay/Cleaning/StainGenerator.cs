using UnityEngine;

public class StainGenerator : MonoBehaviour
{
    [Header("Réglages")]
    public GameObject stainPrefab;
    [Tooltip("Temps entre chaque tache (Mets 15 ici dans l'inspecteur !)")]
    public float spawnInterval = 15f; 
    public Vector3 spawnAreaSize = new Vector3(4, 1f, 4);

    private float timer;

    void Start()
    {
        timer = spawnInterval;
    }

    void Update()
    {
        // On vérifie juste qu'on est bien en Gameplay
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameState.Gameplay) return;

        timer -= Time.deltaTime;
        
        if (timer <= 0)
        {
            ForceSpawnStain();
            timer = spawnInterval;
        }
    }

    void ForceSpawnStain()
    {
        if (stainPrefab == null) return;

        float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
        float randomZ = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);

        // On force la hauteur juste au-dessus du sol (0.02f)
        Vector3 finalPos = new Vector3(
            transform.position.x + randomX, 
            0.02f, 
            transform.position.z + randomZ
        );

        Instantiate(stainPrefab, finalPos, Quaternion.identity, transform);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}