using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEditor;

public class GameManager : MonoBehaviour {
    // Public GameObjects to be assigned in editor
    public GameObject OneByOnePrefab;

    public Camera Camera;
    public List<GameObject> MonsterPrefabsList;
    public List<GameObject> PlayerPrefabList;

    public GameObject MenuCanvas, InGameCanvas, PlayersWinMessage, MonstersWinMessage;
    public TMP_Text TextCurrentActor, TextHP, TextAC, TextAtkName, TextAtkRoll, TextDmgRoll, TextSpeedLeft, TextTurnTracker;

    [HideInInspector] public static GameManager instance;

    // Data structures to support running the game
    private List<Actor> actors;
    private int currentActorTurn;
    public Turn currentTurnStats;
    private int playerCount, monsterCount;
    [HideInInspector] public static STATES state = STATES.MENU;

    public enum STATES {
        MENU,
        AWAITING_INPUT,
        ANIMATING_ACTION
    };

    // Predefined Scenarios
    private SceneActor[] undeadScene;
    private SceneActor[] koboldScene;

    // The game board, available for inspection
    // each Space object contains a reference to a OneByOne (GameObject). Use this to find actual world unit coordinates of each game space
    [HideInInspector] public Space[,] spaces;

    public GameObject SpacesHolder; // an empty GameObject to hold all the spaces. Simply to reduce clutter...doesn't improve performance, I think

    // Properties of the spaces
    public int RowsX = 60, ColsZ = 60;

    private const float DropFromHeight = 10f;
    private const float Margin = 0.05f;
    private const float SpaceHeight = 0.2f;
    public Vector3 SPACE_HEIGHT_MOD;

    private const float cameraSpeed = 4;


    void Start() {
        GameManager.instance = this;
        SPACE_HEIGHT_MOD = new Vector3(0f, SpaceHeight, 0f);
        PopupTextController.Initialize();

        // Build the predefined scenarios
        undeadScene = new SceneActor[] {
            new SceneActor(true, 0, 25, 17, new Color(0, 0.47f, 1f, 0.58f)), // Paladin
            new SceneActor(false, 0, 28, 27, new Color(1f, 0, 0, 0.58f)), // Skeleton
            new SceneActor(false, 0, 13, 30, new Color(1f, 0.5f, 0, 0.58f)),
            new SceneActor(false, 0, 20, 27, new Color(1f, 0.75f, 0, 0.58f)),
            new SceneActor(false, 1, 17, 29, new Color(0.5f, 0.75f, 0.5f, 0.58f)) // Zombie
        };
        koboldScene = new SceneActor[] {
            new SceneActor(true, 0, 25, 17, new Color(0, 0.47f, 1f, 0.58f)) // Paladin
        };

        // Generate game board made of one-by-one squares
        spaces = new Space[RowsX, ColsZ];
        GenerateSquares();
//        ResetBoard();
    }
    public void SetState(STATES newSate) {
        state = newSate;
        if (newSate == STATES.AWAITING_INPUT) {
            MouseHoverHighlight.isEffectActive = true;
            ((Behaviour) actors[currentActorTurn].tokenRef.GetComponent("Halo")).enabled = true;
        } else if (newSate == STATES.ANIMATING_ACTION) {
            MouseHoverHighlight.isEffectActive = false;
            ((Behaviour) actors[currentActorTurn].tokenRef.GetComponent("Halo")).enabled = false;
        } else if (newSate == STATES.MENU) {
            MouseHoverHighlight.isEffectActive = false;
            ((Behaviour) actors[currentActorTurn].tokenRef.GetComponent("Halo")).enabled = false;
            InGameCanvas.SetActive(false);
            MenuCanvas.SetActive(true);
        }
    }

    private static int RollDice(int numDice, int diceMagnitude, int mod) {
        int diceTotal = mod;
        for (int i = 0; i < numDice; ++i) {
            diceTotal += Random.Range(1, diceMagnitude);
        }
        return diceTotal;
    }

    public void OnClickStartUndead() {
        // Reset the scene and place the new scene's tokens
        ResetBuildAndStartScene(undeadScene);
    }

    public void OnClickStartKobol() {
        ResetBuildAndStartScene(koboldScene);
    }

