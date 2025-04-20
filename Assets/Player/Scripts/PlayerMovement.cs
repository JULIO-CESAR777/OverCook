using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset playerControls;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    private float currentSpeed;
    private Vector2 moveInput;
    private Rigidbody rb;

    [Header("Camera")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Camera playerCamera;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    [Header("FOV Sprint Effect")]
    public float normalFOV;
    public float fovTransitionSpeed = 5f;

    private Vector2 lookInput;
    private float verticalRotation = 0f;
    
    [Header("Head Bobbing")]
    public float walkBobSpeed = 14f;
    public float walkBobAmount = 0.05f;
    public float sprintBobSpeed = 18f;
    public float sprintBobAmount = 0.1f;

    private float bobTimer = 0f;
    private Vector3 defaultCamLocalPos;
    
    [Header("Footstep Sounds")]
    public AudioSource footstepSource;
    public AudioClip[] footstepClips;

    public float walkStepRate = 0.5f;
    public float sprintStepRate = 0.35f;

    private float footstepTimer = 0f;
    

    // InputActions
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;

    private void Awake()
    {
        defaultCamLocalPos = cameraHolder.localPosition;
        
        rb = GetComponent<Rigidbody>();
        
        var actionMap = playerControls.FindActionMap("Player");
        moveAction = actionMap.FindAction("Move");
        lookAction = actionMap.FindAction("Look");
        sprintAction = actionMap.FindAction("Sprint");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        normalFOV = playerCamera.fieldOfView;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        sprintAction.Disable();
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();
        
        bool isSprinting = sprintAction.IsPressed();
        bool isMoving = moveInput.magnitude > 0.1f;
        
        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // Actualizar FOV suavemente
        float sprintFOV = normalFOV + 20f;
        float targetFOV = isSprinting ? sprintFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);

        HandleLook();
        HandleHeadBobbing(isMoving, isSprinting);
        
        if (isMoving && rb.linearVelocity.magnitude > 0.1f && footstepSource != null)
        {
            footstepTimer += Time.deltaTime;

            float stepRate = isSprinting ? sprintStepRate : walkStepRate;
            if (footstepTimer >= stepRate)
            {
                PlayFootstep();
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f; // reiniciar cuando no se mueve
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 moveVelocity = transform.TransformDirection(moveDirection) * currentSpeed;
        rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
    }

    private void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        cameraHolder.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
    
    private void HandleHeadBobbing(bool isMoving, bool isSprinting)
    {
        if (!isMoving)
        {
            // Volver suavemente a la posición original si no se mueve
            cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, defaultCamLocalPos, Time.deltaTime * 5f);
            bobTimer = 0f;
            return;
        }

        float bobSpeed = isSprinting ? sprintBobSpeed : walkBobSpeed;
        float bobAmount = isSprinting ? sprintBobAmount : walkBobAmount;

        bobTimer += Time.deltaTime * bobSpeed;
        float bobOffsetY = Mathf.Sin(bobTimer) * bobAmount;
        float bobOffsetX = Mathf.Cos(bobTimer * 0.5f) * bobAmount * 0.5f; // opcional para balanceo lateral

        Vector3 targetPos = defaultCamLocalPos + new Vector3(bobOffsetX, bobOffsetY, 0f);
        cameraHolder.localPosition = targetPos;
    }
    
    private void PlayFootstep()
    {
        if (footstepClips.Length == 0 || footstepSource == null) return;

        int randomIndex = Random.Range(0, footstepClips.Length);
        footstepSource.PlayOneShot(footstepClips[randomIndex]);
    }
    
}
