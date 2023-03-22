using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class WallRunningAdvanced : NetworkBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")] 
    private InputModule _inputModule;
    // public KeyCode jumpKey = KeyCode.Space;
    // public KeyCode upwardsRunKey = KeyCode.LeftShift;
    // public KeyCode downwardsRunKey = KeyCode.LeftControl;
    private bool upwardsRunning;
    private bool downwardsRunning;
    [SerializeField]
    private float horizontalInput;
    [SerializeField]
    private float verticalInput;
    private bool jump;
    private InputAction _movement;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    [SerializeField]
    private bool wallLeft;
    [SerializeField]
    private bool wallRight;

    [Header("Exiting")]
    [SerializeField]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;
    public float gravityMultiplierForce;

    [Header("References")]
    public Transform orientation;
    public CinemachineVirtualCamera cam;
    private PlayerMovement pm;
    //private LedgeGrabbing lg;
    private Rigidbody rb;

    private void Awake()
    {
        _inputModule = App.Modules.Get<InputModule>();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        //lg = GetComponent<LedgeGrabbing>();
        orientation = transform;
        cam = pm._camera;
    }

    public override void OnStartLocalPlayer()
    {
        // _inputModule.BindStartedAction("PlayerMovement/Sprint",_=>upwardsRunning=true);
        // _inputModule.BindCancledAction("PlayerMovement/Sprint",_=>upwardsRunning=false);
        //
        // _inputModule.BindStartedAction("PlayerMovement/Crouch",_=>downwardsRunning=true);
        // _inputModule.BindCancledAction("PlayerMovement/Crouch",_=>downwardsRunning=false);
        
        _inputModule.BindPerformedAction("PlayerMovement/Jump",_=>jump=true);
        
        _movement = _inputModule.Input.FindAction("PlayerMovement/Move");
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (pm.wallrunning)
            WallRunningMovement();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    // private bool AboveGround()
    // {
    //     return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    // }

    private void StateMachine()
    {
        // Getting Inputs
        // horizontalInput = Input.GetAxisRaw("Horizontal");
        // verticalInput = Input.GetAxisRaw("Vertical");
        //
        // upwardsRunning = Input.GetKey(upwardsRunKey);
        // downwardsRunning = Input.GetKey(downwardsRunKey);

        Vector2 movementValue = _movement.ReadValue<Vector2>();
        Vector3 inputDirection = new Vector3(movementValue.x, 0.0f, movementValue.y).normalized;
        verticalInput = inputDirection.z;
        horizontalInput = inputDirection.x;
        
        // State 1 - Wallrunning
        if((wallLeft || wallRight) && verticalInput > 0 && !pm.grounded && !exitingWall)
        {
            if (!pm.wallrunning)
                StartWallRun();

            // wallrun timer
            if (wallRunTimer > 0)
                wallRunTimer -= Time.deltaTime;

            if(wallRunTimer <= 0 && pm.wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            // wall jump
            if (jump) WallJump();
        }

        // State 2 - Exiting
        else if (exitingWall)
        {
            if (pm.wallrunning)
                StopWallRun();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
                exitingWall = false;
        }

        // State 3 - None
        else
        {
            if (pm.wallrunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        pm.wallrunning = true;

        wallRunTimer = maxWallRunTime;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // apply camera effects
        if (cam == null) return;
        // cam.DoFov(90f);
        // if (wallLeft) cam.DoTilt(-5f);
        // if (wallRight) cam.DoTilt(5f);
    }

    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // upwards/downwards force
        if (upwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
        if (downwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);

        // push to wall force
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);

        // weaken gravity
        if (useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun()
    {
        pm.wallrunning = false;

        // reset camera effects
        if (cam == null) return;
        // cam.DoFov(80f);
        // cam.DoTilt(0f);
    }

    private void WallJump()
    {
        //if (lg.holding || lg.exitingLedge) return;

        // enter exiting wall state
        exitingWall = true;
        exitWallTimer = exitWallTime;
        jump = false;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // reset y velocity and add force
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}