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

    private InputModule _inputModule;

    [SerializeField]
    private bool weaponChanging;


    // Start is called before the first frame update
    void Start()
    {

    }
    

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        if (id != null)
        {
            HandleUI();
        }
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer) return;
    }

    public override void OnStartLocalPlayer()
    {
        canShoot = true;
    }

    public override void OnStartServer()
    {
        weaponChanging = false;
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

    
    private void shoot()
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
            id.bulletsLeftInMagazine -= weapon.ammoCostPerFire;
            Debug.Log($"current bullets: {id.bulletsLeftInMagazine}; bullet cost: {weapon.ammoCostPerFire}");
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
                        foreach (var p in firePoint)
                        {
                            if(weapon.muzzleVFX != null)
                            //Instantiate(weapon.muzzleVFX, p.position, mainCamera.transform.rotation, mainCamera.transform); // VFX
                            Instantiate(weapon.muzzleVFX, p.position, weapon.muzzleVFX.transform.rotation, mainCamera.transform); // VFX
                        }
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
                        CmdCreatGunFireEffect();
                        yield return null;
                        CreatGunFireEffect();
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
    void CmdCreatGunFireEffect()
    {
        RpcCreatGunFireEffect();
    }

    [ClientRpc]
    void RpcCreatGunFireEffect()
    {
        if (isLocalPlayer) return;
        CreatGunFireEffect();
    }

    void CreatGunFireEffect()
    {
        foreach (var p in firePoint)
        {
            if (weapon.muzzleVFX != null)
            {
                //Instantiate(weapon.muzzleVFX, p.position, mainCamera.transform.rotation, mainCamera.transform); // VFX
                GameObject effect = Instantiate(
                    weapon.muzzleVFX, 
                    p.position, 
                    mainCamera.transform.rotation,
                    mainCamera.transform); // VFX

                effect.transform.localEulerAngles += weapon.muzzleVFX.transform.localEulerAngles;
                Destroy(effect, effect.GetComponent<ParticleSystem>().main.duration);
            }
        }
    }

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

            if(weapon.explosionOnHit)
                bullet.explosionVFX = weapon.explosionVFX;

            bullet.hurtsPlayer = weapon.hurtsPlayer;
            bullet.explosionOnHit = weapon.explosionOnHit;
            bullet.explosionRadius = weapon.explosionRadius;
            bullet.explosionForce = weapon.explosionForce;

            bullet.criticalMultiplier = weapon.criticalDamageMultiplier; 
            bullet.destination = destination;
            bullet.player = this.transform;
            bullet.speed = weapon.speed;
            bullet.onHitEffect = weapon.onHitEffect;
            bullet.GetComponent<Rigidbody>().isKinematic = (!weapon.projectileUsesGravity) ? true : false; 
            bullet.damage = weapon.damagePerBullet;
            bullet.duration = weapon.bulletDuration; 
            
            NetworkServer.Spawn(bullet.gameObject);
        }
    }

    [ServerCallback]
    private void Hit(int gameObjectLayer, float damage, RaycastHit h, bool damageTarget)
    {
        RpcCreateHitEffect(gameObjectLayer,h);
        // Apply damage
        if (!damageTarget) return;
        if(h.collider.gameObject.tag == "Critical") h.collider.transform.parent.GetComponent<IDamageable>().Damage(damage * weapon.criticalDamageMultiplier * GetDistanceDamageReduction(h.collider.transform));
        else if (h.collider.GetComponent<IDamageable>() != null) h.collider.GetComponent<IDamageable>().Damage(damage * GetDistanceDamageReduction(h.collider.transform));
    }

    [ClientRpc]
    void RpcCreateHitEffect(int gameObjectLayer, RaycastHit h)
    {
        events.OnHit.Invoke(); 
        GameObject impact = null, impactBullet = null;

        switch (gameObjectLayer)
        {
            case 10:
                // impact = Instantiate(effects.grassImpact, h.point, Quaternion.identity); // Grass
                // impact.transform.rotation = Quaternion.LookRotation(h.normal);
                if(weapon != null)
                impactBullet = Instantiate(weapon.bulletHoleImpact.grassImpact, h.point, Quaternion.identity);
                break;
            case 11:
                // impact = Instantiate(effects.metalImpact, h.point, Quaternion.identity); // Metal
                // impact.transform.rotation = Quaternion.LookRotation(h.normal);
                if (weapon != null) impactBullet = Instantiate(weapon.bulletHoleImpact.metalImpact, h.point, Quaternion.identity);
                break;
            case 12:
                // impact = Instantiate(effects.mudImpact, h.point, Quaternion.identity); // Mud
                // impact.transform.rotation = Quaternion.LookRotation(h.normal);
                if (weapon != null) impactBullet = Instantiate(weapon.bulletHoleImpact.mudImpact, h.point, Quaternion.identity);
                break;
            case 13:
                // impact = Instantiate(effects.woodImpact, h.point, Quaternion.identity); // Wood
                // impact.transform.rotation = Quaternion.LookRotation(h.normal);
                if (weapon != null) impactBullet = Instantiate(weapon.bulletHoleImpact.woodImpact, h.point, Quaternion.identity);
                break;
            case 7:
                // impact = Instantiate(effects.enemyImpact, h.point, Quaternion.identity); // Enemy
                // impact.transform.rotation = Quaternion.LookRotation(h.normal);
                if (weapon != null) impactBullet = Instantiate(weapon.bulletHoleImpact.enemyImpact, h.point, Quaternion.identity);
                break;
        }

        if (h.collider != null && impactBullet != null)
        {
            impactBullet.transform.rotation = Quaternion.LookRotation(h.normal);
            impactBullet.transform.SetParent(h.collider.transform);
        }
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
        
        //delete last weapon
        destroyCurrentWeapon();
        yield return null;
        
        if (newId == null)
        {
            Debug.LogWarning($"id is null");
            yield break;
        }

        //for swapping
        Debug.Log($"Swapping weapon {newId.weapon.weaponID}");
        foreach (GameObject weaponPrefab in weapons)
        {
            if (weaponPrefab.GetComponent<WeaponIdentification>().weapon.weaponID == newId.weapon.weaponID)
            {
                Debug.Log($"found weapon {newId.weapon.weaponID}");
                GameObject weaponIn=Instantiate(weaponPrefab,weaponHolder.position,weaponHolder.rotation,weaponHolder);
                        
                //weaponIn.GetComponent<WeaponIdentification>().totalMagazines = pickUp.totalMagazines;
                weaponIn.GetComponent<WeaponIdentification>().totalBullets = newId.totalBullets;
                //weaponIn.GetComponent<WeaponIdentification>().magazineSize = pickUp.magazineSize;
                weaponIn.GetComponent<WeaponIdentification>().bulletsLeftInMagazine = newId.bulletsLeftInMagazine;

                weapon = weaponIn.GetComponent<WeaponIdentification>().weapon;
                currentWeapon = weaponIn.GetComponent<WeaponIdentification>().weapon.weaponID;
                firePoint=weaponIn.GetComponent<WeaponIdentification>().FirePoint;
                id = weaponIn.GetComponent<WeaponIdentification>();
                
                yield return null;

                if (isLocalPlayer)
                {
                    weaponIn.GetComponent<WeaponState>().IniInput();
                    weaponIn.GetComponent<WeaponState>().fire += shoot;
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
            Debug.Log("delete current weapon prefab");
            Destroy(id.gameObject);
            weapon = null;
            firePoint = null;
        }
    }

    public void dropCurrentWeapon()
    {
        CmdDropCurrentWeapon();
    }

    [Command]
    private void CmdDropCurrentWeapon()
    {
        if (weapon == null) return;
        StartCoroutine(dropWeapon());
        
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeWeaponIdentification(WeaponPickable newPickUp)
    {
        if (!weaponChanging)
        {
            StartCoroutine(pickUpWeapon(newPickUp));
        }
    }

    //[Command(requiresAuthority = false)]
    [TargetRpc]
    void TargetSpawnCurrentWeapon()
    {
        if (weapon == null)
        {
            CmdSetWeaponChangingState(false);
            return;
        }
        CmdSpawnWeaponInServer(weapon.weaponID,id.bulletsLeftInMagazine,id.totalBullets);
    }

    [Command]
    private void CmdSetWeaponChangingState(bool state)
    {
        weaponChanging = state;
    }

    [Command]
    private void CmdSpawnWeaponInServer(int weaponId,int bulletLet,int totalBullet)
    {
        if (weapon == null)
        {
            weaponChanging = false;
            return;
        }
        
        GameObject thePickUp=null;
        foreach (GameObject pickup in weaponPickUpDictionary)
        {
            if (pickup.GetComponent<WeaponPickable>().weapon.weaponID == weaponId)
            {
                thePickUp = pickup;
                break;
            }
        }
        
        if (thePickUp != null)
        {
            thePickUp = Instantiate(thePickUp,weaponHolder.position,transform.rotation);
            WeaponPickable pick = thePickUp.GetComponent<WeaponPickable>();
            pick.bulletsLeftInMagazine = bulletLet;
            pick.totalBullets = totalBullet;
            pick.allowToDestroy = true;
            NetworkServer.Spawn(thePickUp);

            weaponChanging = false;
        }
    }

    [ClientRpc]
    void RpcDestroyCurrentWeapon()
    {
        if(id==null || weapon==null)return;
        destroyCurrentWeapon();
    }
    
    [ServerCallback]
    private IEnumerator dropWeapon()
    {
        if(weapon==null)yield break;
        
        weaponChanging = true;
        yield return null;
        
        TargetSpawnCurrentWeapon();
        
        int timeCd = 60 * 2;
        while (weaponChanging && timeCd>0)
        {
            timeCd-= 1;
            yield return new FixedUpdate();
        }

        if (weaponChanging)
        {
            weaponChanging = false;
            yield break;
        }
        
        yield return null;
        pickUp = null;
    }
    
    [ServerCallback]
    private IEnumerator pickUpWeapon(WeaponPickable newPickUp)
    {
        weaponChanging = true;
        yield return new FixedUpdate();

        TargetSpawnCurrentWeapon();

        int timeCd = 60 * 2;
        while (weaponChanging && timeCd>0)
        {
            timeCd-= 1;
            yield return new FixedUpdate();
        }

        if (weaponChanging)
        {
            weaponChanging = false;
            yield break;
        }
        
        yield return null;
        pickUp = newPickUp;
    }

    private void HandleUI()
    {
        
    }
}
