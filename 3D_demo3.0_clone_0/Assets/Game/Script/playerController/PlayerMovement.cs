using System;
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
    public float rotationSpeed = 1f;
    public float speedChangeRate = 10.0f;

    [Header("Jumping & Falling")]
    //public float jumpHeight = 1.2f;
    //public float gravity = -9.81f;
    public bool readyToJump=true;
    public float jumpCd=2f;
    public float jumpForce=10f;
    public float airMultiplier=1f;

    [Header("Grounded Check")] 
    public bool grounded = true;
    public float groundedOffset = 0.8f;
    public float groundedRadius = 0.5f;
    public LayerMask groundLayers;
    public float groundDrag=2f;
    
    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    public float playerHeight=2f;

    
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

        if(!isLocalPlayer || _rigidbody==null)
            return;
        
        _inputModule.BindPerformedAction(
            "PlayerMovement/Jump",
            _ => _jump = true
        );

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
        if(!isLocalPlayer || _rigidbody==null)
            return;
        
        UpdateGrounded();
        
        ProcessJumping();
        
        SpeedControl();
    }

    private void LateUpdate()
    {
        onGamePauseOrContinue();
        if (Time.timeScale == 0 || Globals.Instance.pauseOrNot)
            return;

        if(!isLocalPlayer || _rigidbody==null)
            return;
        
        RotateCamera();
    }

    private void FixedUpdate()
    {
        if(!isLocalPlayer || _rigidbody==null)
            return;
        
        ProcessMoving();
        
    }

    /**
     * for rigidbody
     */
    private void ProcessJumping()
    {
        if (grounded)
        {
            _rigidbody.drag = groundDrag;
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
            _rigidbody.drag = 0;
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
     * for rigidbody
     */
    private void ProcessMoving()
    {
        if (freeze) return;
        movementValue = _movement.ReadValue<Vector2>();
        
        _speed = allowSprinting && _sprint ? sprintSpeed : moveSpeed;
        
        //if (movementValue == Vector2.zero) _speed = 0.0f;
        
        Vector3 inputDirection = new Vector3(movementValue.x, 0.0f, movementValue.y).normalized;
        if (movementValue != Vector2.zero)
        {
            inputDirection = transform.right * movementValue.x + transform.forward * movementValue.y;
        }
        

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            _rigidbody.AddForce(GetSlopeMoveDirection(inputDirection) * _speed * speedChangeRate, ForceMode.Force);

            if (_rigidbody.velocity.y > 0)
                _rigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (grounded)
            _rigidbody.AddForce(inputDirection.normalized * _speed * speedChangeRate, ForceMode.Force);

        // in air
        else if (!grounded)
            _rigidbody.AddForce(inputDirection.normalized * _speed * speedChangeRate * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        //if(!wallrunning) _rigidbody.useGravity = !OnSlope();
    }
    
    private void SpeedControl()
    {
        if(freeze)return;
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (_rigidbody.velocity.magnitude > _speed)
                _rigidbody.velocity = _rigidbody.velocity.normalized * _speed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > _speed)
            {
                Vector3 limitedVel = flatVel.normalized * _speed;
                _rigidbody.velocity = new Vector3(limitedVel.x, _rigidbody.velocity.y, limitedVel.z);
            }
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
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

            // rotate the player left and right
            Quaternion deltaRotation = Quaternion.Euler(new Vector3(0, _rotationVelocity, 0));
            _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);

            // Update Cinemachine camera target pitch
            lookCamera.transform.localRotation = Quaternion.Euler(_cameraPitch, 0.0f, 0.0f);
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
