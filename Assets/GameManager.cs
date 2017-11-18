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
        MouseHoverHighlight.isEffectActive = true;
    }

    //private void OnDrawGizmos() {
    //    Debug.Log("gizmos!");
    //}

    private void MakeBoard()
    {
        for (int x = 0; x < rowsX; x += (int) unitWidth) {
            for (int z = 0; z < colsZ; z += (int) unitWidth) {
                GameObject generatedSquare = null;
                if (x != 29 || z != 14) { // Don't make a square on the tree
                    generatedSquare = (GameObject) Instantiate(instance.oneByOnePrefab);
                    generatedSquare.transform.position = new Vector3(x + margin, dropFromHeight, z + margin);
                    OnClickMsgClickedSpace script = generatedSquare.GetComponent<OnClickMsgClickedSpace>();
                    script.x = x;
                    script.z = z;
                }
                spaces[x, z] = new Space(x, z, generatedSquare);
            }
        }
    }

    private void PlaceTokens() {
        player = (GameObject) Instantiate(instance.playerPrefab);
        Space spaceToPlace = spaces[25, 23];
        Vector3 squareBasis = spaceToPlace.gameSpace.transform.position;
        player.transform.position = new Vector3(squareBasis.x, dropFromHeight + unitWidth, squareBasis.z);
        Pathfind goLocation = player.GetComponent<Pathfind>();
        goLocation.x = 25;
        goLocation.z = 23;
        TokenStats tokenStats = player.GetComponent<TokenStats>();
        tokenStats.x = 25;
        tokenStats.z = 23;
        spaces[25, 23].isBlocked = true;

        monster = (GameObject) Instantiate(instance.monsterPrefabs[0]);
        spaceToPlace = spaces[25, 28];
        squareBasis = spaceToPlace.gameSpace.transform.position;
        monster.transform.position = new Vector3(squareBasis.x, dropFromHeight + unitWidth, squareBasis.z);
        goLocation = monster.GetComponent<Pathfind>();
        goLocation.x = 25;
        goLocation.z = 28;
        tokenStats = monster.GetComponent<TokenStats>();
        tokenStats.x = 25;
        tokenStats.z = 28;
        spaces[25, 28].isBlocked = true;

        // A tree!
        spaces[29, 14].isBlocked = true;
    }

    // Recevied from any arbitrary GameObject with the OnClick-Message script attached
    //public void MessageClickedToken(TokenStats ts) {
    public void MessageClickedToken(GameObject goClicked) {
        //WalkToken(player, ts.x, ts.z);
        Debug.Log("trying to attack this GO:");
        Debug.Log(goClicked);
        player.GetComponent<TokenAttacker>().AttackTowards(goClicked);
    }

    // Recevied from any arbitrary GameObject with the OnClick-Message script attached
    public void MessageClickedSpace(Vector2 coord) {
        WalkToken(player, (int) coord.x, (int) coord.y); // TODO: player is for DEBUG
    }
    // Walk a player or monster token to another token
    private void WalkToken(GameObject token, int xTo, int zTo) {
        // Find a path to the desired square, by getting a queue of sqaures to hop over
        Pathfind tokenPositionScript = token.GetComponent<Pathfind>();
        hopsQueue = tokenPositionScript.findPath(tokenPositionScript.x, tokenPositionScript.z, xTo, zTo);

        if (hopsQueue != null) {
            // change the token's stored properties to its final position
            spaces[tokenPositionScript.x, tokenPositionScript.z].isBlocked = false;
            tokenPositionScript.x = xTo;
            tokenPositionScript.z = zTo;
            spaces[xTo, zTo].isBlocked = true;

            Debug.Log("Hops left=" + hopsQueue.Count);

            // start the hopping at the first one. will continue until hopsQueue is empty
            MouseHoverHighlight.isEffectActive = false;
            NextHopToken(token);
        } else {
            Debug.Log("No path to walk. Pathfinding failed");
        }
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
            // Set up global variables for next hop. (Requried to be global to use InvokeRepeating to loop through the animation.)
            startPos = spaces[nextHop.xFrom, nextHop.zFrom].gameSpace.transform.position + SPACE_HEIGHT_MOD * 2;
            endPos = spaces[nextHop.xTo, nextHop.zTo].gameSpace.transform.position + SPACE_HEIGHT_MOD * 2;
            center = (startPos + endPos) * 0.5f;
            center -= new Vector3(0, 0.1f, 0); // make circular movment a bit flatter (also, this line is necessary to have the arc be along the Y plane)
            relativeStartPos = startPos - center;
            relativeEndPos = endPos - center;
            tokenToAnimate = token;
            startTime = Time.time;
            endTime = startTime + HOP_ANIMATION_TIME;
            Debug.Log("Hopping from (" + nextHop.xFrom + "," + nextHop.zFrom + ") to (" + nextHop.xTo + "," + nextHop.zTo + "). Hops still in queue =" + hopsQueue.Count);
            InvokeRepeating("HopLoop", 0f, 0.01f);
        } else {
            // TODO: DONE WITH HOPPING!
            Debug.Log("DEBUG: Done with hopping!");
            MouseHoverHighlight.isEffectActive = true;
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
