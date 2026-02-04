using UnityEngine;

/// <summary>
/// Support pour ranger le balai a sa place
/// VERSION INTERACTIVE : Le joueur doit appuyer sur E pour poser le balai
/// </summary>
public class BroomHolder : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Position exacte ou le balai sera range")]
    public Transform broomSnapPoint;
    
    [Tooltip("Rotation du balai quand il est range")]
    public Vector3 broomRotation = new Vector3(0, 0, 45);
    
    [Header("Methode de rangement")]
    [Tooltip("Desactiver les colliders (RECOMMANDE) ou les mettre en Trigger")]
    public bool disableColliders = true; // Si false, met en Trigger
    
    [Header("Visuel (Optionnel)")]
    [Tooltip("Objet visuel qui montre ou poser le balai")]
    public GameObject placeholderVisual;

    // Propriete publique pour savoir si le holder a un balai
    public bool HasBroom { get; private set; } = false;
    private GameObject currentBroom;

    void Start()
    {
        if (broomSnapPoint == null)
        {
            GameObject snapPoint = new GameObject("BroomSnapPoint");
            snapPoint.transform.SetParent(transform);
            snapPoint.transform.localPosition = Vector3.zero;
            snapPoint.transform.localRotation = Quaternion.Euler(broomRotation);
            broomSnapPoint = snapPoint.transform;
        }

        // Check if a broom starts in the holder
        BroomController[] brooms = FindObjectsOfType<BroomController>();
        foreach (BroomController broom in brooms)
        {
            if (Vector3.Distance(broom.transform.position, broomSnapPoint.position) < 0.5f)
            {
                // Force place without animation logic for setup
                ForcePlaceBroom(broom.gameObject);
                break;
            }
        }

        UpdateVisual();
    }

    public bool PlaceBroom(GameObject broomObj)
    {
        if (HasBroom || broomObj == null) return false;

        currentBroom = broomObj;
        HasBroom = true;

        // Positionner
        broomObj.transform.position = broomSnapPoint.position;
        broomObj.transform.rotation = broomSnapPoint.rotation;

        // Parent au support
        broomObj.transform.SetParent(broomSnapPoint);

        // Gestion des colliders
        Collider[] colliders = broomObj.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (col != null)
            {
                if (disableColliders) col.enabled = false;
                else col.isTrigger = true;
            }
        }

        // Physique
        Rigidbody rb = broomObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Update BroomController state
        BroomController controller = broomObj.GetComponent<BroomController>();
        if(controller != null)
        {
            controller.isEquipped = false;
        }

        Debug.Log("Balai range sur le support !");
        UpdateVisual();
        return true;
    }

    void ForcePlaceBroom(GameObject broom)
    {
        currentBroom = broom;
        HasBroom = true;
        
        broom.transform.SetParent(broomSnapPoint);
        broom.transform.localPosition = Vector3.zero;
        broom.transform.localRotation = Quaternion.identity;

        Rigidbody rb = broom.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
        
        Collider[] colliders = broom.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (disableColliders) col.enabled = false;
            else col.isTrigger = true;
        }
    }

    public void ResetBroom()
    {
        // On cherche le balai n'importe où
        BroomController broom = FindAnyObjectByType<BroomController>();
        
        if (broom != null)
        {
            // On le détache de la main du joueur ou d'ailleurs
            broom.transform.SetParent(null);
            
            // On force le placement 
            ForcePlaceBroom(broom.gameObject); 
            
            // On s'assure que le collider du balai est bien désactivé 
            // pour que le joueur clique sur le SUPPORT et non le balai
            Collider[] cols = broom.GetComponentsInChildren<Collider>();
            foreach(var c in cols) c.enabled = false;

            Debug.Log("🧹 Balai réinitialisé de force sur le support !");
        }
    }

    public void OnBroomTaken()
    {
        HasBroom = false;
        currentBroom = null;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (placeholderVisual != null)
        {
            placeholderVisual.SetActive(!HasBroom);
        }
    }
}