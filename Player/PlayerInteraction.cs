using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro; 

public class PlayerInteraction : MonoBehaviour
{
    [Header("Reglages")]
    public float interactionDistance = 3.0f;
    public LayerMask interactionLayer;

    [Header("Configuration des Mains")]
    public Animator handAnimator;
    public Transform broomMountPoint;
    public Transform glassMountPoint;

    [Header("Etat Main")]
    public GameObject currentHeldObject;

    private float nextSweepTime = 0f;
    private bool isWashing = false;
    private bool hasShownBeerTutorial = false;
    private bool hasShownServingTutorial = false;
    

    private SimpleFPSController playerController;

    void Start()
    {
        playerController = GetComponentInParent<SimpleFPSController>();
    }

    void Update()
    {
        // 1. RAYCAST
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, interactionDistance, interactionLayer);

        HandleUI(hit, hitSomething);

        // --- LOGIQUE DE LAVAGE CONTINUE (MAINTENIR E) ---
        if (hitSomething && Keyboard.current != null)
        {
            SinkController sink = hit.collider.GetComponent<SinkController>();
            
            // Si on regarde le tonneau, qu'on a les mains vides, et qu'on appuie sur E
            if (sink != null && currentHeldObject == null && Keyboard.current.eKey.isPressed)
            {
                // Bloquer le mouvement
                if (playerController != null) playerController.CanMove = false;

                sink.WashContinuous(); 
                
                if (handAnimator) handAnimator.SetBool("Washing", true);
                isWashing = true;
            }
            // Si on relâche ou qu'on ne regarde plus le tonneau
            else 
            {
                // Débloquer le mouvement
                if (playerController != null) playerController.CanMove = true;

                // Si c'était bien un évier qu'on regardait (ou qu'on vient de lâcher)
                if (sink != null) sink.StopWashing();
                
                if (handAnimator) handAnimator.SetBool("Washing", false);
                isWashing = false;
            }
        }
        else
        {
            // Sécurité : Si on ne regarde rien du tout, on débloque aussi
            if (playerController != null) playerController.CanMove = true;
            if (handAnimator) handAnimator.SetBool("Washing", false);
        }

        // --- CLIC GAUCHE (BALAI) ---
        if (Mouse.current != null && Mouse.current.leftButton.isPressed && 
            currentHeldObject != null && currentHeldObject.GetComponent<BroomController>() != null)
        {
            // On vérifie le Timer D'ABORD
            if (Time.time >= nextSweepTime)
            {
                // 1. On lance l'animation
                if (handAnimator) handAnimator.SetTrigger("Sweep");
                
                // 2. On calcule le point d'impact
                Vector3 sweepCenter = transform.position + transform.forward * 1.5f;
                if (hitSomething) sweepCenter = hit.point;

                // 3. Nettoyage de la zone
                Collider[] hitColliders = Physics.OverlapSphere(sweepCenter, 1.0f);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider != null)
                    {
                        DirtSpot dirt = hitCollider.GetComponent<DirtSpot>();
                        if (dirt != null) 
                        {
                            dirt.TryClean(dirt.transform.position);
                        }
                    }
                }

