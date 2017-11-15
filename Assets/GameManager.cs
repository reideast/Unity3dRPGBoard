using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    private class Space {
        public GameObject gameSpace; // public reference to the OneByOne GameObject pointed to by this space
        public int x, z; // public reference to this object's position in the grid
        public Space(int x, int z, GameObject gameSpace) {
            this.gameSpace = gameSpace;
            this.x = x;
            this.z = z;
        }
    }

    // Public GameObjects to be assigned in editor
    public GameObject oneByOnePrefab;
    public Camera camera;
    public List<GameObject> monsterPrefabs;
    public GameObject playerPrefab;

    public static GameManager instance;

    public int startX, endX, startZ, endZ;
    private Space[,] spaces;
    public float dropFromHeight = 10f;
    public float unitWidth = 1;
    public float margin = 0.10f;

	// Use this for initialization
	void Start () {
        GameManager.instance = this;

        spaces = new Space[endX - startX, endZ - startZ];

        //// mess with transparency on game board squares. See: https://answers.unity.com/questions/282272/how-to-do-a-glass-cube.html
        //Renderer r = oneByOnePrefab.AddComponent<Renderer>();
        //r.material = new Material(Shader.Find("Transparent/Diffuse"));
        //r.material.color = new Color(0f, 0.2f, 1f, 0.5f); // 50% alpha with a blue-green colour

        // Set camera a bit above the ground, and pointing at the middle
        camera.transform.position = new Vector3((endX - startX) / 2f, 8f, (endZ - startZ) / 2f);
        camera.transform.LookAt(new Vector3(0f, 5f, 0f));

        // Create the grid
        MakeBoard();

        // Place some playing objects
        PlaceTokens();
    }

    //private void OnDrawGizmos() {
    //    Debug.Log("gizmos!");
    //}

    private void MakeBoard()
    {
        for (int x = startX; x < endX; x += (int) unitWidth) {
            for (int z = startZ; z < endZ; z += (int) unitWidth) {
                GameObject generatedSquare = (GameObject) Instantiate(instance.oneByOnePrefab);
                generatedSquare.transform.position = new Vector3(x + margin, dropFromHeight, z + margin);
                //Debug.Log("Made a square=" + generatedSquare);
                spaces[x, z] = new Space(x, z, generatedSquare);
            }
        }
    }

    private void PlaceTokens() {
        GameObject generatedPlayer = (GameObject) Instantiate(instance.playerPrefab);
        Space spaceToPlace = spaces[3, 4];
        Vector3 squareBasis = spaceToPlace.gameSpace.transform.position;
        generatedPlayer.transform.position = new Vector3(squareBasis.x, dropFromHeight + unitWidth, squareBasis.z);

        GameObject generatedMonster = (GameObject) Instantiate(instance.monsterPrefabs[0]);
        spaceToPlace = spaces[5, 8];
        squareBasis = spaceToPlace.gameSpace.transform.position;
        generatedMonster.transform.position = new Vector3(squareBasis.x, dropFromHeight + unitWidth, squareBasis.z);
    }

    // Update is called once per frame
    void Update () {
        float cameraSpeed = 4;
        float deltaX = 0f, deltaZ = 0f;

        if (Input.GetKey(KeyCode.A)) {
            deltaX += cameraSpeed * Time.deltaTime;
            deltaZ -= cameraSpeed * Time.deltaTime;
        } else if (Input.GetKey(KeyCode.D)) {
            deltaX -= cameraSpeed * Time.deltaTime;
            deltaZ += cameraSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.W)) {
            deltaX -= cameraSpeed * Time.deltaTime;
            deltaZ -= cameraSpeed * Time.deltaTime;
        } else if (Input.GetKey(KeyCode.S)) {
            deltaX += cameraSpeed * Time.deltaTime;
            deltaZ += cameraSpeed * Time.deltaTime;
        }
        if (deltaX != 0f || deltaZ != 0f) {
            camera.transform.position = new Vector3(camera.transform.position.x + deltaX, camera.transform.position.y, camera.transform.position.z + deltaZ);
        }
	}
}
