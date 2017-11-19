using UnityEngine;

// Method of creating a popup text is by GameGrind on: https://www.youtube.com/watch?v=fbUOG7f3jq8
public class PopupTextController : MonoBehaviour {
    private static PopupText popupText;
    private static GameObject canvas;

    public static void Initialize() {
        popupText = Resources.Load<PopupText>("PopupTextParent");
        canvas = GameObject.Find("InGameCanvas");
    }

    public static void PopupText(string text, Transform attachTo) {
        PopupText textGameObject = Instantiate(popupText);
        textGameObject.SetText(text);

        // Set position
        textGameObject.transform.SetParent(canvas.transform, false);
        // Map 3d position of transform we are attaching to into flat camera/screen position
        textGameObject.transform.position = (Vector2) Camera.main.WorldToScreenPoint(attachTo.position);
    }
}
