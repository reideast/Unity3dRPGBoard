using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    // Public GameObjects to be assigned in editor
    public GameObject oneByOnePrefab;
    public Camera camera;
    public List<GameObject> monsterPrefabs;
    public GameObject playerPrefab;

    [HideInInspector] public static GameManager instance;
    private GameObject player;
    private GameObject monster;

    // The game board, available for inspection
    // each Space object contains a reference to a OneByOne (GameObject). Use this to find actual world unit coordinates of each game space
    [HideInInspector] public Space[,] spaces;

    // Properties of the spaces
    public int rowsX = 60, colsZ = 60;
    private readonly float dropFromHeight = 10f;
    private readonly float unitWidth = 1;
    private readonly float margin = 0.05f;
    private readonly float spaceHeight = 0.2f;
    private Vector3 SPACE_HEIGHT_MOD;

    private readonly float cameraSpeed = 4;
    private readonly float HOP_ANIMATION_TIME = 0.5f;

    void Start () {
        GameManager.instance = this;
        SPACE_HEIGHT_MOD = new Vector3(0f, spaceHeight, 0f);

        spaces = new Space[rowsX, colsZ];

        //// mess with transparency on game board squares. See: https://answers.unity.com/questions/282272/how-to-do-a-glass-cube.html
        //Renderer r = oneByOnePrefab.AddComponent<Renderer>();
        //r.material = new Material(Shader.Find("Transparent/Diffuse"));
        //r.material.color = new Color(0f, 0.2f, 1f, 0.5f); // 50% alpha with a blue-green colour

        // Create the grid
        MakeBoard();

        // Place some playing objects
        PlaceTokens();

        // DEBUG: only turn on when it's the player's turn
        MouseOverSquareEffect.isEffectActive = true;
    }

    //private void OnDrawGizmos() {
    //    Debug.Log("gizmos!");
    //}

    private void MakeBoard()
    {
        for (int x = 0; x < rowsX; x += (int) unitWidth) {
            for (int z = 0; z < colsZ; z += (int) unitWidth) {
                GameObject generatedSquare = (GameObject) Instantiate(instance.oneByOnePrefab);
                generatedSquare.transform.position = new Vector3(x + margin, dropFromHeight, z + margin);
                //Debug.Log("Made a square=" + generatedSquare);
                spaces[x, z] = new Space(x, z, generatedSquare);
                OnClickMsgGameManager script = generatedSquare.GetComponent<OnClickMsgGameManager>();
                script.x = x;
                script.z = z;
            }
        }
    }

    private void PlaceTokens() {
        player = (GameObject) Instantiate(instance.playerPrefab);
        Space spaceToPlace = spaces[20, 20];
        Vector3 squareBasis = spaceToPlace.gameSpace.transform.position;
        player.transform.position = new Vector3(squareBasis.x, dropFromHeight + unitWidth, squareBasis.z);
        Pathfind goLocation = player.GetComponent<Pathfind>();
        goLocation.x = 20;
        goLocation.z = 20;

        monster = (GameObject) Instantiate(instance.monsterPrefabs[0]);
        spaceToPlace = spaces[25, 28];
        squareBasis = spaceToPlace.gameSpace.transform.position;
        monster.transform.position = new Vector3(squareBasis.x, dropFromHeight + unitWidth, squareBasis.z);
        goLocation = monster.GetComponent<Pathfind>();
        goLocation.x = 25;
        goLocation.z = 28;
    }


    // Recevied from any arbitrary GameObject with the OnClick-Message script attached
    public void MessageClick(Vector2 coord) { 
        Debug.Log("Clicked " + coord.x + "," + coord.y);

        WalkToken(player, (int) coord.x, (int) coord.y); // TODO: player is for DEBUG
    }
    // Walk a player or monster token to another token
    private void WalkToken(GameObject token, int xTo, int zTo) {
        //// make a queue of sqaures to hop over
        //hopsQueue = new LinkedList<Hop>();
        //// TODO: DEBUG: testing four hops
        //hopsQueue.AddLast(new Hop(20, 20, 21, 20));
        //hopsQueue.AddLast(new Hop(21, 20, 22, 20));
        //hopsQueue.AddLast(new Hop(22, 20, 23, 20));
        //hopsQueue.AddLast(new Hop(23, 20, 23, 21));
        //hopsQueue.AddLast(new Hop(23, 21, 23, 22));
        //hopsQueue.AddLast(new Hop(23, 22, 24, 23));
        //hopsQueue.AddLast(new Hop(24, 23, 25, 24));

        Pathfind tokenPositionScript = token.GetComponent<Pathfind>();
        int xFrom = tokenPositionScript.x, zFrom = tokenPositionScript.z;
        hopsQueue = tokenPositionScript.findPath(xFrom, zFrom, xTo, zTo);

        // start the hopping at the first one. will continue until hopsQueue is empty
        MouseOverSquareEffect.isEffectActive = false;
        NextHopToken(token);

        // change the token's stored position to its final position
        tokenPositionScript.x = xTo;
        tokenPositionScript.z = zTo;
    }
    public class Hop {
        public int xFrom, zFrom, xTo, zTo;
        public Hop(int xFrom, int zFrom, int xTo, int zTo) { this.xFrom = xFrom; this.zFrom = zFrom; this.xTo = xTo; this.zTo = zTo; }
    }
    private LinkedList<Hop> hopsQueue;
    // Hop from one space to another space (probably right next to it)
    private void NextHopToken(GameObject token) {
        // Pop first hop off the queue
        if (hopsQueue.First != null) {
            // Get next hop out of queue
            Hop nextHop = hopsQueue.First.Value;
            hopsQueue.RemoveFirst(); // pop
            // Set up global variables for next hop. (Requried to be global to use InvokeRepeating to loop through an animation.)
            startPos = spaces[nextHop.xFrom, nextHop.zFrom].gameSpace.transform.position + SPACE_HEIGHT_MOD * 2;
            endPos = spaces[nextHop.xTo, nextHop.zTo].gameSpace.transform.position + SPACE_HEIGHT_MOD * 2;
            center = (startPos + endPos) * 0.5f;
            center -= new Vector3(0, 0.1f, 0); // make circular movment a bit flatter (also, this line is necessary to have the arc be along the Y plane)
            relativeStartPos = startPos - center;
            relativeEndPos = endPos - center;
            tokenToAnimate = token;
            startTime = Time.time;
            endTime = startTime + HOP_ANIMATION_TIME;
            InvokeRepeating("HopLoop", 0f, 0.01f);
        } else {
            // TODO: DONE WITH HOPPING!
            Debug.Log("DEBUG: Done with hopping!");
            MouseOverSquareEffect.isEffectActive = true;
        }
    }
    private GameObject tokenToAnimate;
    private Vector3 startPos, endPos, relativeStartPos, relativeEndPos, center;
    private float startTime, endTime;

    private void HopLoop() {
        if (Time.time < endTime) {
            // Using Slerp to make an arc of movement is from unity manual: https://docs.unity3d.com/ScriptReference/Vector3.Slerp.html
            //     and this post: https://answers.unity.com/questions/11184/moving-player-in-an-arc-from-startpoint-to-endpoin.html
            tokenToAnimate.transform.position = Vector3.Slerp(relativeStartPos, relativeEndPos, (Time.time - startTime) / (endTime - startTime));
            tokenToAnimate.transform.position += center;
        } else {
            //tokenToAnimate.transform.position = endPos;
            //Debug.Log("At endPos: " + tokenToAnimate.transform.position);
            //tokenToAnimate.transform.position = tokenToAnimate.transform.position + (new Vector3(0, 2f, 0));
            //Debug.Log("after add (0,2,0): " + tokenToAnimate.transform.position);
            //HopToken
            CancelInvoke("HopLoop");
            NextHopToken(tokenToAnimate);
        }
    }

    // Update is called once per frame
    void Update () {
        float deltaX = 0f, deltaZ = 0f;

        // Move the camera along the diagonals
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

    // A struct to hold information about the game board spaces
    public class Space {
        public GameObject gameSpace; // public reference to the OneByOne GameObject pointed to by this space
        public int x, z; // public reference to this object's position in the grid
        public bool isBlocked = false; // Define if this space is impassible
        public Space(int x, int z, GameObject gameSpace) {
            this.gameSpace = gameSpace;
            this.x = x;
            this.z = z;
        }
    }
}
