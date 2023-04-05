using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class WeaponPickable : BaseInteractable
{
    [SyncVar]
    public Weapon_SO weapon;

    [SyncVar,HideInInspector] 
    public int totalMagazines,
        magazineSize;
    [SyncVar]
    public int  bulletsLeftInMagazine,
        totalBullets; // Internal use
    [HideInInspector]
    [SyncVar]
    public int weaponId;
    private GameObject weaponIn;

    public bool isInitialWeapon;

    [Header("Whether Destroyable")]
    [SyncVar] public bool allowToDestroy;
    
    private void OnValidate()
    {
    }

    public override void Start()
    {
        base.Start();
        displayName = $"Pick Up Weapon <color=red> {weapon._name} </color>";
        weaponId = weapon.weaponID;
        weaponIn=Instantiate(weapon.pickUpGraphics,transform,false);

        totalMagazines = weapon.totalMagazines;
        magazineSize = weapon.magazineSize;
    }

    public override void OnStartServer()
    {
        if (isInitialWeapon)
        {
            bulletsLeftInMagazine = magazineSize;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    public override void OnInteract(GameObject fatherObject)
    {
        //AuthorityRequest(fatherObject);

        if (fatherObject.tag == "Player" && fatherObject.GetComponent<WeaponController>() != null)
        {
            //Debug.Log($"{fatherObject.GetComponent<PlayerMovement>().isLocal}");
            fatherObject.GetComponent<WeaponController>().CmdChangeWeaponIdentification(this);
        }
        
    }

    [Command(requiresAuthority = false)]
    private void AuthorityRequest(GameObject fatherObject)
    {
        if (fatherObject.GetComponent<NetworkIdentity>() != null)
        {
            GetComponent<NetworkIdentity>()
                .AssignClientAuthority(fatherObject.GetComponent<NetworkIdentity>().connectionToClient);
        }
    }

    public override void OffInteract()
    {
        // GetComponent<NetworkIdentity>().RemoveClientAuthority();
    }

    [Command(requiresAuthority = false)]
    public void CmdDestroyThisObject()
    {
        if(allowToDestroy)
            NetworkServer.Destroy(this.gameObject);
    }

    //private void Destroy(GameObject destroyObject) => NetworkServer.Destroy(destroyObject);
    void OnDisable()
    {
        Destroy(weaponIn);
    }
}
