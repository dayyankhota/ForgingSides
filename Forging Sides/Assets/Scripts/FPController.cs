using UnityEngine;
using UnityEngine.InputSystem;

public class FPController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Sprint Settings")]
    public float sprintSpeed = 8f;
    public float speedChangeRate = 10f;
    public bool sprintForwardOnly = true;

    [Header("Look Settings")]
    public Transform cameraTransform;
    public float lookSensitivity = 2f;
    public float verticalLookLimit = 90f;

    private CharacterController controller;
    private Animator anim;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float verticalRotation = 0f;

    private bool sprintHeld = false;
    private float currentSpeed;

    public bool IsSprinting { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        currentSpeed = moveSpeed;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
        UpdateAnimations();
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        sprintHeld = context.ReadValueAsButton();
    }

    private Vector2 EffectiveMoveInput => CraftingQTE.IsActive ? Vector2.zero : moveInput;

    public void HandleMovement()
    {
        Vector2 input = EffectiveMoveInput;

        bool isMoving = input.sqrMagnitude > 0.01f;
        bool directionAllowed = !sprintForwardOnly || input.y > 0.1f;
        IsSprinting = sprintHeld && isMoving && directionAllowed;

        float targetSpeed = IsSprinting ? sprintSpeed : moveSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, speedChangeRate * Time.deltaTime);

        Vector3 move = transform.right * input.x + transform.forward * input.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void HandleLook()
    {
        if (CraftingQTE.IsActive) return;

        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);

        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void UpdateAnimations()
    {
        if (anim != null)
        {
            float animSpeed = EffectiveMoveInput.magnitude * (currentSpeed / moveSpeed);
            anim.SetFloat("Speed", animSpeed);
        }
    }
}