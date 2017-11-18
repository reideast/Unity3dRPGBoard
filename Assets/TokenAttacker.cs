using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenAttacker : MonoBehaviour {
    public GameObject animatedWeaponPrefab;


    public void AttackTowards(GameObject victim) {
        // Face the GO that's being attacked
        transform.LookAt(new Vector3(victim.transform.position.x, this.transform.position.y, victim.transform.position.z)); // Look at a point that is in the victim's X and Z, but locked to the current Y (so token doesn't tilt). See: https://answers.unity.com/answers/250578/view.html

        // Spawn a floating weapon, and face it the right way
        GameObject floatingWeapon = (GameObject) Instantiate(animatedWeaponPrefab);
        floatingWeapon.transform.position = transform.position + new Vector3(0, 1f, 0.3f);
        //floatingWeapon.transform.rotation = transform.rotation;
        //GameObject floatingWeapon = (GameObject) Instantiate(animatedWeaponPrefab, transform.position + new Vector3(0, 1f, 0.3f), transform.rotation);
        floatingWeapon.transform.LookAt(victim.transform);

        // Weapon will animate itself, and animation contains the routine to start the particle effects
        // Weapon also takes care of removing itself
    }
}
