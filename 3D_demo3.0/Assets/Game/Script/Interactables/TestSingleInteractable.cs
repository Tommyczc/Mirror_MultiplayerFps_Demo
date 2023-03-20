using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using TMPro;
using UnityEngine;

public class TestSingleInteractable : BaseInteractable
{
     [SerializeField] 
     private TMP_Text demoText;

     [SerializeField] 
     private string interact_text;

     [SyncVar]
     private int interactTimes;

     public override void Start()
     {
         base.Start();
         interact_text = (interact_text != null || interact_text != null) ? interact_text:"this object is touched";
     }

     [Command(requiresAuthority = false)]
     public override void OnInteract(GameObject fatherObject)
     {
         CmdshowInformation();
     }

     [ClientRpc]
     public void CmdshowInformation()
     {
         demoText.text += $"{interact_text} ---times: {interactTimes+=1}\n";
         if (demoText.textInfo.lineCount >= 5)
         {
             demoText.text = string.Join( "\n",
                 demoText.text.Split("\n").Skip(1).ToArray()
             );
         }
     }

     public void Update()
     {
         if (NetworkClient.localPlayer.gameObject != null)
         {
             demoText.transform.rotation = Quaternion.LookRotation(demoText.transform.position - NetworkClient.localPlayer.gameObject.transform.position);
         }
     }
     
     Vector3 GetSymmetryPoint(Transform LookTran) {
         return new Vector3(
             transform.position.x * 2 - LookTran.position.x, 
             transform.position.y * 2 - LookTran.position.y, 
             transform.position.z * 2 - LookTran.position.z
             );
     }
     
}
