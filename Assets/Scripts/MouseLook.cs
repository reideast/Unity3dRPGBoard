using UnityEngine;

// This MouseLook script is taken directly from a Unity Answer: https://answers.unity.com/answers/1135843/view.html
// THIS IS NOT MY CODE
public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100.0f;
    public float clampAngle = 80.0f;

    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    void Start()
    {
        // Start camera a bit above the ground, and pointing at the middle (MY CODE)
        transform.position = new Vector3(30f, 10f, 30f);
        transform.LookAt(new Vector3(20f, 0f, 20f));
        // END OF MY CODE

        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        rotY += mouseX * mouseSensitivity * Time.deltaTime;
        rotX += mouseY * mouseSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        transform.rotation = localRotation;
    }
}
