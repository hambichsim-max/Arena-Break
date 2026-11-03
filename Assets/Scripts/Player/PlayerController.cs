using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaBreak.Controllers
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0f)] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField, Min(0f)] private float mouseSensitivity = 0.1f;
    [SerializeField, Min(0f)] private float maxMouseDeltaPerFrame = 20f;
    [SerializeField, Range(0f, 89f)] private float pitchLimit = 80f;
    [SerializeField] private Transform cameraPivot;

    [Header("Cursor")]
    [SerializeField] private bool lockCursor = true;

    private CharacterController characterController;
    private InputActionMap playerActionMap;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private float verticalVelocity;
    private float pitch;
    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (inputActions == null)
        {
            Debug.LogError("PlayerController requires an InputActionAsset reference.", this);
            enabled = false;
            return;
        }

        playerActionMap = inputActions.FindActionMap("Player");
        if (playerActionMap == null)
        {
            Debug.LogError("InputActionAsset is missing the 'Player' action map.", this);
            enabled = false;
            return;
        }

        moveAction = playerActionMap.FindAction("Move");
        lookAction = playerActionMap.FindAction("Look");
        jumpAction = playerActionMap.FindAction("Jump");
        if (moveAction == null || lookAction == null || jumpAction == null)
        {
            Debug.LogError("The 'Player' action map must contain 'Move', 'Look', and 'Jump' actions.", this);
            enabled = false;
            return;
        }

        if (cameraPivot == null)
        {
            Debug.LogError("Assign the child camera pivot Transform to PlayerController.", this);
            enabled = false;
            return;
        }

        pitch = NormalizeAngle(cameraPivot.localEulerAngles.x);
    }

    private void OnEnable()
    {
        if (playerActionMap == null)
            return;

        playerActionMap.Enable();

        if (lockCursor)
        {
            previousCursorLockState = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnDisable()
    {
        playerActionMap?.Disable();

        if (lockCursor)
        {
            Cursor.lockState = previousCursorLockState;
            Cursor.visible = previousCursorVisible;
        }
    }

    private void Update()
    {
        UpdateLook();
        UpdateMovement();
    }

    private void UpdateLook()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        float deltaLimit = Mathf.Max(0f, maxMouseDeltaPerFrame);
        lookInput = Vector2.ClampMagnitude(lookInput, deltaLimit);

        float yaw = lookInput.x * mouseSensitivity;
        float pitchDelta = lookInput.y * mouseSensitivity;

        transform.Rotate(0f, yaw, 0f, Space.Self);

        pitch = Mathf.Clamp(pitch - pitchDelta, -pitchLimit, pitchLimit);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void UpdateMovement()
    {
        bool isGrounded = characterController.isGrounded;
        if (isGrounded)
        {
            verticalVelocity = -2f;

            if (jumpAction.WasPressedThisFrame())
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector2 moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        Vector3 horizontalMove = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 velocity = horizontalMove * moveSpeed + Vector3.up * verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
    }

        private static float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }
    }
}
