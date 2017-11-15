using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClickMsgGameManager : MonoBehaviour {
    [HideInInspector] public int x, z;

    private void OnMouseDown() {
        GameManager.instance.SendMessage("MessageClick", new Vector2(x, z)); // Using a Vector2 to hold an X,Z because SendMessage can only handle ONE param
    }
}
