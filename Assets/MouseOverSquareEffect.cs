using UnityEngine;

// Script is taken from Unity Docs: https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnMouseOver.html
// THIS IS NOT MY CODE!
public class MouseOverSquareEffect : MonoBehaviour {

    public static bool isEffectActive = false;

    //This second example changes the GameObject's color to red when the mouse hovers over it
    //Ensure the GameObject has a MeshRenderer

    //When the mouse hovers over the GameObject, it turns to this color (red)
    Color m_MouseOverColor = Color.red;
    //This stores the GameObject’s original color
    Color m_OriginalColor;
    //Get the GameObject’s mesh renderer to access the GameObject’s material and color
    MeshRenderer m_Renderer;

    void Start()
    {
        //Fetch the mesh renderer component from the GameObject
        m_Renderer = GetComponent<MeshRenderer>();
        //Fetch the original color of the GameObject
        m_OriginalColor = m_Renderer.material.color;
    }

    void OnMouseOver()
    {
        //Change the color of the GameObject to red when the mouse is over GameObject
        if (isEffectActive) {
            m_Renderer.material.color = m_MouseOverColor;
        } else {
            m_Renderer.material.color = m_OriginalColor;
        } 
    }

    void OnMouseExit()
    {
        //Reset the color of the GameObject back to normal
        m_Renderer.material.color = m_OriginalColor;
    }
}