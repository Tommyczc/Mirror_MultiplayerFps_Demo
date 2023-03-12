using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class WeaponController : NetworkBehaviour
{

    [System.Serializable]
    public class Events
    {
        public UnityEvent OnShoot, OnReload, OnFinishReload, OnAim, OnStopAim, OnHit, OnInventorySlotChanged;
    }

    private InputModule _inputModule;

    [Tooltip("Attach your weapons in order depending of their ID")]
    public GameObject[] weapons;
    public GameObject[] weaponPickUpDictionary;

    public Weapon_SO[] initialWeapons;

    public LayerMask hitLayer;

    public GameObject[] inventory;

    // public UISlot[] slots;

    public Weapon_SO weapon;

    [Tooltip("Attach your main camera")] public CinemachineVirtualCamera mainCamera;

    [Tooltip("Attach your camera pivot object")]
    public Transform cameraPivot;

    private Transform[] firePoint;

    

    [Tooltip("Attach your weapon holder")] public Transform weaponHolder;


    [SerializeField, HideInInspector] public bool isAiming;

    private bool reloading;

    public bool Reloading
    {
        get { return reloading; }
        set { reloading = value; }
    }

    [Tooltip("If true you won´t have to press the reload button when you run out of bullets")]
    public bool autoReload;

    private float spread;

    public Events events;

    private int bulletsPerFire;

    public bool canShoot;

    RaycastHit hit;

    public int currentWeapon;

    public bool holding;

    [SyncVar(hook = nameof(weaponIdentificationChanged))]
    public WeaponPickable pickUp;

    public WeaponIdentification id;

    [SyncVar] private int bulletLeft;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer) return;
        if (_inputModule.isPressed("Interaction/Fire(click)") ) //按下鼠标左键
            if(weapon!=null&&currentWeapon!=0&&weapon.shootMethod==ShootingMethod.HoldAndRelease)
            {
                shoot(default);
            }
    }

    public override void OnStartLocalPlayer()
    {
        canShoot = true;
    }

    private void Awake()
    {
        _inputModule = App.Modules.Get<InputModule>();
    }

    private float aimingSpeed;

    public void Aim()
    {
        isAiming = true;
        if (weapon.applyBulletSpread) spread = weapon.aimSpreadAmount;
        events.OnAim.Invoke();
        Vector3 newPos = weapon.aimingPosition; // Get the weapon aimingPosition
        weaponHolder.localPosition = Vector3.Lerp(weaponHolder.transform.localPosition, newPos,
            weapon.aimingSpeed * Time.deltaTime);
        weaponHolder.localRotation = Quaternion.Lerp(weaponHolder.transform.localRotation,
            Quaternion.Euler(weapon.aimingRotation), weapon.aimingSpeed * Time.deltaTime);
    }

    public void StopAim()
    {
        if (weapon != null && weapon.applyBulletSpread) spread = weapon.spreadAmount;
        isAiming = false;
        // No need to declare we are not aiming cuz we have already done that on the input handling before
        events.OnStopAim.Invoke(); // Invoke your custom method on stop aiming

        Vector3 newPos = Vector3.zero;
        // Change the position and FOV
        weaponHolder.localPosition =
            Vector3.Lerp(weaponHolder.transform.localPosition, newPos, aimingSpeed * Time.deltaTime);
        weaponHolder.localRotation = Quaternion.Lerp(weaponHolder.transform.localRotation, Quaternion.Euler(newPos),
            aimingSpeed * Time.deltaTime);
    }

    
    private void shoot(InputAction.CallbackContext callbackContext)
    {
        int shootStyle = (int)weapon.shootStyle;
        Debug.Log($"Weapon start shooting, shoot-style: {weapon.shootStyle}, fire rate: {weapon.fireRate}, attack rate: {weapon.attackRate}");
        // Hitscan or projectile
        if (shootStyle == 0 || shootStyle == 1)
        {
            if (!canShoot) return;
            foreach (var p in firePoint)
            {
                canShoot = false; // since you have already shot, you will have to wait in order to being able to shoot again
                bulletsPerFire = weapon.bulletsPerFire;
                StartCoroutine(HandleShooting());
            }

            // if (weapon.timeBetweenShots == 0) SoundManager.Instance.PlaySound(weapon.audioSFX.firing, 0, .15f, 0);
            Invoke("CanShoot", weapon.fireRate);
        }
        else //Melee
        {
            canShoot = false;
            StartCoroutine(HandleShooting());
            Invoke("CanShoot", weapon.attackRate);
        }
    }

    private IEnumerator HandleShooting()
    {
        int style = (int)weapon.shootStyle;
        // Adding a layer of realism, bullet shells get instantiated and interact with the world
        // We should obviously first check if we really wanna do this
        // if(weapon.showBulletShells && style != 2)
        // {
        //     foreach(var p in firePoint)
        //     {
        //         
        //         var b = Instantiate(weapon.bulletGraphics, p.position, mainCamera.transform.rotation);
        //         // Adding random rotation to the instantiated bullet shells
        //         float torque = Random.Range(-15, 15);
        //         b.GetComponent<Rigidbody>().AddTorque(mainCamera.transform.right * torque, ForceMode.Impulse);
        //         b.GetComponent<Rigidbody>().AddForce(mainCamera.transform.right * 5, ForceMode.Impulse);
        //         b.GetComponent<Rigidbody>().AddForce(mainCamera.transform.up * 5, ForceMode.Impulse);
        //     }
        // }
        
        if(!weapon.infiniteBullets)
            Debug.Log($"current bullets: {id.bulletsLeftInMagazine}; bullet cost: {weapon.ammoCostPerFire}");
            id.bulletsLeftInMagazine -= weapon.ammoCostPerFire;

        switch (style)
        {
            case 0: //hitscan
                    int i = 0;  
                    while(i <bulletsPerFire)
                    {
                        HitscanShot();
                        // CamShake.instance.ShootShake(weapon.camShakeAmount);
                        // // Determine if we want to add an effect for FOV
                        // if (weapon.applyFOVEffectOnShooting)
                        // {
                        //     if(isAiming) mainCamera.fieldOfView = weapon.AimingFOVValue;
                        //     else mainCamera.fieldOfView = weapon.FOVValue;
                        // }
                        // foreach (var p in firePoint)
                        // {
                        // if(weapon.muzzleVFX != null)
                        // Instantiate(weapon.muzzleVFX, p.position, mainCamera.transform.rotation, mainCamera.transform); // VFX
                        // }
                        // StartCoroutine(CowsinsUtilities.PlayAnim("shooting", inventory[currentWeapon].GetComponent<Animator>()));
                        // if(weapon.timeBetweenShots != 0)SoundManager.Instance.PlaySound(weapon.audioSFX.firing, 0, .15f, 0);
                        //
                        // ProgressRecoil();
                        yield return new WaitForSeconds(weapon.timeBetweenShots);
                        i++;
                    }
                    yield break;
            
            case 1: // projectile   
                    i = 0;
                    while (i < bulletsPerFire)
                    {
                        CmdProjectileShot();
                        // CamShake.instance.ShootShake(weapon.camShakeAmount);
                        // // Determine if we want to add an effect for FOV
                        // if (weapon.applyFOVEffectOnShooting)
                        // {
                        //     if(isAiming) mainCamera.fieldOfView = weapon.AimingFOVValue;
                        //     else mainCamera.fieldOfView = weapon.FOVValue;
                        // }
                        // foreach (var p in firePoint)
                        // {
                        //     if (weapon.muzzleVFX != null)
                        //     Instantiate(weapon.muzzleVFX, p.position, mainCamera.transform.rotation, mainCamera.transform); // VFX
                        // }
                        // StartCoroutine(CowsinsUtilities.PlayAnim("shooting", inventory[currentWeapon].GetComponent<Animator>()));
                        // if(weapon.timeBetweenShots != 0) SoundManager.Instance.PlaySound(weapon.audioSFX.firing, 0, .15f, 0);
                        //
                        // ProgressRecoil(); 
                    yield return new WaitForSeconds(weapon.timeBetweenShots);
                        i++;
                    }
            break;
            // case 2:
            //     MeleeAttack(weapon.attackRange, weapon.damagePerHit);
            //     CamShake.instance.ShootShake(weapon.camShakeAmount);
            //     // Determine if we want to add an effect for FOV
            //     if (weapon.applyFOVEffectOnShooting) mainCamera.fieldOfView = weapon.FOVValue;
            //     StartCoroutine(CowsinsUtilities.PlayAnim("shooting", inventory[currentWeapon].GetComponent<Animator>()));
            // break;
        }
        
        yield return null;
    }

    private void CanShoot() => canShoot = true;
    
    [Command]
    private void HitscanShot()
    {
        events.OnShoot.Invoke(); 
        // if (resizeCrosshair && crosshair != null ) crosshair.Resize(weapon.crosshairResize * 100);

        Debug.Log($"HitscanShot start shooting");
        Transform hitObj; 

        //This defines the first hit on the object
        // Vector3 dir = CowsinsUtilities.GetSpreadDirection(weapon.spreadAmount, mainCamera);
        Vector3 dir = mainCamera.transform.forward;
        Ray ray = new Ray(mainCamera.transform.position, dir);

        if (Physics.Raycast(ray, out hit, weapon.bulletRange, hitLayer))
        {
            float dmg= weapon.damagePerBullet; 
            Hit(hit.collider.gameObject.layer,dmg,hit,true); 
            hitObj = hit.collider.transform;

            
            //Handle Penetration 穿墙，二次射线检测
            // Ray newRay = new Ray(hit.point, ray.direction);
            // RaycastHit newHit;
            //
            // if (Physics.Raycast(newRay, out newHit, weapon.penetrationAmount,hitLayer))
            // {
            //     if(hitObj != newHit.collider.transform)
            //     {
            //         float dmg_ = weapon.damagePerBullet * GetComponent<PlayerStats>().damageMultiplier * weapon.penetrationDamageReduction;
            //         Hit(newHit.collider.gameObject.layer, dmg_, newHit,true);
            //     }
            // }
        }
    }

    [Command]
    private void CmdProjectileShot()
    {
        Debug.Log($"CmdProjectileShot start shooting");
        
        events.OnShoot.Invoke(); 
        // if (resizeCrosshair && crosshair != null) crosshair.Resize(weapon.crosshairResize * 100);

        Vector3 dir = mainCamera.transform.forward;
        Ray ray = new Ray(mainCamera.transform.position, dir);
        // Vector3 destination = (Physics.Raycast(ray, out hit) && hit.transform.tag != "Player") ? destination = hit.point + CowsinsUtilities.GetSpreadDirection(weapon.spreadAmount, mainCamera) : destination = ray.GetPoint(50f) + CowsinsUtilities.GetSpreadDirection(weapon.spreadAmount, mainCamera); 
        Vector3 destination = (Physics.Raycast(ray, out hit) && hit.transform.tag != "Player") ? destination = hit.point: destination = ray.GetPoint(50f); 
        
        foreach (var p in firePoint)
        {
            Bullet bullet = Instantiate(weapon.projectile, p.position, p.transform.rotation) as Bullet;

            if(weapon.explosionOnHit) bullet.explosionVFX = weapon.explosionVFX;

            bullet.hurtsPlayer = weapon.hurtsPlayer;
            bullet.explosionOnHit = weapon.explosionOnHit;
            bullet.explosionRadius = weapon.explosionRadius;
            bullet.explosionForce = weapon.explosionForce;

            bullet.criticalMultiplier = weapon.criticalDamageMultiplier; 
            bullet.destination = destination;
            bullet.player = this.transform;
            bullet.speed = weapon.speed;
            bullet.GetComponent<Rigidbody>().isKinematic = (!weapon.projectileUsesGravity) ? true : false; 
            bullet.damage = weapon.damagePerBullet;
            bullet.duration = weapon.bulletDuration; 
            
            NetworkServer.Spawn(bullet.gameObject);
        }
    }

    // [Command]
    private void Hit(int gameObjectLayer, float damage, RaycastHit h, bool damageTarget)
    {
        
        // Apply damage
        if (!damageTarget) return;
        if(h.collider.gameObject.tag == "Critical") h.collider.transform.parent.GetComponent<IDamageable>().Damage(damage * weapon.criticalDamageMultiplier * GetDistanceDamageReduction(h.collider.transform));
        else if (h.collider.GetComponent<IDamageable>() != null) h.collider.GetComponent<IDamageable>().Damage(damage * GetDistanceDamageReduction(h.collider.transform));
    }
    
    private float GetDistanceDamageReduction(Transform target)
    {
        if(!weapon.applyDamageReductionBasedOnDistance) return 1; 
        if (Vector3.Distance(target.position, transform.position) > weapon.minimumDistanceToApplyDamageReduction)
            return (weapon.minimumDistanceToApplyDamageReduction / Vector3.Distance(target.position, transform.position) ) * weapon.damageReductionMultiplier;
        else return 1; 
    }

    void weaponIdentificationChanged(WeaponPickable oldId, WeaponPickable newId)
    {
        StartCoroutine(swapWeapon(newId));
    }

    IEnumerator swapWeapon(WeaponPickable newId)
    {
        if (newId == null) yield break;
        
        //delete last weapon
        destroyCurrentWeapon();
        
        //for swapping
        Debug.Log($"Swapping weapon {newId.weapon.weaponID}");
        foreach (GameObject weaponPrefab in weapons)
        {
            if (weaponPrefab.GetComponent<WeaponIdentification>().weapon.weaponID == newId.weapon.weaponID)
            {
                Debug.Log($"found weapon {newId.weapon.weaponID}");
                GameObject weaponIn=Instantiate(weaponPrefab,weaponHolder.position,weaponHolder.rotation,weaponHolder);
                        
                // weaponIn.GetComponent<WeaponIdentification>().totalMagazines = pickUp.totalMagazines;
                weaponIn.GetComponent<WeaponIdentification>().totalBullets = newId.totalBullets;
                weaponIn.GetComponent<WeaponIdentification>().currentBullets = newId.currentBullets;
                // weaponIn.GetComponent<WeaponIdentification>().magazineSize = pickUp.magazineSize;
                weaponIn.GetComponent<WeaponIdentification>().bulletsLeftInMagazine = newId.bulletsLeftInMagazine;
                weaponIn.GetComponent<WeaponIdentification>().currentMagazineSize = newId.currentMagazineSize;

                weapon = weaponIn.GetComponent<WeaponIdentification>().weapon;
                currentWeapon = weaponIn.GetComponent<WeaponIdentification>().weapon.weaponID;
                firePoint=weaponIn.GetComponent<WeaponIdentification>().FirePoint;
                id = weaponIn.GetComponent<WeaponIdentification>();
                
                yield return null;

                if (isLocalPlayer)
                {
                    if (weapon.shootMethod == ShootingMethod.Press)
                    {
                        _inputModule.BindPerformedAction("Interaction/Fire(click)", shoot);
                    }

                    newId.CmdDestroyThisObject();
                }

                break;
            }
        }
        yield return null;
    }

    void destroyCurrentWeapon()
    {
        if (id != null)
        {
            Debug.Log("delete weapon prefab");
            Destroy(id.gameObject);
        }
    }


    [Command]
    public void CmdChangeWeaponIdentification(WeaponPickable newPickUp)
    {
        //for drop weapon
        Debug.Log($"dropping weapon");
        //RpcDestroyCurrentWeapon();

        pickUp = newPickUp;
    }

    [Command]
    public void CmdDropWeapon()
    {
        RpcDestroyCurrentWeapon();
        // GameObject thePickUp=null;
        // foreach (GameObject pickup in weaponPickUpDictionary)
        // {
        //     if (pickup.GetComponent<WeaponPickable>().weapon.weaponID == weapon.weaponID)
        //     {
        //         thePickUp = pickup;
        //         break;
        //     }
        // }
        //
        // if (thePickUp != null)
        // {
        //     thePickUp = Instantiate(thePickUp);
        //     WeaponPickable pick = thePickUp.GetComponent<WeaponPickable>();
        //     pick.currentBullets = id.currentBullets;
        //     pick.currentMagazineSize = id.currentMagazineSize;
        //     Debug.Log("shfhs");
        //     NetworkServer.Spawn(thePickUp);
        // }
    }

    [ClientRpc]
    void RpcDestroyCurrentWeapon()
    {
        destroyCurrentWeapon();
    }
}
