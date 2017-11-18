using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenAttacker : MonoBehaviour {
    public GameObject animatedWeaponPrefab;


    public void AttackTowards(GameObject victim) {
        // Face the GO that's being attacked
        transform.LookAt(victim.transform);

        // Spawn a floating weapon, and face it the right way
        GameObject floatingWeapon = (GameObject) Instantiate(animatedWeaponPrefab);
        floatingWeapon.transform.position = transform.position; // + new Vector3(0, 0, 0.1f);
        floatingWeapon.transform.rotation = transform.rotation;

        // Weapon will animate itself, and animation contains the routine to start the particle effects
        // Weapon also takes care of removing itself
    }
}