    private void ResetBuildAndStartScene(SceneActor[] predefinedSceneActors) {
        // Reset the scene to blank
        ResetBoard(); // put the squares back in their reset position
        ReleaseBoard(); // Drop the squares
        actors = new List<Actor>();
        playerCount = 0;
        monsterCount = 0;
        currentActorTurn = -1; // -1 so turns actually start a 0

        // Build scene objects from predefined
        foreach (SceneActor actorData in predefinedSceneActors) {
            // Create GameObject and place it in the correct square
            GameObject newGameObject;
            if (actorData.IsPlayer) {
                newGameObject = (GameObject) Instantiate(instance.PlayerPrefabList[actorData.PrefabIndex]);
                playerCount++;
            } else {
                newGameObject = (GameObject) Instantiate(instance.MonsterPrefabsList[actorData.PrefabIndex]);
                monsterCount++;
            }
            Space spaceToPlace = spaces[actorData.x, actorData.z];
            Vector3 squareBasis = spaceToPlace.gameSpace.transform.position;
            newGameObject.transform.position = new Vector3(squareBasis.x, DropFromHeight + 1, squareBasis.z);

            TokenStats stats = newGameObject.GetComponent<TokenStats>();
            Actor newActor = new Actor(newGameObject, actorData.x, actorData.z, actorData.ActorColor, actorData.IsPlayer, stats.characterName, stats.HP, stats.AC,
                stats.InitativeMod, stats.Speed, stats.AttackName, stats.AttackRange, stats.AttackMod, stats.DamageDiceNum,
                stats.DamageDiceMagnitude, stats.DamageMod);
            spaces[actorData.x, actorData.z].isBlocked = true;

            actors.Add(newActor);
        }

        // Show UI
        InGameCanvas.SetActive(true);

        // Roll init and sort
        RollInit();

        // Start the action!
        NextTurn();
    }

    // Instantiate square objects, but don't make them active yet
    private void GenerateSquares() {
        // Set up X,Z containers
        for (int x = 0; x < RowsX; x++) {
            for (int z = 0; z < ColsZ; z++) {
                spaces[x, z] = new Space(x, z, false);
            }
        }

        // Block any spaces that are impassible
        // A tree!
        spaces[29, 14].isBlocked = true;
        // A big rock!
        spaces[12, 32].isBlocked = true;
        spaces[13, 25].isBlocked = true;
        spaces[13, 26].isBlocked = true;
        spaces[13, 32].isBlocked = true;
        spaces[14, 26].isBlocked = true;
        spaces[14, 27].isBlocked = true;
        spaces[14, 28].isBlocked = true;
        spaces[14, 29].isBlocked = true;
        spaces[14, 30].isBlocked = true;
        spaces[14, 31].isBlocked = true;
        spaces[14, 32].isBlocked = true;
        spaces[15, 27].isBlocked = true;
        spaces[15, 28].isBlocked = true;
        spaces[15, 29].isBlocked = true;
        spaces[15, 30].isBlocked = true;
        spaces[15, 31].isBlocked = true;

        for (int x = 0; x < RowsX; x++) {
            for (int z = 0; z < ColsZ; z++) {
                if (!spaces[x, z].isBlocked) {
                    spaces[x, z].gameSpace = (GameObject) Instantiate(instance.OneByOnePrefab, SpacesHolder.transform);
                }
            }
        }
    }

    // Place squares back in the original position for a new game scenario
    private void ResetBoard() {
        // Hide menu
        MenuCanvas.SetActive(false);

        // Remove any actors that are still on the board
        if (actors != null) {
            foreach (Actor actor in actors) {
                Destroy(actor.tokenRef);
                spaces[actor.x, actor.z].isBlocked = false;
            }
        }

        for (int x = 0; x < RowsX; x++) {
            for (int z = 0; z < ColsZ; z++) {
                if (!spaces[x, z].isBlocked) {
                    spaces[x, z].gameSpace.transform.position = new Vector3(x + Margin, DropFromHeight, z + Margin);
                    spaces[x, z].gameSpace.SetActive(false);
                }
            }
        }
    }

    // Re-activate all squares so they fall
    private void ReleaseBoard() {
        for (int x = 0; x < RowsX; x++) {
            for (int z = 0; z < ColsZ; z++) {
                if (!spaces[x, z].isBlocked) {
                    spaces[x, z].gameSpace.SetActive(true);
                }
            }
        }
    }

    private void RollInit() {
        foreach (Actor actor in actors) {
            actor.RollInit();
        }
        actors.Sort((a, b) => b.Initative.CompareTo(a.Initative));
        string turnTrackerList = "";
        foreach (Actor actor in actors) {
            turnTrackerList += actor.Initative + " - " + actor.ActorName + "\n";
        }
        TextTurnTracker.text = turnTrackerList;
    }

