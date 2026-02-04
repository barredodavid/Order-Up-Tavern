using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSController : MonoBehaviour
{
    [Header("État du Joueur")]
    public bool CanMove = true;

    [Header("Déplacement")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float gravity = 20f;
    
    [Header("Caméra")]
    public Transform cameraPivot; 
    public float mouseSensitivity = 0.2f;
    public float lookLimit = 85f;

    [Header("Animation")]
    public Animator handsAnimator;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        if (characterController == null)
        {
            Debug.LogError("❌ CharacterController manquant sur le joueur !");
            enabled = false;
            return;
        }

        if (cameraPivot == null)
        {
            Debug.LogWarning("⚠️ cameraPivot n'est pas assigné dans SimpleFPSController");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Désactivation du mouvement
        if (!CanMove)
        {
            // On arrête les animations de marche
            if (handsAnimator != null)
            {
                handsAnimator.SetFloat("Speed", 0f);
                handsAnimator.SetBool("IsWalking", false);
            }
            return;
        }

        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        if (characterController == null) return;

        if (characterController.isGrounded)
        {
            Vector2 input = Vector2.zero;
            bool isRunning = false;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) input.y += 1;
                if (Keyboard.current.sKey.isPressed) input.y -= 1;
                if (Keyboard.current.aKey.isPressed) input.x -= 1;
                if (Keyboard.current.dKey.isPressed) input.x += 1;
                isRunning = Keyboard.current.leftShiftKey.isPressed;
            }
            
            // Normaliser pour éviter d'aller plus vite en diagonale
            input.Normalize();

            // Calcul de la direction locale
            moveDirection = transform.right * input.x + transform.forward * input.y;
            
            float speed = isRunning ? runSpeed : walkSpeed;
            moveDirection *= speed;

            // Animation
            if (handsAnimator != null)
            {
                float currentSpeed = input.magnitude * (isRunning ? 2f : 1f);
                handsAnimator.SetFloat("Speed", currentSpeed);
                handsAnimator.SetBool("IsWalking", input.magnitude > 0.1f);
            }
        }

        // Gravité
        moveDirection.y -= gravity * Time.deltaTime;

        // Mouvement
        characterController.Move(moveDirection * Time.deltaTime);
    }

    void HandleRotation()
    {
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // Rotation du corps (Y)
        transform.Rotate(0, mouseDelta.x * mouseSensitivity, 0);

        // Rotation de la tête (X)
        rotationX -= mouseDelta.y * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -lookLimit, lookLimit);

        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
    }

    /// <summary>
    /// Permet de désactiver/réactiver le mouvement depuis d'autres scripts
    /// </summary>
    public void SetMovementEnabled(bool enabled)
    {
        CanMove = enabled;
        Debug.Log($"🎮 Mouvement {(enabled ? "activé" : "désactivé")}");
    }
}