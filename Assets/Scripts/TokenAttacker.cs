using UnityEngine;

public class TokenAttacker : MonoBehaviour {
    public GameObject animatedWeaponPrefab;

    public void AttackTowards(Transform victim) {
        // Face the GO that's being attacked by creating a point that is in the victim's X and Z, but locked to the current Y (so token doesn't tilt).
        //    See: https://answers.unity.com/answers/250578/view.html
        Vector3 victimSpotLevelled = new Vector3(victim.position.x, this.transform.position.y, victim.position.z);
        transform.LookAt(victimSpotLevelled);

        // Spawn a floating weapon, and face it the right way
        GameObject floatingWeapon = (GameObject) Instantiate(animatedWeaponPrefab, transform);
        floatingWeapon.transform.position = transform.position + 0.5f * transform.forward + Vector3.up; // token's position + token's facing (scaled back) + 1m high
        floatingWeapon.transform.LookAt(victimSpotLevelled + Vector3.up);

        // Weapon will animate itself, and animation contains the routine to start the particle effects
        // Weapon also takes care of removing itself
    }
}
