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

        // Set camera a bit above the ground, and pointing at the middle
        camera.transform.position = new Vector3(5f, 3f, 5f);
        camera.transform.LookAt(Vector3.zero);

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
        float unitWidth = 1;
        float margin = 0.05f;
        float startX = -10f, endX = 10f,
            startZ = -10f, endZ = 10f;
        for (float x = startX; x <= endX; x += unitWidth) {
            for (float z = startZ; z <= endZ; z += unitWidth) {
                GameObject generatedSquare = (GameObject) Instantiate(instance.oneByOnePrefab);
                generatedSquare.transform.position = new Vector3(x + margin, 0f, z + margin);
                Debug.Log("Made a square=" + generatedSquare);
            }
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
