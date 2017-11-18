using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenAttacker : MonoBehaviour {
    public GameObject animatedWeaponPrefab;

    public void AttackTowards(GameObject victim) {
        // Face the GO that's being attacked
        Vector3 victimSpotLevelled = new Vector3(victim.transform.position.x, this.transform.position.y, victim.transform.position.z); // a point that is in the victim's X and Z, but locked to the current Y (so token doesn't tilt). See: https://answers.unity.com/answers/250578/view.html
        transform.LookAt(victimSpotLevelled); // Look at 

        // Spawn a floating weapon, and face it the right way
        Debug.Log("parent transform at=" + transform.position);
        GameObject floatingWeapon = (GameObject) Instantiate(animatedWeaponPrefab, transform);
        Debug.Log("floatingWeapon created at=" + floatingWeapon.transform.position);
        //floatingWeapon.transform.position = transform.position + new Vector3(0.5f, 1f, 0.5f);
        floatingWeapon.transform.position = transform.position + 0.5f * transform.forward + Vector3.up; // token's position + token's facing (scaled back) + 1m high
        Debug.Log("floatingWeapon moved to=" + floatingWeapon.transform.position);
        //floatingWeapon.transform.rotation = transform.rotation;
        floatingWeapon.transform.LookAt(victimSpotLevelled + Vector3.up);
        //GameObject floatingWeapon = (GameObject) Instantiate(animatedWeaponPrefab, transform.position + new Vector3(0, 1f, 0.3f), transform.rotation);

        // Weapon will animate itself, and animation contains the routine to start the particle effects
        // Weapon also takes care of removing itself
    }
}