    public void NextTurn() {
        // Turn of highlight for previous token
        if (currentActorTurn >= 0) { // skip for first turn
            ((Behaviour) actors[currentActorTurn].tokenRef.GetComponent("Halo")).enabled = false;
        }

        // Update counter for new turn (skipping killed actors)
        int infinteLoopGuard = actors.Count + 1; // paranoid that Unity will crash on me again....
        do {
            currentActorTurn = (currentActorTurn + 1) % actors.Count;
            infinteLoopGuard--;
        } while (!actors[currentActorTurn].IsAlive || infinteLoopGuard < 0);
        if (infinteLoopGuard < 0) {
            Debug.Log("INFINTE LOOP!");
        }

        // Set text for this actor
        TextCurrentActor.text = actors[currentActorTurn].ActorName;
        TextHP.text = "HP: " + actors[currentActorTurn].HP;
        TextAC.text = "AC: " + actors[currentActorTurn].AC;
        TextAtkName.text = actors[currentActorTurn].AttackName;
        TextAtkRoll.text = "1d20 + " + actors[currentActorTurn].AttackMod;
        TextDmgRoll.text = actors[currentActorTurn].DamageDieNum + "d" + actors[currentActorTurn].DamageDieMagnitude + " + " + actors[currentActorTurn].DamageMod;
        TextSpeedLeft.text = actors[currentActorTurn].Speed + " Spaces";

        // Track what's been happening this turn
        currentTurnStats = new Turn {MovementLeft = actors[currentActorTurn].Speed};

        // Change visuals for this actor's turn
        MouseHoverHighlight.MouseOverColor = actors[currentActorTurn].ActorColor;

        // Set state
        SetState(STATES.AWAITING_INPUT);
    }

    public void CheckForTurnCompleted() {
        if (currentTurnStats.MovementLeft == 0 && currentTurnStats.HasAttackHappened) {
            // Current turn actor is out of movement and has already attacked
            NextTurn();
        }
    }

    // Contains the information for a current turn. Temporary: will be deleted after one turn is done
    public class Turn {
        public int MovementLeft;
        public bool HasAttackHappened = false;
    }

    // Recevied from any arbitrary GameObject with the OnClick-Message script attached
    public void MessageClickedToken(GameObject attackee) {
        SetState(STATES.ANIMATING_ACTION);
        if (currentTurnStats.HasAttackHappened) {
            PopupTextController.PopupText("Already attacked", attackee.transform);
        } else {
            GameObject attacker = actors[currentActorTurn].tokenRef;
            if (attackee == attacker) {
                PopupTextController.PopupText("Can't attack self", attackee.transform);
            } else {
                Actor victim = actors.Find(actor => { return actor.tokenRef == attackee; });
                if (victim == null) {
                    PopupTextController.PopupText("ERROR FINDING ACTOR", attackee.transform);
                } else {
                    if (!victim.IsAlive) {
                        PopupTextController.PopupText("Creature is already dead", attackee.transform);
                    } else {
                        // Check if attack is possible, using A* pathfinding to find range in num squares, manhattan distance
                        if (Pathfind.FindDistance(actors[currentActorTurn].x, actors[currentActorTurn].z, victim.x, victim.z) > actors[currentActorTurn].AttackRange) {
                            PopupTextController.PopupText("Out of range", attackee.transform);
                        } else {
                            // Roll to hit
                            int attackResult = RollDice(1, 20, actors[currentActorTurn].AttackMod);
                            if (attackResult >= victim.AC) {
                                PopupTextController.PopupText("Hit: " + attackResult + " vs. " + victim.AC, attacker.transform);

                                // Animate attack
                                attacker.GetComponent<TokenAttacker>().AttackTowards(attackee.transform);

                                int damageResult = RollDice(actors[currentActorTurn].DamageDieNum, actors[currentActorTurn].DamageDieMagnitude, actors[currentActorTurn].DamageMod);
                                victim.HP -= damageResult;

                                delayedMessage = damageResult + " damage";
                                delayedActor = victim;
                                Invoke("DelayDamagePopup", 0.5f);
                                return;
                            } else {
                                PopupTextController.PopupText("Miss: " + attackResult + " vs. " + victim.AC, attackee.transform);
                            }

                            // Finalise attack
                            currentTurnStats.HasAttackHappened = true;
                        }
                    }
                }
            }
        }
        SetState(STATES.AWAITING_INPUT);
        CheckForTurnCompleted();
    }
    private Actor delayedActor;
    private string delayedMessage;
    private void DelayDamagePopup() {
        PopupTextController.PopupText(delayedMessage, delayedActor.tokenRef.transform);
        CheckForDeath(delayedActor);
        SetState(STATES.AWAITING_INPUT);
        CheckForTurnCompleted();
    }

    public void CheckForDeath(Actor actor) {
        if (actor.HP <= 0) {
            actor.IsAlive = false; // Note: still blocking its space, which is fine!
            KillAnimation(actor.tokenRef);
            if (actor.IsPlyaer) {
                playerCount--;
            } else {
                monsterCount--;
            }
            Invoke("CheckForGameOver", 1.1f);
        }
    }

