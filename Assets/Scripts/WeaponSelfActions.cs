using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSelfActions : MonoBehaviour {
    // Fire off attack effects. (ParticleSystem should not be playing on start)
    // This function will be assigned to an Animation Event
    private void AttackEffects() {
        GetComponentInChildren<ParticleSystem>().Play(); // Not looped, so no need to Stop()
    }

    // Delete the parent GO
    // This function will be assigned to an Animation Event
    private void RemoveWeapon() {
        Destroy(transform.parent.gameObject);
    }
}
