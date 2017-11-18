using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClickMsgClickedToken : MonoBehaviour {
    [HideInInspector] public int x, z;

    private void OnMouseDown() {
        if (MouseHoverHighlight.isEffectActive) {
            //GameManager.instance.SendMessage("MessageClickedToken", GetComponent<TokenStats>());
            GameManager.instance.SendMessage("MessageClickedToken", this.gameObject);
        }
    }
}
