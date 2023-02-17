using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cinemachine;

public class PlayerStateMachine : MonoBehaviour
{
    [Header("References")]
    public GunHandler gunHandler;

    public CinemachineVirtualCamera _followVirtualCamera;
    public CinemachineVirtualCamera _aimVirtualCamera;

    [SerializeField]
    private Image _normalReticle;
    [SerializeField]
    private Image _aimReticle;

    public LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField]
    private Transform debugTransform;


    PlayerInput _playerInput;
    CharacterController _characterController;
    Animator _animator;

    private Vector3 _mouseWorldPosition = Vector3.zero;


    int _isDashingHash;
    int _isRunningHash;
    int _isJumpingHash;
    int _isFallingHash;
    int _isFlyingHash;


    Vector2 _currentMovementInput;
    Vector3 _currentMovement;
    Vector3 _currentRunMovement;
    Vector3 _appliedMovement;

    Vector3 _cameraRelativeMovement;

    bool _isMovementPressed;
    bool _isFlyPressed;
    bool _isAimPressed;
    bool _isDashPressed;
    bool _isShootPressed;

    float _gravity = -9.8f;

    bool _isJumpPressed = false;
    float _initialJumpVelocity;
    float _maxJumpHeight = 2f;
    float _maxJumpTime = 1f;
    bool _isJumping = false;
    bool _requireNewJumpPress;
    float maxHealth = 100;
    float health = 100;

    [Header("Dash")]
    [SerializeField]
    float _dashGas = 1f;
    [SerializeField]
    float _dashThrust = 15f;

    float _rotationFactorPerFrame = 15.0f;

    [Header("RunSpeedMultiplier")]
    [SerializeField]
    float _runMultiplier = 5f;

    [Header("Jetpack")]
    [SerializeField]
    float _jetPackGas = 1f;
    [SerializeField]
    float _jetPackThrust = 0.25f;
    [SerializeField]
    float _jetPackConsumptionSpeed = 0.3f;
    [SerializeField]
    float _jetPackRefuelSpeed = 0.1f;

    [Header("Debug")]
    [SerializeField]
    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    public PlayerBaseState CurrentState { get { return CurrentState; } set { _currentState = value; } }
    public CharacterController CharacterController { get { return _characterController; } }
    public Animator Animator { get { return _animator; } }

    public bool IsMovementPressed { get { return _isMovementPressed; } }
    public bool IsDashPressed { get { return _isDashPressed; } }
    public bool IsJumpPressed { get { return _isJumpPressed; } }
    public bool IsFlyPressed { get { return _isFlyPressed; } }
    public bool IsAimPressed { get { return _isAimPressed; } }
    public bool IsShootPressed { get { return _isShootPressed; } }


    public int IsDashingHash { get { return _isDashingHash; } }
    public int IsRunningHash { get { return _isRunningHash; } }
    public int IsJumpingHash { get { return _isJumpingHash; } }
    public int IsFallingHash { get { return _isFallingHash; } }
    public int IsFlyingHash { get { return _isFlyingHash; } }


    public float Gravity { get { return _gravity; } }

    public bool IsJumping { get { return _isJumping; } set { _isJumping = value; } }
    public float Health { get { return health; } }
    public float MaxHealth { get { return maxHealth; } }

    public bool RequireNewJumpPress { get { return _requireNewJumpPress; } set { _requireNewJumpPress = value; } }
    public float InitialJumpVelocity { get { return _initialJumpVelocity; } }


    public float CurrentMovementY { get { return _currentMovement.y; } set { _currentMovement.y = value; } }
    public float AppliedMovementY { get { return _appliedMovement.y; } set { _appliedMovement.y = value; } }
    public float AppliedMovementX { get { return _appliedMovement.x; } set { _appliedMovement.x = value; } }
    public float AppliedMovementZ { get { return _appliedMovement.z; } set { _appliedMovement.z = value; } }

    public float RunMultiplier { get { return _runMultiplier; } }
    public Vector2 CurrentMovementInput { get { return _currentMovementInput; } }

    public Vector3 CameraRelativeMovement { get { return _cameraRelativeMovement; } set { _cameraRelativeMovement = value; } }

    public float JetPackGas { get { return _jetPackGas; } set { _jetPackGas = value; } }
    public float JetPackThrust { get { return _jetPackThrust; } }
    public float JetPackConsumptionSpeed { get { return _jetPackConsumptionSpeed; } set { _jetPackConsumptionSpeed = value; } }
    public float JetPackRefuelSpeed { get { return _jetPackRefuelSpeed; } set { _jetPackRefuelSpeed = value; } }

    public float DashGas { get { return _dashGas; } set { _dashGas = value; } }
    public float DashThrust { get { return _dashThrust; } }

    private void Awake()
    {
        _playerInput = new PlayerInput();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        gunHandler = GetComponent<GunHandler>();

        _states = new PlayerStateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();

        _isDashingHash = Animator.StringToHash("isDashing");
        _isRunningHash = Animator.StringToHash("isRunning");
        _isJumpingHash = Animator.StringToHash("isJumping");
        _isFallingHash = Animator.StringToHash("isFalling");
        _isFlyingHash = Animator.StringToHash("isFlying");

        _playerInput.CharacterControls.Move.started += OnMovementInput;
        _playerInput.CharacterControls.Move.canceled += OnMovementInput;
        _playerInput.CharacterControls.Move.performed += OnMovementInput;
        _playerInput.CharacterControls.Dash.started += OnDash;
        _playerInput.CharacterControls.Dash.canceled += OnDash;
        _playerInput.CharacterControls.Jump.started += OnJump;
        _playerInput.CharacterControls.Jump.canceled += OnJump;
        _playerInput.CharacterControls.Fly.performed += OnFly;
        _playerInput.CharacterControls.Fly.canceled += OnFly;
        _playerInput.CharacterControls.Aim.started += OnAim;
        _playerInput.CharacterControls.Aim.canceled += OnAim;
        _playerInput.CharacterControls.Shoot.started += OnShoot;
        _playerInput.CharacterControls.Shoot.canceled += OnShoot;

        SetupJumpVariables();
    }

    private void OnEnable()
    {
        _playerInput.CharacterControls.Enable();
    }


    private void OnDisable()
    {
        _playerInput.CharacterControls.Disable();
    }


    private void Start()
    {
        _characterController.Move(_appliedMovement * Time.deltaTime);
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!_isDashPressed && CharacterController.isGrounded)
        {
            _jetPackGas = Mathf.Min(1.0f, _jetPackGas + _jetPackRefuelSpeed * Time.deltaTime);
        }

        HandleRotation();
        _currentState.UpdateStates();

        HandleShooting();
        HandleAim();
        _cameraRelativeMovement = ConvertToCameraSpace(_appliedMovement);
        _characterController.Move(_cameraRelativeMovement * Time.deltaTime);
    }

    private void SetupJumpVariables()
    {
        float timeToApex = _maxJumpTime / 2;
        _gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        _initialJumpVelocity = (2 * _maxJumpHeight) / timeToApex;
    }


    private void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _currentMovement.x = _currentMovementInput.x;
        _currentMovement.z = _currentMovementInput.y;
        _currentRunMovement.x = _currentMovementInput.x * _runMultiplier;
        _currentRunMovement.z = _currentMovementInput.y * _runMultiplier;
        _isMovementPressed = _currentMovementInput.x != 0 || _currentMovementInput.y != 0;
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        _isDashPressed = context.ReadValueAsButton();
    }

    private void OnFly(InputAction.CallbackContext context)
    {
        _isFlyPressed = context.ReadValueAsButton();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();
        _requireNewJumpPress = false;
    }

    private void OnAim(InputAction.CallbackContext context)
    {
        _isAimPressed = context.ReadValueAsButton();
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        _isShootPressed = context.ReadValueAsButton();
    }


    private void HandleRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = _cameraRelativeMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = _cameraRelativeMovement.z;

        Quaternion currentRotation = transform.rotation;

        if (_isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame * Time.deltaTime);
        }
    }

    private void HandleAim()
    {

        if (_isAimPressed)
        {
            _aimVirtualCamera.Priority = 20;
            _aimReticle.gameObject.SetActive(true);
            _normalReticle.gameObject.SetActive(false);
            _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));

        }
        else
        {
            _aimVirtualCamera.Priority = 10;
            _normalReticle.gameObject.SetActive(true);
            _aimReticle.gameObject.SetActive(false);
            _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
        }

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);

        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        {
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
            {
                debugTransform.position = raycastHit.point;
                _mouseWorldPosition = raycastHit.point;
            }
        }
    }

    private void HandleShooting()
    {
        if (_isAimPressed)
        {
            gunHandler.enabled = true;
        }
        else
        {
            gunHandler.enabled = false;
        }
    }

    Vector3 ConvertToCameraSpace (Vector3 vectorToRotate)
    {
        float currentYValue = vectorToRotate.y;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 cameraFowardZProduct = vectorToRotate.z * cameraForward;
        Vector3 cameraRightXproduct = vectorToRotate.x * cameraRight;

        Vector3 vectorRotatedToCameraSpace = cameraFowardZProduct + cameraRightXproduct;
        vectorRotatedToCameraSpace.y = currentYValue;
        return vectorRotatedToCameraSpace;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        //Don't destroy but change phase
        //if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }
}
