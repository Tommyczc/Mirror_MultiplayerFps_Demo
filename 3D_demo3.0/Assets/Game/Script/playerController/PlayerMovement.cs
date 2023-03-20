using System.Collections.Generic;
using Cinemachine;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : NetworkBehaviour
{
    
    [Header("Reference")]
    private InputModule _inputModule;
    /**
     * 由于游戏开发后期角色控制需要一些物理效果（例如抓钩，蹭墙），所以改用rigidbody组件
     */
    //private CharacterController characterController;
    [SerializeField]
    private Rigidbody _rigidbody;
    [SerializeField]
    private PlayerInteraction _playerInteraction;
    [SerializeField]
    private WeaponController wp;

    [Header("Config")] 
    public bool allowJumping;
    public bool allowSprinting;

    [Header("Player Capability")] 
    public float moveSpeed = 10.0f;
    public float sprintSpeed = 15.0f;
    [SyncVar]
    public float rotationSpeed = 1f;
    
    public float speedChangeRate = 10.0f;

    [Header("Jumping & Falling")]
    //public float jumpHeight = 1.2f;
    //public float gravity = -9.81f;
    public bool readyToJump=true;

    public float jumpCd=2f;

    public float jumpForce=10f;

    [Header("Grounded Check")] 
    public bool grounded = true;
    public float groundedOffset = 0.8f;
    public float groundedRadius = 0.5f;
    public LayerMask groundLayers;
    
    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    
    public bool activeGrapple
    {
        get { return activeGrapple;}
        set { freeze = value; }
    }
    [Header("Grappling")]
    public float grappleFov = 70f;

    [Header("Wall Runing")]
    public bool wallrunning;

    [Header("Ledge Grabbing")]
    public bool unlimited;
    public bool restricted;
    
    [Header("Camera Limits")] 
    public GameObject lookCamera;
    public CinemachineVirtualCamera _camera;
    public float originFov = 60;
    public float topClamp = 90.0f;
    public float bottomClamp = -90.0f;

    [Header("Canvas Limits")] 
    public Canvas user_Canvas;

    // cinemachine
    [SyncVar(hook = nameof(OnLookValueChanged))]
    private float _cameraPitch;

    // player
    private float _speed;
    [SyncVar]
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private const float Threshold = 0.01f;

    // player inputs
    public bool _jump;
    public bool _sprint;
    private InputAction _movement;
    private InputAction _look;

    [Header("Player Hand")] 
    public Transform leftHand;
    public Transform rightHand;

    [Header("DEBUG")] 
    public Vector2 movementValue;
    public Vector2 lookValue;
    public bool isLocal = false;
    public bool freeze;


    void OnValidate()
    {
        //todo only character controller
        // if (characterController == null)
        //     characterController = GetComponent<CharacterController>();

        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();
        if (user_Canvas == null)
            user_Canvas = transform.Find("Canvas_HUB").GetComponent<Canvas>();
        if (lookCamera == null)
            lookCamera = transform.Find("camera action").gameObject;
        if(_playerInteraction==null)
            _playerInteraction=GetComponent<PlayerInteraction>();
        if (_camera == null)
            _camera = lookCamera.GetComponent<CinemachineVirtualCamera>();
        
        //todo only character controller
        //characterController.enabled = false;

        //_playerInteraction.enabled = false;
        user_Canvas.gameObject.SetActive(false);
        //GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<NetworkTransform>().syncDirection = SyncDirection.ClientToServer;
        
    }

    public override void OnStartLocalPlayer()
    {
        //todo only character controller
        // characterController.enabled = true;
        
        //_playerInteraction.enabled = true;
        isLocal = true;
        Globals.Instance.player = this.gameObject;
        _camera.m_Lens.FieldOfView = originFov;
        wp = GetComponent<WeaponController>();
        loadSceneFinished();
    }

    private void Awake()
    {
        //todo only character controller
        // if (!isLocalPlayer || characterController == null || !characterController.enabled)
        //     return;
        
        _inputModule = App.Modules.Get<InputModule>();
        _verticalVelocity = -2.0f;
        _sprint = false;
    }

    private void Start()
    {
        gameObject.tag = "Player";
        
        //todo only character controller
        // if (!isLocalPlayer || characterController == null || !characterController.enabled)
        //     return;
        
        if(!isLocalPlayer || _rigidbody==null)
            return;
        
        _inputModule.BindPerformedAction(
            "PlayerMovement/Jump",
            _ => _jump = true
        );

        // _inputModule.BindPerformedAction(
        //     "PlayerMovement/Sprint",
        //     _ => _sprint = !_sprint
        // );
        _inputModule.BindStartedAction(
            "PlayerMovement/Sprint",
            _ => _sprint =true
            );

        _inputModule.BindCancledAction(
            "PlayerMovement/Sprint",
            _ => _sprint =false
        );
        
        _movement = _inputModule.Input.FindAction("PlayerMovement/Move");

        _look = _inputModule.Input.FindAction("PlayerMovement/Look");
        
        _inputModule.BindPerformedAction("Interaction/Drop",dropItem);
    }

    private void Update()
    {
        //todo only character controller
        // if (!isLocalPlayer || characterController == null || !characterController.enabled)
        //     return;

        if(!isLocalPlayer || _rigidbody==null)
            return;
        
        
        UpdateGrounded();
        
        ProcessJumping();

        ProcessMoving();

    }

    private void LateUpdate()
    {
        onGamePauseOrContinue();
        if (Time.timeScale == 0 || Globals.Instance.pauseOrNot)
            return;
        
        //todo only character controller
        // if (!isLocalPlayer || characterController == null || !characterController.enabled)
        //     return;

        if(!isLocalPlayer || _rigidbody==null)
            return;
        
        RotateCamera();
    }

    
    /**
     * jump for character controller
     */
    // private void ProcessJumping()
    // {
    //     if (grounded)
    //     {
    //         // reset the fall timeout timer
    //         _fallTimeoutDelta = fallTimeout;
    //
    //         // stop our velocity dropping infinitely when grounded
    //         if (_verticalVelocity < 0.0f)
    //         {
    //             _verticalVelocity = -2f;
    //         }
    //
    //         // Jump
    //         if (allowJumping && _jump && _jumpTimeoutDelta <= 0.0f)
    //         {
    //             // the square root of H * -2 * G = how much velocity needed to reach desired height
    //             _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    //         }
    //
    //         // jump timeout
    //         if (_jumpTimeoutDelta >= 0.0f)
    //         {
    //             _jumpTimeoutDelta -= Time.deltaTime;
    //         }
    //     }
    //     else
    //     {
    //         // reset the jump timeout timer
    //         _jumpTimeoutDelta = jumpTimeout;
    //
    //         // fall timeout
    //         if (_fallTimeoutDelta >= 0.0f)
    //         {
    //             _fallTimeoutDelta -= Time.deltaTime;
    //         }
    //
    //         // if we are not grounded, do not jump
    //         _jump = false;
    //     }
    //
    //     // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
    //     if (_verticalVelocity < _terminalVelocity)
    //     {
    //         _verticalVelocity += gravity * Time.deltaTime;
    //     }
    // }

    /**
     * for rigidbody
     */
    private void ProcessJumping()
    {
        if (grounded)
        {
            // Jump
            if (allowJumping && _jump && readyToJump )
            {
                exitingSlope = true;
                readyToJump = false;
                _rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
                Invoke(nameof(ResetJump), jumpCd);
            }
        }
        else
        {
            
        }
    }

    private void ResetJump()
    {
        exitingSlope = false;
        _jump = false;
        readyToJump = true;
    }

    private void UpdateGrounded()
    {
        var position = transform.position;
        Vector3 spherePosition = new Vector3(position.x, position.y - groundedOffset, position.z);
        grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
            QueryTriggerInteraction.Ignore);
    }

    /**
     * only fot character controller
     */
    // private void ProcessMoving()
    // {
    //     movementValue = _movement.ReadValue<Vector2>();
    //
    //     float targetSpeed = allowSprinting && _sprint ? sprintSpeed : moveSpeed;
    //
    //     if (movementValue == Vector2.zero) targetSpeed = 0.0f;
    //
    //     float currentHorizontalSpeed =
    //         new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z).magnitude;
    //
    //     float speedOffset = 0.05f;
    //
    //     if (currentHorizontalSpeed < targetSpeed - speedOffset ||
    //         currentHorizontalSpeed > targetSpeed + speedOffset)
    //     {
    //         _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
    //
    //         _speed = Mathf.Round(_speed * 10000f) / 10000f;
    //     }
    //     else
    //     {
    //         _speed = targetSpeed;
    //     }
    //
    //     Vector3 inputDirection = new Vector3(movementValue.x, 0.0f, movementValue.y).normalized;
    //
    //     if (movementValue != Vector2.zero)
    //     {
    //         inputDirection = transform.right * movementValue.x + transform.forward * movementValue.y;
    //     }
    //
    //     Vector3 movedThisFrame = inputDirection.normalized * (_speed * Time.deltaTime) +
    //                              new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;
    //
    //
    //     // move the player
    //     characterController.Move(movedThisFrame);
    // }

    /**
     * for rigidbody
     */
    private void ProcessMoving()
    {
        if (freeze) return;
        movementValue = _movement.ReadValue<Vector2>();
        
        float targetSpeed = allowSprinting && _sprint ? sprintSpeed : moveSpeed;
        
        if (movementValue == Vector2.zero) targetSpeed = 0.0f;
        
        Vector3 inputDirection = new Vector3(movementValue.x, 0.0f, movementValue.y).normalized;
        if (movementValue != Vector2.zero)
        {
            inputDirection = transform.right * movementValue.x + transform.forward * movementValue.y;
        }
        
        
        if(!wallrunning) _rigidbody.useGravity = !OnSlope();
        
        // speed controll
        Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x,0f,_rigidbody.velocity.z);
        if (OnSlope() && !exitingSlope)
        {
            if (_rigidbody.velocity.magnitude > targetSpeed)
            {
                 _rigidbody.velocity = _rigidbody.velocity.normalized * targetSpeed;
            }
            else
            {
                _rigidbody.AddForce(GetSlopeMoveDirection(inputDirection) * targetSpeed * 20f, ForceMode.Force);
            }
            
            if ( _rigidbody.velocity.y > 0)
                _rigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else
        {
            if (flatVelocity.magnitude > targetSpeed)
            {
                //Debug.LogWarning("over speed");
                Vector3 limitVelocity = flatVelocity.normalized * targetSpeed;
                _rigidbody.velocity = new Vector3(limitVelocity.x, _rigidbody.velocity.y, limitVelocity.z);
            }
            else
            {
                _rigidbody.AddForce(inputDirection * targetSpeed * speedChangeRate, ForceMode.Force);
            }
        }
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, groundedOffset * 5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private void RotateCamera()
    {
        lookValue=_look.ReadValue<Vector2>();
        // if there is an input
        if (lookValue.sqrMagnitude >= Threshold)
        {
            _cameraPitch += lookValue.y * rotationSpeed;
            _rotationVelocity = lookValue.x * rotationSpeed;

            // clamp our pitch rotation
            _cameraPitch=ClampAngle(_cameraPitch, bottomClamp, topClamp);

            CmdChangeCameraPitch(_cameraPitch);
            
            // Update Cinemachine camera target pitch
            lookCamera.transform.localRotation = Quaternion.Euler(_cameraPitch, 0.0f, 0.0f);
            // rotate the player left and right
            transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    [Command]
    private void CmdChangeCameraPitch(float _theCameraPitch)
    {
        _cameraPitch = _theCameraPitch;
    }

    private void OnLookValueChanged(float _oldCameraPitch, float _newCameraPitch)
    {
        if(isLocalPlayer)return;
        lookCamera.transform.localRotation = Quaternion.Euler(_newCameraPitch, 0.0f, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void loadSceneFinished()
    {
        //TODO: 把主机端角色的摄像头priority调成11（默认10）
        CinemachineVirtualCamera camera = gameObject.GetComponentInChildren<CinemachineVirtualCamera>();
        if (camera == null) return;
        camera.m_Priority = camera.Priority + 1;

        //TODO: promote cursor related to a cursor manager 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        user_Canvas.gameObject.SetActive(true);
    }

    private void onGamePauseOrContinue()
    {
        if ((Time.timeScale == 0 || Globals.Instance.pauseOrNot) && !SceneModule.isLoading)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            user_Canvas.enabled = false;
        }
        else if ((Time.timeScale == 1 || !Globals.Instance.pauseOrNot) && !SceneModule.isLoading)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            user_Canvas.enabled = true;
        }
    }
    
    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) 
                                               + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
    
    private bool enableMovementOnNextTouch;
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        //Invoke(nameof(ResetRestrictions), 3f);
    }
    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        _rigidbody.velocity = velocityToSet;

        _camera.m_Lens.FieldOfView=grappleFov;
    }

    public void ResetRestrictions()
    {
        activeGrapple = false;
        _camera.m_Lens.FieldOfView=originFov;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<Grappling>().StopGrapple();
        }
    }

    private void dropItem(InputAction.CallbackContext callbackContext)
    {
        if (rightHand.childCount > 0)
        {
            if (rightHand.transform.GetChild(0).tag == "Weapon")
            {
                wp.dropCurrentWeapon();
            }
        }

        if (leftHand.childCount > 0)
        {
            
        }
    }
}
