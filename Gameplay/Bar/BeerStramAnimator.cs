using UnityEngine;

public class BeerStreamAnimator : MonoBehaviour
{
    [Header("Animation")]
    public float pourSpeed = 2f;
    public float streamLength = 0.5f;

    private Vector3 startScale;
    private bool isPouring = false;

    void Start()
    {
        startScale = transform.localScale;
        gameObject.SetActive(false); // Caché par défaut
    }

    void Update()
    {
        if (isPouring)
        {
            // Animer l'échelle Y pour simuler le versement
            float newY = Mathf.PingPong(Time.time * pourSpeed, streamLength);
            transform.localScale = new Vector3(startScale.x, newY, startScale.z);
        }
    }

    public void StartPour()
    {
        gameObject.SetActive(true);
        isPouring = true;
    }

    public void StopPour()
    {
        isPouring = false;
        gameObject.SetActive(false);
        // Réinitialiser la taille
        if (startScale != Vector3.zero)
        {
            transform.localScale = startScale;
        }
    }
}