    private void KillAnimation(GameObject actorTokenRef) {
        actorTokenRef.transform.position += new Vector3(0.3f, 0.5f, 0);
        toResetFreeze = actorTokenRef.GetComponent<Rigidbody>();

        // allow only Z rotation
        toResetFreeze.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX |
                                    RigidbodyConstraints.FreezeRotationY;

        // Tap! Fall down
        toResetFreeze.AddTorque(new Vector3(0, 0, 1.5f)); // rotate along Z axis;

        // Lock back in place after it has a chance to fall down
        Invoke("ReFreeze", 1f);
    }
    private Rigidbody toResetFreeze;
    private void ReFreeze() {
        toResetFreeze.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
    }

    private void CheckForGameOver() {
        if (playerCount < 1) {
            MonstersWinMessage.SetActive(true);
            PlayersWinMessage.SetActive(false);
            SetState(STATES.MENU);
        } else if (monsterCount < 1) {
            MonstersWinMessage.SetActive(false);
            PlayersWinMessage.SetActive(true);
            SetState(STATES.MENU);
        }
    }

    // Recevied from any arbitrary GameObject with the OnClick-Message script attached
    public void MessageClickedSpace(Vector2 coord) {
        WalkActor(actors[currentActorTurn], (int) coord.x, (int) coord.y);
    }

    // Walk a player or monster token to a space
    private void WalkActor(Actor actor, int xTo, int zTo) {
        // Find a path to the desired square, by getting a queue of sqaures to hop over
        LinkedList<TokenWalker.Hop> hopsQueue = Pathfind.FindPath(actor.x, actor.z, xTo, zTo);

        if (hopsQueue != null) {
            if (hopsQueue.Count > currentTurnStats.MovementLeft) {
                PopupTextController.PopupText("Not Enough Movement", spaces[xTo, zTo].gameSpace.transform);
            } else {
                // change the token's stored properties to its final position
                spaces[actor.x, actor.z].isBlocked = false;
                actor.x = xTo;
                actor.z = zTo;
                spaces[xTo, zTo].isBlocked = true;

                SetState(STATES.ANIMATING_ACTION);

                // Use the script attached to the token to walk the path
                actor.tokenRef.GetComponent<TokenWalker>().WalkPath(hopsQueue);
            }
        } else {
            PopupTextController.PopupText("Pathfinding failed", spaces[xTo, zTo].gameSpace.transform);
        }
    }

    void Update() {
        // Move the camera along the diagonals
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
            Camera.transform.position = new Vector3(Camera.transform.position.x + deltaX, Camera.transform.position.y, Camera.transform.position.z + deltaZ);
        }

        if (state == STATES.AWAITING_INPUT) {
            if (Input.GetKey(KeyCode.Space)) {
                NextTurn();
            }
        }
    }

    // A struct to hold information about the game board spaces
    public class Space {
        public GameObject gameSpace = null; // public reference to the OneByOne GameObject pointed to by this space
        public int x, z; // public reference to this object's position in the grid
        public bool isBlocked; // Define if this space is impassible

        public Space(int x, int z, bool isBlocked) {
            this.x = x;
            this.z = z;
            this.isBlocked = isBlocked;
        }
    }

    // A class to define a Prebuilt Scenario, stored as an array of SceneActors
    // Stores each token's initial position and properties
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
        public int x, z;
        public bool IsPlyaer;
        public bool IsAlive = true;
        public readonly string ActorName;
        public int HP, AC, InitativeMod, Speed;
        public int Initative;
        public string AttackName;
        public int AttackRange, AttackMod, DamageDieNum, DamageDieMagnitude, DamageMod;
        public Color ActorColor; // the colour to surround this token with indicating it is the active Actor, and to use as the cursor highlight

        public Actor(GameObject tokenRef, int x, int z, Color actorColor, bool isPlyaer, string actorName, int hp, int ac, int initativeMod, int speed, string attackName,
            int attackRange, int attackMod, int damageDieNum, int damageDieMagnitude, int damageMod) {
            this.tokenRef = tokenRef;
            this.x = x;
            this.z = z;
            ActorColor = actorColor;
            IsPlyaer = isPlyaer;
            ActorName = actorName;
            HP = hp;
            AC = ac;
            InitativeMod = initativeMod;
            Speed = speed;
            AttackName = attackName;
            AttackRange = attackRange;
            AttackMod = attackMod;
            DamageDieNum = damageDieNum;
            DamageDieMagnitude = damageDieMagnitude;
            DamageMod = damageMod;
        }

        public void RollInit() {
            Initative = RollDice(1, 20, InitativeMod);
        }
    }
}