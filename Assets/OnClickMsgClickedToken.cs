using UnityEngine;
using UnityEngine.EventSystems;

public class OnClickMsgClickedToken : MonoBehaviour {
    [HideInInspector] public int x, z;

    private void OnMouseDown() {
        if (!EventSystem.current.IsPointerOverGameObject() && MouseHoverHighlight.isEffectActive) {
            //GameManager.instance.SendMessage("MessageClickedToken", GetComponent<TokenStats>());
            GameManager.instance.SendMessage("MessageClickedToken", this.gameObject);
        }
    }
}
