using System.Collections.Generic;
using UnityEngine;

public class TokenWalker : MonoBehaviour {
    private static readonly float HOP_ANIMATION_TIME = 0.5f;

    private LinkedList<Hop> hopsQueue;
    private GameObject tokenToAnimate;
    private Vector3 startPos, endPos, relativeStartPos, relativeEndPos, center;
    private float startTime, endTime;
    private bool isWalking;

    public void WalkPath(LinkedList<Hop> hopsQueue) {
	    this.hopsQueue = hopsQueue;
        NextHop();
	}

    // Hop from one space to another space (probably right next to it)
    private void NextHop() {
        // Pop first hop off the queue
        if (hopsQueue.First != null) {
            // Get next hop out of queue
            Hop nextHop = hopsQueue.First.Value;
            hopsQueue.RemoveFirst(); // pop

            // Update UI to match one space moved
            GameManager.instance.currentTurnStats.MovementLeft -= 1;
            GameManager.instance.TextSpeedLeft.text = GameManager.instance.currentTurnStats.MovementLeft + " Spaces";

            // Set up global variables for next hop. (Requried to be global to use InvokeRepeating to loop through the animation.)
            startPos = GameManager.instance.spaces[nextHop.xFrom, nextHop.zFrom].gameSpace.transform.position + GameManager.instance.SPACE_HEIGHT_MOD * 2;
            endPos = GameManager.instance.spaces[nextHop.xTo, nextHop.zTo].gameSpace.transform.position + GameManager.instance.SPACE_HEIGHT_MOD * 2;
            center = (startPos + endPos) * 0.5f;
            center -= new Vector3(0, 0.1f, 0); // make circular movment a bit flatter (also, this line is necessary to have the arc be along the Y plane)
            relativeStartPos = startPos - center;
            relativeEndPos = endPos - center;
            tokenToAnimate = gameObject;
            startTime = Time.time;
            endTime = startTime + HOP_ANIMATION_TIME;
            isWalking = true;
        } else {
            GameManager.instance.SetState(GameManager.STATES.AWAITING_INPUT);
            GameManager.instance.CheckForTurnCompleted();
        }
    }

	// Update is called once per frame
	void Update () {
        if (isWalking) {
            if (Time.time < endTime) {
                // Using Slerp to make an arc of movement is from unity manual: https://docs.unity3d.com/ScriptReference/Vector3.Slerp.html
                //     and this post: https://answers.unity.com/questions/11184/moving-player-in-an-arc-from-startpoint-to-endpoin.html
                tokenToAnimate.transform.position = Vector3.Slerp(relativeStartPos, relativeEndPos, (Time.time - startTime) / (endTime - startTime));
                tokenToAnimate.transform.position += center;
            } else {
                isWalking = false;
                NextHop();
            }
        }
	}

    public class Hop {
        public int xFrom, zFrom, xTo, zTo;
        public Hop(int xFrom, int zFrom, int xTo, int zTo) { this.xFrom = xFrom; this.zFrom = zFrom; this.xTo = xTo; this.zTo = zTo; }
    }

}