                // 4. On remet le Timer (1.2f = plus lent entre chaque coup)
                nextSweepTime = Time.time + 1.2f;
            }
        }

        // --- TOUCHE E (INTERACTION UNIQUE) ---
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && hitSomething)
        {
            GameObject target = hit.collider.gameObject;

            // 1. DETECTER LE LIVRE
            if (target.name == "LedgerBook")
            {
                if (ShopManager.Instance != null) ShopManager.Instance.OpenShop();
                return;
            }

            // 2. GESTION DES PORTES
            if (target.TryGetComponent(out DoorController door))
            {
                door.Toggle();
                return;
            }

            // 3. PRENDRE LE BALAI
            if (currentHeldObject == null)
            {
                BroomController broom = target.GetComponent<BroomController>();
                if (broom == null) broom = target.GetComponentInParent<BroomController>();
                if (broom == null)
                {
                    BroomHolder holder = target.GetComponentInParent<BroomHolder>();
                    if (holder != null && holder.HasBroom) broom = holder.GetComponentInChildren<BroomController>();
                }

                if (broom != null)
                {
                    BroomHolder holder = broom.GetComponentInParent<BroomHolder>();
                    if (holder != null) holder.OnBroomTaken();

                    if (broomMountPoint != null)
                    {
                        HoldObject(broom.gameObject, broomMountPoint);
                        broom.Equip(broomMountPoint);
                        if (handAnimator) handAnimator.SetBool("HoldBroom", true);
                    }
                    return;
                }
            }
            
            // 4. VERIFICATION PROPRETE & TUTO
            // On vérifie si on est dans le Tuto de nettoyage OU si la taverne est sale
            bool isTutorialCleaning = GameManager.Instance != null && GameManager.Instance.currentState == GameState.TutorialCleaning;
            bool isDirty = CleaningManager.Instance != null && !CleaningManager.Instance.IsTavernClean();

            if (isTutorialCleaning || isDirty)
            {
                // Si le joueur essaie de toucher un Verre (sur table ou armoire) ou la Bière
                if (target.GetComponent<GlassController>() || 
                    target.GetComponent<GlassDispenser>() || 
                    target.GetComponent<BeerKegController>())
                {
                    // Message différent selon le cas
                    string msg = isTutorialCleaning ? "Le Barman a dit : Nettoyez d'abord !" : "C'est trop sale pour servir !";
                    
                    if (UIMessageManager.Instance != null) 
                        UIMessageManager.Instance.ShowInteractionMessage(msg);
                    
                    return; // Interaction impossible si zone sale
                }
            }

            // --- CAS : MAINS VIDES ---
            if (currentHeldObject == null)
            {
                // Prendre verre sale
                if (target.TryGetComponent(out DirtyGlass dirtyGlass))
                {
                    if (glassMountPoint != null) { 
                        HoldObject(dirtyGlass.gameObject, glassMountPoint); if (handAnimator) handAnimator.SetBool("HoldGlass", true); 
                        if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.TutorialCleaning)
                            {
                                TutorialManager tuto = FindAnyObjectByType<TutorialManager>();
                                if (tuto != null) tuto.ShowPutInBucketTutorial();
                            }
                    }
                    return;
                }
                // Prendre verre propre existant
                if (target.TryGetComponent(out GlassController glass))
                {
                    if (glassMountPoint != null) { 

                        HoldObject(glass.gameObject, glassMountPoint); glass.Equip(glassMountPoint); if (handAnimator) handAnimator.SetBool("HoldGlass", true); 

                    }
                    return;
                }
                // Prendre verre depuis distributeur (Armoire)
                if (target.TryGetComponent(out GlassDispenser dispenser))
                {
                    GameObject newGlass = dispenser.GetNewGlass();
                    if (newGlass != null && glassMountPoint != null)
                    {
                        HoldObject(newGlass, glassMountPoint);
                        GlassController glassScript = newGlass.GetComponent<GlassController>();
                        if (glassScript != null) glassScript.Equip(glassMountPoint);
                        if (handAnimator) handAnimator.SetBool("HoldGlass", true);
                    }
                    return;
                }
            }
            // --- CAS : MAINS PLEINES ---
            else 
            {
                GlassController heldGlass = currentHeldObject.GetComponent<GlassController>();
                DirtyGlass heldDirtyGlass = currentHeldObject.GetComponent<DirtyGlass>();
                BroomController heldBroom = currentHeldObject.GetComponent<BroomController>();
                CustomerAI customer = target.GetComponentInParent<CustomerAI>();

                if (heldGlass != null)
                {
                    // Remplir Bière
                    if (!heldGlass.isFilled && target.TryGetComponent(out BeerKegController keg))
                    {
                        keg.FillGlass(heldGlass); 
                        return;
                    }
                    // Servir Client
                    if (heldGlass.isFilled && customer != null && customer.IsWaiting())
                    {
                        customer.ReceiveDrink();
                        Destroy(currentHeldObject);
                        if (handAnimator) handAnimator.SetBool("HoldGlass", false);
                        currentHeldObject = null;
                        
                        if (GameManager.Instance.currentState == GameState.TutorialServing && !hasShownServingTutorial)
                        {
                            hasShownServingTutorial = true;
                            TutorialManager tuto = FindAnyObjectByType<TutorialManager>();
                            if (tuto != null) tuto.ShowFirstCustomerServedTutorial();
                        }
                        return;
                    }
                    // Ranger un verre propre dans l'armoire (Suppression)
                    if (target.GetComponent<GlassDispenser>())
                    {
                        Destroy(currentHeldObject);
                        if (handAnimator) handAnimator.SetBool("HoldGlass", false);
                        currentHeldObject = null;
                        if (UIMessageManager.Instance != null) UIMessageManager.Instance.ShowInteractionMessage("Verre rangé");
                        return;
                    }
                }
                else if (heldDirtyGlass != null)
                {
                    if (target.TryGetComponent(out SinkController sink))
                    {
                        if (sink.AddDirtyGlass()) // S'il y a de la place
                        {
                            Destroy(currentHeldObject); // On détruit celui qu'on tient
                            currentHeldObject = null;
                            if (handAnimator) handAnimator.SetBool("HoldGlass", false);
                        }
                        else
                        {
                            if (UIMessageManager.Instance != null) UIMessageManager.Instance.ShowInteractionMessage("Le bac est plein !");
                        }
                        return;
                    }
                }
                else if (heldBroom != null)
                {
                    if (target.TryGetComponent(out BroomHolder holder) && !holder.HasBroom)
                    {
                        holder.PlaceBroom(currentHeldObject);
                        if (handAnimator) handAnimator.SetBool("HoldBroom", false);
                        currentHeldObject = null;
                        return;
                    }
                }
            }
        }
    }

    void HandleUI(RaycastHit hit, bool hitSomething)
    {
        if (UIMessageManager.Instance == null) return;

        string message = ""; // On commence avec un message vide

        // -----------------------------------------------------------
        // 1. D'ABORD, ON REGARDE SI ON VISE UN OBJET INTERACTIF
        // -----------------------------------------------------------
        if (hitSomething)
        {
            GameObject target = hit.collider.gameObject;

            // Conditions de blocage
            bool isDirty = CleaningManager.Instance != null && !CleaningManager.Instance.IsTavernClean();
            bool isTutoCleaning = GameManager.Instance != null && GameManager.Instance.currentState == GameState.TutorialCleaning;
            bool isBlocked = isDirty || isTutoCleaning;

            if (target.name == "LedgerBook")
            {
                message = "[E] Livre d'améliorations";
            }
            else if (currentHeldObject == null) // MAINS VIDES
            {
                if (target.GetComponent<BroomController>() || (target.GetComponent<BroomHolder>() && target.GetComponent<BroomHolder>().HasBroom))
                    message = "[E] Prendre le balai";
                else if (target.GetComponent<DoorController>())
                    message = "[E] Ouvrir/Fermer";
                else if (target.GetComponent<DirtyGlass>())
                    message = "[E] Debarrasser verre sale";
                else if (target.GetComponent<SinkController>())
                {
                    SinkController sink = target.GetComponent<SinkController>();
                    if (sink.currentDirtyGlasses > 0) message = "[Maintenir E] Laver la vaisselle";
                    else message = "Le bac est vide";
                }
                else if (target.GetComponent<GlassController>() || target.GetComponent<GlassDispenser>())
                {
                    message = isBlocked ? "Finissez le menage d'abord !" : "[E] Prendre un verre";
                }
            }
            else // MAINS PLEINES
            {
                GlassController heldGlass = currentHeldObject.GetComponent<GlassController>();
                DirtyGlass heldDirtyGlass = currentHeldObject.GetComponent<DirtyGlass>();
                BroomController heldBroom = currentHeldObject.GetComponent<BroomController>();
                CustomerAI customer = target.GetComponentInParent<CustomerAI>();
                
                if (heldGlass != null)
                {
                    if (target.GetComponent<BeerKegController>() && !heldGlass.isFilled)
                        message = isBlocked ? "Finissez le menage d'abord !" : "[E] Remplir biere";
                    else if (customer != null && heldGlass.isFilled)
                        message = "[E] Servir client";
                    else if (target.GetComponent<GlassDispenser>())
                        message = "[E] Ranger le verre";
                }
                else if (heldDirtyGlass != null)
                {
                    if (target.GetComponent<SinkController>())
                        message = "[E] Deposer dans le bac";
                }
                else if (heldBroom != null)
                {
                    if (target.GetComponent<DirtSpot>())
                        message = "[Clic-Gauche] pour nettoyer";
                    else if (target.GetComponent<BroomHolder>() && !target.GetComponent<BroomHolder>().HasBroom)
                        message = "[E] Ranger le balai";
                }
            }
        }

        // -----------------------------------------------------------
        // 2. SI AUCUN MESSAGE N'A ÉTÉ DÉFINI (On regarde le ciel, un mur, ou rien)
        // -----------------------------------------------------------
        if (message == "")
        {
            // Cela s'affichera par défaut si on ne vise rien d'autre d'important
            if (GameManager.Instance != null && 
                GameManager.Instance.currentState == GameState.Preparation && 
                GameManager.Instance.canOpenTavern)
            {
                message = "[F] Ouvrir la Taverne";
            }
        }

        // -----------------------------------------------------------
        // 3. AFFICHAGE FINAL
        // -----------------------------------------------------------
        if (message != "")
            UIMessageManager.Instance.ShowInteractionMessage(message);
        else
            UIMessageManager.Instance.HideInteractionMessage();
    }


    void HoldObject(GameObject obj, Transform mountPoint)
    {
        if (obj == null || mountPoint == null) return;

        currentHeldObject = obj;
        obj.transform.SetParent(mountPoint);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        SetLayerRecursively(obj, 2); 

        Collider[] cols = obj.GetComponentsInChildren<Collider>();
        foreach (Collider c in cols) if (c != null) c.enabled = false;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void DropCurrentObject()
    {
        if (currentHeldObject == null) return;

        BroomController broom = currentHeldObject.GetComponent<BroomController>();
        if (broom != null) broom.Drop(transform.position + transform.forward, transform.forward);

        GlassController glass = currentHeldObject.GetComponent<GlassController>();
        if (glass != null) glass.Unequip();

        SetLayerRecursively(currentHeldObject, 0); 
        
        Collider[] cols = currentHeldObject.GetComponentsInChildren<Collider>();
        foreach (Collider c in cols) if (c != null) c.enabled = true;

        Rigidbody rb = currentHeldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (handAnimator != null)
        {
            handAnimator.SetBool("HoldBroom", false);
            handAnimator.SetBool("HoldGlass", false);
        }
        currentHeldObject = null;
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) if (child != null) SetLayerRecursively(child.gameObject, newLayer);
    }

    public void OnBeerFilled()
    {
        if (!hasShownBeerTutorial)
        {
            hasShownBeerTutorial = true;
            TutorialManager tuto = FindAnyObjectByType<TutorialManager>();
            if (tuto != null) tuto.ShowServeCustomerTutorial();
        }
    }
}