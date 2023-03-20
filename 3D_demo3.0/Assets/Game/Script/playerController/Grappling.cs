using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grappling : NetworkBehaviour
{
   [Header("References")]
   private PlayerMovement pm;
   private Spring spring;
   public Transform cam;
   public Transform gunTip;
   public LayerMask whatIsGrappleable;
   public LineRenderer lr;

   [Header("Grappling")]
   public float maxGrappleDistance;
   public float grappleDelayTime;
   public float overshootYAxis;

   [SyncVar]
   private Vector3 grapplePoint;
   [SyncVar]
   private Vector3 currentGrapplePosition;
   
   [Header("Rope Setting")]
   public float waveCount;
   public float waveHeight;
   public int quality;
   public AnimationCurve affectCurve;
   public float damper;
   public float strength;
   public float velocity;

   [Header("Cooldown")]
   public float grapplingCd;
   private float grapplingCdTimer;
   public bool doNotUseTimer;

   [Header("Input")]
   private InputModule _inputModule;

   [Header("debug")]
   [SyncVar]
   public bool grappling;

   private void Start()
   {
       
   }

   private void Awake()
   {
       _inputModule = App.Modules.Get<InputModule>();
       spring = new Spring();
       spring.SetTarget(0);
   }

   public override void OnStartLocalPlayer()
   {
       pm = GetComponent<PlayerMovement>();
       _inputModule.BindPerformedAction("Interaction/Grapple",StartGrapple);
   }

   private void Update()
   {

       if (grapplingCdTimer > 0)
           grapplingCdTimer -= Time.deltaTime;
   }

   private void LateUpdate()
   {
       //if (grappling)
           //lr.SetPosition(0, gunTip.position);
           DrawRope();
   }

   private void StartGrapple(InputAction.CallbackContext ctx)
   {
       if (!doNotUseTimer)
       {
           if (grapplingCdTimer > 0) return;
       }

       grappling = true;

       //pm.freeze = true;

       RaycastHit hit;
       if(Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
       {
           grapplePoint = hit.point;

           Invoke(nameof(ExecuteGrapple), grappleDelayTime);
       }
       else
       {
           grapplePoint = cam.position + cam.forward * maxGrappleDistance;

           Invoke(nameof(StopGrapple), grappleDelayTime);
       }

       //CmdEnableLineRenState(true);
   }

   private void ExecuteGrapple()
   {
       //pm.freeze = true;

       Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

       float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
       float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

       if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

       pm.JumpToPosition(grapplePoint, highestPointOnArc);

       Invoke(nameof(StopGrapple), 1f);
   }

   public void StopGrapple()
   {
       //pm.freeze = false;

       grappling = false;

       grapplingCdTimer = grapplingCd;

       //CmdEnableLineRenState(false);
   }
   
   [Command]
   private void CmdEnableLineRenState(bool state)
   {
       RpcEnableLineRenState(state);
   }

   [ClientRpc]
   private void RpcEnableLineRenState(bool state)
   {
       if (state)
       {
           lr.enabled = true;
           //lr.SetPosition(1, grapplePoint);
       }
       else
       {
           lr.enabled = false;
       }
   }

   void DrawRope() {
       //If not grappling, don't draw rope
       if (!grappling) {
           currentGrapplePosition = gunTip.position;
           spring.Reset();
           if (lr.positionCount > 0)
               lr.positionCount = 0;
           return;
       }

       if (lr.positionCount == 0) {
           spring.SetVelocity(velocity);
           lr.positionCount = quality + 1;
       }
        
       spring.SetDamper(damper);
       spring.SetStrength(strength);
       spring.Update(Time.deltaTime);

       var grapplePoint = this.grapplePoint;
       var gunTipPosition = gunTip.position;
       var up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

       currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 12f);

       for (var i = 0; i < quality + 1; i++) {
           var delta = i / (float) quality;
           var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value *
                        affectCurve.Evaluate(delta);
            
           lr.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset);
       }
   }

   public bool IsGrappling()
   {
       return grappling;
   }

   public Vector3 GetGrapplePoint()
   {
       return grapplePoint;
   }
}
