using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour {
    // Public GameObjects to be assigned in editor
    public GameObject OneByOnePrefab;
    public Camera Camera;
    public List<GameObject> MonsterPrefabsList;
    public List<GameObject> PlayerPrefabList;

    public GameObject MenuCanvas, InGameCanvas;
    public TMP_Text TextCurrentActor, TextHP, TextAC, TextAtkName, TextAtkRoll, TextDmgRoll, TextTurnTracker;

    [HideInInspector] public static GameManager instance;
    private GameObject player; // TODO: DEBUG
    private GameObject monster; // TODO: DEBUG

    // Data structures to support running the game
    private List<Actor> actors;
    private int currentActorTurn;
//    [HideInInspector] public static STATES state = STATES.MENU;
    [HideInInspector] public static STATES state = STATES.AWAITING_INPUT; // TODO: DEBUG
    public enum STATES { MENU, AWAITING_INPUT, ANIMATING_ACTION };

    // Predefined Scenarios
    private SceneActor[] skeletonScene;
    private SceneActor[] OTHERScene; // TODO: Make this one, too!

    // The game board, available for inspection
    // each Space object contains a reference to a OneByOne (GameObject). Use this to find actual world unit coordinates of each game space
    [HideInInspector] public Space[,] spaces;
    public GameObject SpacesHolder; // an empty GameObject to hold all the spaces. Simply to reduce clutter...doesn't improve performance, I think

    // Properties of the spaces
    public int RowsX = 60, ColsZ = 60;
    private const float DropFromHeight = 10f;
    private const float UnitWidth = 1;
    private const float Margin = 0.05f;
    private const float SpaceHeight = 0.2f;
    private Vector3 SPACE_HEIGHT_MOD;

    private const float cameraSpeed = 4;
    private const float HOP_ANIMATION_TIME = 0.5f;


    void Start () {
        GameManager.instance = this;
        SPACE_HEIGHT_MOD = new Vector3(0f, SpaceHeight, 0f);

        // Build the predefined scenarios
        skeletonScene = new SceneActor[] {
            new SceneActor(true, 0, 25, 25, new Color(0, 120f, 255f, 150f)),
            new SceneActor(false, 0, 25, 28, new Color(255f, 0, 0, 150f))
        };

        // Generate game board made of one-by-one squares
        spaces = new Space[RowsX, ColsZ];
        GenerateSquares();

        // DEBUG
        Invoke("StartScenarioSkeletons", 1);
    }

    public void OnClickStartButton() {
        StartScenarioSkeletons();
    }

    public void StartScenarioSkeletons() {
        // Reset the grid
        ResetBoard();

        // Reset the array of actors
        actors = new List<Actor>();

        // Place the scene's actors
        BuildActiveTokensFromScene(skeletonScene);

        player = actors[0].tokenRef;
        monster = actors[1].tokenRef;

        // DEBUG: only turn on when it's the player's turn
        MouseHoverHighlight.isEffectActive = true;
    }

    private void BuildActiveTokensFromScene(SceneActor[] predefinedSceneActors) {
        foreach (SceneActor actorData in predefinedSceneActors) {
            // Create GameObject and place it in the correct square
            GameObject newGameObject;
            if (actorData.IsPlayer) {
                newGameObject = (GameObject) Instantiate(instance.PlayerPrefabList[actorData.PrefabIndex]);
            } else {
                newGameObject = (GameObject) Instantiate(instance.MonsterPrefabsList[actorData.PrefabIndex]);
            }
            Space spaceToPlace = spaces[actorData.x, actorData.z];
            Vector3 squareBasis = spaceToPlace.gameSpace.transform.position;
            newGameObject.transform.position = new Vector3(squareBasis.x, DropFromHeight + UnitWidth, squareBasis.z);

            TokenStats stats = newGameObject.GetComponent<TokenStats>();
            Actor newActor = new Actor(newGameObject, actorData.IsPlayer, stats.characterName, stats.hp, stats.ac, stats.attackName, stats.attackMod, stats.dmgDieNum,
                stats.dmgDieMagnitude, stats.attackMod, actorData.ActorColor);
            Pathfind goLocation = newGameObject.GetComponent<Pathfind>();
            goLocation.x = actorData.x;
            goLocation.z = actorData.z;
            stats.x = actorData.x; // may not be necessary??
            stats.z = actorData.z;
            spaces[actorData.x, actorData.z].isBlocked = true;

            actors.Add(newActor);
        }
    }

    // Instantiate square objects, but don't make them active yet
    private void GenerateSquares() {
        for (int x = 0; x < RowsX; x += (int) UnitWidth) {
            for (int z = 0; z < ColsZ; z += (int) UnitWidth) {
                GameObject generatedSquare = null;
                if (x != 29 || z != 14) { // Don't make a square on the tree
                    //generatedSquare = (GameObject) Instantiate(instance.oneByOnePrefab, new Vector3(x + margin, dropFromHeight, z + margin), Quaternion.identity);
                    generatedSquare = (GameObject) Instantiate(instance.OneByOnePrefab, SpacesHolder.transform);
                    OnClickMsgClickedSpace script = generatedSquare.GetComponent<OnClickMsgClickedSpace>();
                    script.x = x;
                    script.z = z;
                }
                spaces[x, z] = new Space(x, z, generatedSquare);
            }
        }

        // A tree!
        spaces[29, 14].isBlocked = true;
    }

    // Place squares back in the original position for a new game scenario
    private void ResetBoard() {
        for (int x = 0; x < RowsX; x += (int) UnitWidth) {
            for (int z = 0; z < ColsZ; z += (int) UnitWidth) {
                if (x != 29 || z != 14) { // Don't make a square on the tree
                    spaces[x, z].gameSpace.transform.position = new Vector3(x + Margin, DropFromHeight, z + Margin);
                    spaces[x, z].gameSpace.SetActive(true);
                }
            }
        }
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
            ((Behaviour) token.GetComponent("Halo")).enabled = false;
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
            ((Behaviour) token.GetComponent("Halo")).enabled = true;
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
            Camera.transform.position = new Vector3(Camera.transform.position.x + deltaX, Camera.transform.position.y, Camera.transform.position.z + deltaZ);
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

    // A class to have a token's initial position and properties. Several of these will make up a prebuild scenario
    public class SceneActor {
        public bool IsPlayer; // grab GameObject from player list or monster list
        public int PrefabIndex; // which item in the list of players/monsters does this Actor refer to?
        public int x, z; // location on the grid to start the token
        public Color ActorColor;
        public SceneActor(bool isPlayer, int prefabIndex, int x, int z, Color actorColor) {
            IsPlayer = isPlayer;
            PrefabIndex = prefabIndex;
            this.x = x;
            this.z = z;
            ActorColor = actorColor;
        }
    }

    // A struct to hold an actor on the game board
    // A list of these will make up a scene
    public class Actor {
        public GameObject tokenRef;
        public bool IsPlyaer;
        public bool IsAlive = true;
        public string ActorName;
        public int HP, AC;
        public string AttackName;
        public int AttackMod, DamageDieNum, DamageDieMagnitude, DamageMod;
        public Color ActorColor; // the colour to surround this token with indicating it is the active Actor, and to use as the cursor highlight
        public Actor(GameObject tokenRef, bool isPlyaer, string actorName, int hp, int ac, string attackName, int attackMod, int damageDieNum, int damageDieMagnitude, int damageMod, Color actorColor) {
            this.tokenRef = tokenRef;
            IsPlyaer = isPlyaer;
            ActorName = actorName;
            HP = hp;
            AC = ac;
            AttackName = attackName;
            AttackMod = attackMod;
            DamageDieNum = damageDieNum;
            DamageDieMagnitude = damageDieMagnitude;
            DamageMod = damageMod;
            ActorColor = actorColor;
        }
    }
}
