using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class WeaponSpawner : NetworkSpawner
{

    [SerializeField]
    public Weapon_SO weapon;
    
    [HideInInspector]
    //[SyncVar]
    public int totalMagazines,
        magazineSize,
        bulletsLeftInMagazine,
        totalBullets; // Internal use

    //[SyncVar]
    public int currentBullets,
        currentMagazineSize;
    
    [Header("Whether Destroyable")]
    //[SyncVar] 
    public bool allowToDestroy=true;
    public override void OnValidate()
    {
        base.OnValidate();
        isInstantiated = true;
    }

    void Start()
    {
       
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public override void OnStartServer()
    {
        CmdSynWeapon();
        base.OnStartServer();
    }

    [Command]
    public void CmdSynWeapon()
    {
        objectSpawn = Instantiate(Resources.Load<GameObject>("Prefabs/weapons/pickable/weaponPickUp"),transform.position,transform.rotation);
        WeaponPickable pick=objectSpawn.GetComponent<WeaponPickable>();
        
        pick.weapon = this.weapon;
        pick.allowToDestroy = this.allowToDestroy;
    }

    public override void Awake()
    {
        base.Awake();
    }
}
