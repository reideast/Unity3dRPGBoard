using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSelfActions : MonoBehaviour {

    private void AttackEffects() {
        ParticleSystem effects = GetComponentInChildren<ParticleSystem>();
        if (!effects.isPlaying) {
            effects.Play(); // Not looped, so no need to Stop()
        } else {
            Debug.Log("ERROR: Effects were already plaing!");
        }
    }

    private void RemoveWeapon() {
        Destroy(this.gameObject);
    }
}
