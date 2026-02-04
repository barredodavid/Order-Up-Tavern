using UnityEngine;
using UnityEngine.UI;

public class SinkController : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject cleanGlassPrefab;
    public Transform outputPoint; 
    public float washDuration = 1.5f;
    public int maxCapacity = 5;

    [Header("État")]
    public int currentDirtyGlasses = 0;
    private float currentWashProgress = 0f;

    [Header("UI")]
    public GameObject progressBarCanvas; 
    public Image progressBarFill;        

    [Header("Audio & Effets")]
    public AudioClip dropGlassSound; 
    public AudioClip washingLoopSound; 
    public AudioClip finishSound;
    public ParticleSystem splashParticles; 

    // On sépare les deux lecteurs audio
    private AudioSource sfxSource; 
    private AudioSource loopSource;  

    void Start()
    {
        // 1. Configuration du lecteur d'effets (SFX)
        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 1f; // Son 3D

        // 2. Création automatique du lecteur de boucle (Loop)
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.playOnAwake = false;
        loopSource.loop = true; // On le force en boucle
        loopSource.spatialBlend = 1f; // Son 3D
        loopSource.clip = washingLoopSound; // On prépare le clip

        if (cleanGlassPrefab == null) Debug.LogWarning("⚠️ cleanGlassPrefab manquant !");
        
        UpdateUI();
    }

    public bool AddDirtyGlass()
    {
        if (currentDirtyGlasses < maxCapacity)
        {
            currentDirtyGlasses++;
            
            // On joue sur le canal SFX qui n'est jamais coupé
            if (sfxSource != null && dropGlassSound != null)
            {
                sfxSource.PlayOneShot(dropGlassSound);
            }

            Debug.Log($"Verre ajouté ! Stock : {currentDirtyGlasses}/{maxCapacity}");
            
            if (currentDirtyGlasses == 1 && GameManager.Instance != null && GameManager.Instance.currentState == GameState.TutorialCleaning)
            {
                TutorialManager tuto = FindAnyObjectByType<TutorialManager>();
                if (tuto != null) tuto.ShowWashMechanicTutorial();
            }
            
            UpdateUI();
            return true;
        }
        return false;
    }

    public void WashContinuous()
    {
        if (currentDirtyGlasses > 0)
        {
            float speedMultiplier = (GameManager.Instance != null) ? GameManager.Instance.washSpeedMultiplier : 1.0f;
            currentWashProgress += Time.deltaTime * speedMultiplier;
            
            if (progressBarCanvas != null) progressBarCanvas.SetActive(true);
            if (progressBarFill != null) progressBarFill.fillAmount = currentWashProgress / washDuration;

            if (splashParticles != null && !splashParticles.isPlaying) splashParticles.Play();

            // On utilise le canal LOOP dédié
            if (loopSource != null && !loopSource.isPlaying && washingLoopSound != null)
            {
                // On s'assure que le clip est bien assigné
                if (loopSource.clip != washingLoopSound) loopSource.clip = washingLoopSound;
                loopSource.Play();
            }

            if (currentWashProgress >= washDuration) FinishWashingOneGlass();
        }
        else
        {
            StopWashing();
        }
    }

    public void StopWashing()
    {
        currentWashProgress = 0f;
        if (splashParticles != null) splashParticles.Stop();
        
        // On coupe SEULEMENT l'eau, pas le splash en cours !
        if (loopSource != null)
        {
            loopSource.Stop();
        }

        if (progressBarCanvas != null) progressBarCanvas.SetActive(false);
    }

    private void FinishWashingOneGlass()
    {
        currentDirtyGlasses--;
        currentWashProgress = 0f;

        // On joue le ding sur le canal SFX
        if (sfxSource != null && finishSound != null) sfxSource.PlayOneShot(finishSound);

        Vector3 basePos = (outputPoint != null) ? outputPoint.position : transform.position + Vector3.up;
        Instantiate(cleanGlassPrefab, basePos + new Vector3(Random.Range(-0.2f,0.2f), 0.1f, Random.Range(-0.2f,0.2f)), Quaternion.Euler(0, Random.Range(0, 360), 0));
        
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.TutorialCleaning)
        {
            GameManager.Instance.UnlockTutorialEnd();
        }

        UpdateUI();
        if (currentDirtyGlasses == 0) StopWashing();
    }

    void UpdateUI()
    {
        if (currentWashProgress == 0 && progressBarCanvas != null) progressBarCanvas.SetActive(false);
    }
}