using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;

    int isWalkingHash;
    int isRunningHash;

    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;

    bool isMovementPressed;
    bool isRunPressed;

    float groundedGravity = -0.05f;
    float gravity = -9.8f;

    bool isJumpPressed = false;
    float initialJumpVelocity;
    float maxJumpHeight = 3f;
    float maxJumpTime = 1f;
    bool isJumping = false;
    int isJumpingHash;
    bool isJumpAnimating;

    float RotationFactorPerFrame = 15.0f;
    float runMultiplier = 3.0f;



    private void Awake()
    {
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");

        playerInput.CharacterControls.Move.started += OnMovementInput;
        playerInput.CharacterControls.Move.canceled += OnMovementInput;
        playerInput.CharacterControls.Move.performed += OnMovementInput;
        playerInput.CharacterControls.Run.started += OnRun;
        playerInput.CharacterControls.Run.canceled += OnRun;
        playerInput.CharacterControls.Jump.started += OnJump;
        playerInput.CharacterControls.Jump.canceled += OnJump;

        SetupJumpVariables();
    }

    private void OnEnable()
    {
        playerInput.CharacterControls.Enable();
    }


    void Update()
    {        
        HandleRotation();
        HandleAnimator();

        if (isRunPressed)
        {
            characterController.Move(currentRunMovement * Time.deltaTime);
        }
        else
        {
            characterController.Move(currentMovement * Time.deltaTime);
        }
        HandleGravity();
        HandleJump();
    }
    
    private void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }

    private void OnMovementInput(InputAction.CallbackContext ctx)
    {
        currentMovementInput = ctx.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        currentRunMovement.x = currentMovementInput.x * runMultiplier;
        currentRunMovement.z = currentMovementInput.y * runMultiplier;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    private void OnRun(InputAction.CallbackContext ctx)
    {
        isRunPressed = ctx.ReadValueAsButton();
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        isJumpPressed = ctx.ReadValueAsButton();
    }

    private void SetupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    private void HandleJump()
    {
        if (!isJumping && characterController.isGrounded && isJumpPressed)
        {
            animator.SetBool(isJumpingHash, true);
            isJumpAnimating = true;
            isJumping= true;
            currentMovement.y = initialJumpVelocity * 0.5f;
            currentRunMovement.y = initialJumpVelocity * 0.5f;
        }
        else if (!isJumpPressed && isJumping && characterController.isGrounded  )
        {
            isJumping= false;
        }
    }

    private void HandleAnimator()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        if(isMovementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
        }
        
        else if(!isMovementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }
        
        if((isMovementPressed && isRunPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
        }
        
        else if((!isMovementPressed || !isRunPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }
    }

    private void HandleRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;

        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, RotationFactorPerFrame* Time.deltaTime);
        }
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded)
        {
            if (isJumpAnimating)
            {
                animator.SetBool(isJumpingHash, false);
                isJumpAnimating = false;
            }
            currentMovement.y = groundedGravity;
            currentRunMovement.y = groundedGravity;
        }
        else
        {
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (gravity * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            currentMovement.y = nextYVelocity;
            currentRunMovement.y = nextYVelocity;
        }
    }
}
