using Mirror;
using UnityEngine;

/// <summary>
/// Attach this to your weapon object ( the one that goes in the weapon array of WeaponController )
/// </summary>
public class WeaponIdentification : MonoBehaviour
{
    [Tooltip("Attach your desired weapon scriptable object" +
           ". You can make a new one by clicking right click on your project folder, and follow the path: Create / COWSINS / New weapon. Add a name to it " +
           "and customize your weapon.")]
    public Weapon_SO weapon;

    [Tooltip("Every weapon, excluding melee, must have a firePoint, which is the point where the bullet comes from." +
        "Just make an empty object, call it firePoint for organization purposes and attach it here. ")]
    public Transform[] FirePoint;

    // [HideInInspector]
    public int totalMagazines,magazineSize,bulletsLeftInMagazine,totalBullets; // Internal use

    private void Start()
    {
        totalMagazines = weapon.totalMagazines;
        magazineSize = weapon.magazineSize;
        // GetComponent<Rigidbody>().isKinematic = true; 
    }
    
}
