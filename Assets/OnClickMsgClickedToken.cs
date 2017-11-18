using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClickMsgClickedToken : MonoBehaviour {
    [HideInInspector] public int x, z;

    private void OnMouseDown() {
        if (MouseHoverHighlight.isEffectActive) {
            GameManager.instance.SendMessage("MessageClickedToken", GetComponent<TokenStats>()); // Using a Vector2 to hold an X,Z because SendMessage can only handle ONE param
        }
    }
}
