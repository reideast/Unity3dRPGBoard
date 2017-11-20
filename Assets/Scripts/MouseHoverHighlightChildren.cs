using System.Collections.Generic;
using UnityEngine;

// Script is taken from Unity Docs: https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnMouseOver.html
// Modifed to work with multiple MeshRenderers on one object
public class MouseHoverHighlightChildren : MouseHoverHighlight {


    // uses MouseHoverHighlight.MouseOverColor to change colour

    //This stores the GameObject’s original color
    private List<Color> m_OriginalColorList = new List<Color>();
    //Get the GameObject’s mesh renderer to access the GameObject’s material and color
    private List<MeshRenderer> m_RendererList = new List<MeshRenderer>();

    void Start()
    {
        //Fetch the mesh renderer component from the GameObject
        GetComponentsInChildren<MeshRenderer>(m_RendererList);
        //Fetch the original color of the GameObject
        IEnumerator<MeshRenderer> i = m_RendererList.GetEnumerator();
        while (i.MoveNext()) {
            m_OriginalColorList.Add(i.Current.material.color);
        }
    }

    void OnMouseOver()
    {
        //Change the color of the GameObject to red when the mouse is over GameObject
        if (MouseHoverHighlight.isEffectActive) {
            IEnumerator<MeshRenderer> i = m_RendererList.GetEnumerator();
            while (i.MoveNext()) {
                i.Current.material.color = MouseHoverHighlight.MouseOverColor;
            }
        } else {
            IEnumerator<MeshRenderer> i = m_RendererList.GetEnumerator();
            IEnumerator<Color> c = m_OriginalColorList.GetEnumerator();
            while (i.MoveNext() && c.MoveNext()) {
                i.Current.material.color = c.Current;
            }
        }
    }

    void OnMouseExit()
    {
        //Reset the color of the GameObject back to normal
        IEnumerator<MeshRenderer> i = m_RendererList.GetEnumerator();
        IEnumerator<Color> c = m_OriginalColorList.GetEnumerator();
        while (i.MoveNext() && c.MoveNext()) {
            i.Current.material.color = c.Current;
        }
    }
}