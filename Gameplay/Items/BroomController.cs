using UnityEngine;

public class BroomController : MonoBehaviour
{
    [Tooltip("Balai deja equipe ? (rempli par le Player)")]
    public bool isEquipped = false;

    private Rigidbody cachedRigidbody;
    private Collider cachedCollider;

    void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        cachedCollider = GetComponent<Collider>();
    }

    public void Equip(Transform holder)
    {
        if (holder == null) return;

        isEquipped = true;

        // Prevenir le support que le balai est pris
        BroomHolder broomHolder = FindObjectOfType<BroomHolder>();
        if (broomHolder != null)
        {
            broomHolder.OnBroomTaken();
        }

        // Attacher le balai a la main
        transform.SetParent(holder);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Desactiver physique
        if (cachedRigidbody != null)
        {
            cachedRigidbody.isKinematic = true;
            cachedRigidbody.useGravity = false;
        }

        if (cachedCollider != null)
        {
            cachedCollider.enabled = false;
        }
    }

    /// <summary>
    /// Poser le balai au sol
    /// </summary>
    public void Drop(Vector3 dropPosition, Vector3 forward)
    {
        isEquipped = false;

        // Detacher du holder
        transform.SetParent(null);

        // Poser a la position donnee
        transform.position = dropPosition;

        // Reactiver la physique
        if (cachedRigidbody != null)
        {
            cachedRigidbody.isKinematic = false;
            cachedRigidbody.useGravity = true;
            cachedRigidbody.linearVelocity = Vector3.zero;
            cachedRigidbody.angularVelocity = Vector3.zero;

            // Petit push vers l'avant
            cachedRigidbody.AddForce(forward * 1.5f, ForceMode.VelocityChange);
        }

        if (cachedCollider != null)
        {
            cachedCollider.enabled = true;
        }
    }
}