using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script is taken from Unity Docs: https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnMouseOver.html
// THIS IS NOT MY CODE!
public class MouseHoverHighlightChildren : MonoBehaviour {

    //This second example changes the GameObject's color to red when the mouse hovers over it
    //Ensure the GameObject has a MeshRenderer

    //When the mouse hovers over the GameObject, it turns to this color (red)
    public Color m_MouseOverColor = Color.red;
    //This stores the GameObject’s original color
    private List<Color> m_OriginalColor = new List<Color>();
    //Get the GameObject’s mesh renderer to access the GameObject’s material and color
    private List<MeshRenderer> m_Renderer = new List<MeshRenderer>();

    void Start()
    {
        //Fetch the mesh renderer component from the GameObject
        GetComponentsInChildren<MeshRenderer>(m_Renderer);
        //Fetch the original color of the GameObject
        IEnumerator<MeshRenderer> i = m_Renderer.GetEnumerator();
        while (i.MoveNext()) {
            m_OriginalColor.Add(i.Current.material.color);
        }
    }

    void OnMouseOver()
    {
        //Change the color of the GameObject to red when the mouse is over GameObject
        if (MouseHoverHighlight.isEffectActive) {
            IEnumerator<MeshRenderer> i = m_Renderer.GetEnumerator();
            while (i.MoveNext()) {
                i.Current.material.color = m_MouseOverColor;
            }
        } else {
            IEnumerator<MeshRenderer> i = m_Renderer.GetEnumerator();
            IEnumerator<Color> c = m_OriginalColor.GetEnumerator();
            while (i.MoveNext() && c.MoveNext()) {
                i.Current.material.color = c.Current;
            }
        } 
    }

    void OnMouseExit()
    {
        //Reset the color of the GameObject back to normal
        IEnumerator<MeshRenderer> i = m_Renderer.GetEnumerator();
        IEnumerator<Color> c = m_OriginalColor.GetEnumerator();
        while (i.MoveNext() && c.MoveNext()) {
            i.Current.material.color = c.Current;
        }
    }
}