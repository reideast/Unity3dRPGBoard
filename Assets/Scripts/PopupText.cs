using UnityEngine;
using UnityEngine.UI;

// Method of creating a popup text is by GameGrind on: https://www.youtube.com/watch?v=fbUOG7f3jq8
public class PopupText : MonoBehaviour {
    public Text textObject;

    private void OnEnable() {
        Destroy(gameObject, 0.8f);
    }

    public void SetText(string text) {
        textObject.text = text;
    }
}
