using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NpcHealthController : CharacterHealthState
{
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if(NetworkClient.localPlayer.gameObject!=null)
            sliderObject.transform.rotation = Quaternion.LookRotation(sliderObject.transform.position - NetworkClient.localPlayer.gameObject.transform.position);
    }
}
