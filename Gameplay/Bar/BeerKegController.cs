using UnityEngine;
using System.Collections;

public class BeerKegController : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Type de boisson dans ce fût")]
    public string drinkType = "Bière";
    
    [Tooltip("Son de remplissage")]
    public AudioClip fillSound;
    
    [Tooltip("Particules de bière qui coule")]
    public ParticleSystem beerParticles;
    
    [Tooltip("Stream visuel animé")]
    public BeerStreamAnimator beerStream;
    
    [Tooltip("Durée de remplissage (secondes)")]
    public float fillDuration = 2f;
    
    [Header("Positionnement du joueur")]
    [Tooltip("Position où le joueur sera téléporté (Crée un Empty devant le fût)")]
    public Transform playerPourPosition;
    
    private AudioSource audioSource;
    private bool isFilling = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && fillSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }
    }

    public void FillGlass(GlassController glass)
    {
        if (glass == null || glass.isFilled || isFilling) return;

        // Trouve le joueur
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            SimpleFPSController ctrl = FindAnyObjectByType<SimpleFPSController>();
            if (ctrl != null) player = ctrl.gameObject;
        }

        if (player != null)
        {
            StartCoroutine(FillGlassCoroutine(glass, player));
        }
    }

    System.Collections.IEnumerator FillGlassCoroutine(GlassController glass, GameObject player)
    {
        isFilling = true;
        SimpleFPSController playerController = player.GetComponent<SimpleFPSController>();

        // 1. DÉMARRAGE EFFETS
        if (beerParticles != null) beerParticles.Play();
        if (beerStream != null) beerStream.StartPour();
        if (audioSource != null && fillSound != null) audioSource.PlayOneShot(fillSound);

        // --- CALCUL DE LA VITESSE ---
        float actualDuration = fillDuration;
        
        if (GameManager.Instance != null)
        {
            // On récupère le multiplicateur (Ex: 1.0, 1.5, 2.0...)
            float multiplier = GameManager.Instance.beerFillSpeedMultiplier;
            
            // Sécurité pour éviter la division par zéro
            if (multiplier <= 0) multiplier = 1f;

            // Plus le multiplicateur est grand, plus le temps est court
            actualDuration = fillDuration / multiplier;
        }
        // ----------------------------------------------------

        // 2. BOUCLE DE REMPLISSAGE 
        float timer = 0f;
        while (timer < actualDuration)
        {
            timer += Time.deltaTime;

            // SI ON A UNE POSITION DE SERVICE ASSIGNÉE
            if (playerPourPosition != null)
            {
                // A. On force la position du corps
                player.transform.position = playerPourPosition.position;
                player.transform.rotation = playerPourPosition.rotation;

                // B. On force la TÊTE (Caméra) à regarder le fût
                if (playerController != null && playerController.cameraPivot != null)
                {
                    playerController.cameraPivot.localRotation = Quaternion.Euler(25f, 0f, 0f);
                }
            }

            // On empêche aussi le mouvement clavier
            if (playerController != null) playerController.CanMove = false;

            yield return null; // On attend la frame suivante
        }

        // 3. FIN DU REMPLISSAGE
        if (glass != null) glass.Fill(drinkType);

        // Arrêt des effets
        if (beerParticles != null) beerParticles.Stop();
        if (beerStream != null) beerStream.StopPour();

        // 4. DÉBLOCAGE TOTAL
        if (playerController != null)
        {
            playerController.CanMove = true;
        }

        isFilling = false;

        // 5. EVENTS & TUTO
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.TutorialServing)
        {
            PlayerInteraction pi = player.GetComponentInChildren<PlayerInteraction>();
            if (pi == null) pi = player.GetComponent<PlayerInteraction>();
            if (pi != null) pi.OnBeerFilled();
        }
    }
}