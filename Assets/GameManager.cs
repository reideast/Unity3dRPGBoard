using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    // Public GameObjects to be assigned in editor
    public GameObject oneByOnePrefab;
    public Camera camera;

    public static GameManager instance;

	// Use this for initialization
	void Start () {
        GameManager.instance = this;

        //// mess with transparency on game board squares. See: https://answers.unity.com/questions/282272/how-to-do-a-glass-cube.html
        //Renderer r = oneByOnePrefab.AddComponent<Renderer>();
        //r.material = new Material(Shader.Find("Transparent/Diffuse"));
        //r.material.color = new Color(0f, 0.2f, 1f, 0.5f); // 50% alpha with a blue-green colour

        // Set camera a bit above the ground, and pointing at the middle
        camera.transform.position = new Vector3(5f, 8f, 5f);
        camera.transform.LookAt(new Vector3(0f, 5f, 0f));

        // Create the grid
        MakeBoard();
    }

    private void MakeBoard()
    {
        // Just messing around for now
        MakeTestingBoard();
    }

    private void MakeTestingBoard()
    {
        float dropFromHeight = 10f;
        float unitWidth = 1;
        float margin = 0.10f;
        float startX = -30f, endX = 30f,
            startZ = -30f, endZ = 30f;
        for (float x = startX; x <= endX; x += unitWidth) {
            for (float z = startZ; z <= endZ; z += unitWidth) {
                GameObject generatedSquare = (GameObject) Instantiate(instance.oneByOnePrefab);
                generatedSquare.transform.position = new Vector3(x + margin, dropFromHeight, z + margin);
                //Debug.Log("Made a square=" + generatedSquare);
            }
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
