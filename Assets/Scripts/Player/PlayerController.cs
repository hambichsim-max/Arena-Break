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

    [Header("Dash")]
    [SerializeField, Min(0f)] private float _dashDistance = 5f;
    [SerializeField, Min(0.01f)] private float _dashDuration = 0.2f;
    [SerializeField, Min(0f)] private float _dashCooldown = 1f;

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
    private InputAction _dashAction;
    private float verticalVelocity;
    private float _dashTimeRemaining;
    private float _dashCooldownRemaining;
    private Vector3 _dashDirection;
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

        _dashAction = new InputAction("Dash", InputActionType.Button, "<Keyboard>/q");
        pitch = NormalizeAngle(cameraPivot.localEulerAngles.x);
    }

    private void OnEnable()
    {
        if (playerActionMap == null)
            return;

        playerActionMap.Enable();
        _dashAction?.Enable();

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
        _dashAction?.Disable();
        _dashTimeRemaining = 0f;

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
        float deltaTime = Time.deltaTime;
        _dashCooldownRemaining = Mathf.Max(0f, _dashCooldownRemaining - deltaTime);

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

        if (_dashAction.WasPressedThisFrame() && _dashCooldownRemaining <= 0f)
            BeginDash(moveInput);

        if (_dashTimeRemaining > 0f)
        {
            float dashStep = Mathf.Min(deltaTime, _dashTimeRemaining);
            Vector3 dashDisplacement = _dashDirection * (_dashDistance / _dashDuration) * dashStep;
            Vector3 verticalDisplacement = Vector3.up * verticalVelocity * deltaTime;
            characterController.Move(dashDisplacement + verticalDisplacement);
            _dashTimeRemaining -= dashStep;
            return;
        }

        Vector3 horizontalMove = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 velocity = horizontalMove * moveSpeed + Vector3.up * verticalVelocity;

        characterController.Move(velocity * deltaTime);
    }

    private void BeginDash(Vector2 moveInput)
    {
        if (_dashDistance <= 0f || _dashDuration <= 0f)
            return;

        Vector3 dashDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        _dashDirection = dashDirection.sqrMagnitude > 0f ? dashDirection.normalized : transform.forward;
        _dashTimeRemaining = _dashDuration;
        _dashCooldownRemaining = _dashCooldown;
    }

    private void OnDestroy()
    {
        _dashAction?.Dispose();
    }

        private static float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }
    }
}
