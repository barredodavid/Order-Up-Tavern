using UnityEngine;

public class DirtSpot : MonoBehaviour
{
    [Header("Nettoyage")]
    [Tooltip("Points de vie de la tache")]
    public int hitsToClean = 2; 
    
    [Tooltip("Distance max pour que le balai puisse nettoyer")]
    public float cleanRadius = 1.5f;
    
    [Header("Visuel")]
    public Renderer dirtRenderer;
    public float fadeSpeed = 0.3f;
    public ParticleSystem cleanParticles;
    public GameObject bubbleEffectPrefab;

    [Header("Audio")]
    public AudioClip cleanSound;
    private AudioSource audioSource;

    private int currentDamageTaken = 0;
    private Material dirtMaterial;
    private Color originalColor;
    private float targetAlpha;
    private float currentAlpha;
    private bool isCleaned = false;

    void Start()
    {
        // --- SETUP AUDIO & VISUEL ---
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && cleanSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }

        if (dirtRenderer == null) dirtRenderer = GetComponent<Renderer>();

        if (dirtRenderer != null)
        {
            dirtMaterial = dirtRenderer.material;
            originalColor = dirtMaterial.color;
            currentAlpha = originalColor.a;
            targetAlpha = originalColor.a;
        }

        // --- ENREGISTREMENT ---
        if (CleaningManager.Instance != null)
        {
            CleaningManager.Instance.RegisterDirt(this);
        }
    }

    void Update()
    {
        // --- VISUEL (FADE) ---
        if (dirtMaterial != null && !Mathf.Approximately(currentAlpha, targetAlpha))
        {
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime / fadeSpeed);
            Color newColor = originalColor;
            newColor.a = currentAlpha;
            dirtMaterial.color = newColor;

            if (isCleaned && currentAlpha < 0.01f)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnDestroy()
    {
        // Sécurité : si détruit, on prévient le manager
        if (CleaningManager.Instance != null)
        {
            CleaningManager.Instance.RemoveDirt(this);
        }
        
        if (dirtMaterial != null) Destroy(dirtMaterial);
    }

    public void TryClean(Vector3 broomPosition)
    {
        if (isCleaned) return;

        float distance = Vector3.Distance(transform.position, broomPosition);
        
        if (distance <= cleanRadius)
        {
            // --- GESTION PUISSANCE DU BALAI ---
            int damage = 1;
            // Si on a le Super Balai, on tape plus fort
            if (GameManager.Instance != null) 
            {
                damage = GameManager.Instance.broomPower;
            }

            currentDamageTaken += damage;
            
            // --- FEEDBACK VISUEL ---
            float progress = (float)currentDamageTaken / hitsToClean;
            targetAlpha = Mathf.Lerp(originalColor.a, 0f, progress);

            // Effets (Bulles, Particules, Son)
            if (bubbleEffectPrefab != null)
            {
                GameObject bubbles = Instantiate(bubbleEffectPrefab, transform.position, Quaternion.identity);
                Destroy(bubbles, 2f);
            }

            if (cleanParticles != null) cleanParticles.Play();
            if (audioSource != null && cleanSound != null) audioSource.PlayOneShot(cleanSound);

            StartCoroutine(CleanFeedback());

            // --- VÉRIFICATION ---
            if (currentDamageTaken >= hitsToClean)
            {
                OnCleaned();
            }
        }
    }

    void OnCleaned()
    {
        isCleaned = true;
        targetAlpha = 0f;

        // On prévient juste le Manager pour la barre verte
        if (CleaningManager.Instance != null)
        {
            CleaningManager.Instance.RemoveDirt(this);
        }

    }

    System.Collections.IEnumerator CleanFeedback()
    {
        Vector3 originalScale = transform.localScale;
        float duration = 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.1f;
            transform.localScale = originalScale * scale;
            yield return null;
        }
        transform.localScale = originalScale;
    }
}