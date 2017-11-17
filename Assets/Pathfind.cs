﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfind : MonoBehaviour {
    // A way for players/monsters to store their own grid coordinates. These are NOT used by the algorithm below! (It's easier to store them here than putting them in an empty script.)
    [HideInInspector] public int x, z;


    private bool DEBUG_MODE = true;

    public LinkedList<GameManager.Hop> findPath(int xFrom, int zFrom, int xTo, int zTo) {
        // **** do not pathfind to own square ****
        if (xTo == xFrom && zTo == zFrom) {
            return null;
        }

        // **** create data structures ****
        Node[,] nodes = new Node[GameManager.instance.rowsX, GameManager.instance.colsZ];
        LinkedList<Node> openList = new LinkedList<Node>();

        // **** set initial conditions ****
        // create node objects and set walls to closed
        for (int row = 0; row < GameManager.instance.rowsX; ++row) {
            for (int col = 0; col < GameManager.instance.colsZ; ++col) {
                nodes[col, row] = new Node();
                nodes[col, row].x = col;
                nodes[col, row].y = row;
                if (GameManager.instance.spaces[col, row].isBlocked) {
                    nodes[col, row].isClosed = true;
                }
            }
        }

        // **** add initial node to open list ****
        Node initialNode = nodes[xFrom, zFrom];
        initialNode.g = 0; // condition of the initial node
        initialNode.parent = null; // leaving this null will be the termination signal for the found path
        openList.AddLast(initialNode);

        // **** loop through nodes on open list until a path is found or list is empty ****
        Node curr; // the node we've just popped off the open list
        Node nearby; // hold nodes to compare to the open node
        bool isPathFound = false;
        bool isMazeSolvable = true;
        while (!isPathFound && isMazeSolvable) {
            // 1. find the open node with lowest f
            curr = openList.First.Value;
            foreach (Node openNode in openList) {
            //for (Node openNode : openList) {
                if (openNode.f <= curr.f) { // by doing less or EQUAL, this biases towards items examined last, i.e. the newer ones added to the open list
                    curr = openNode;
                }
            }
            // curr is now node with lowest f
            if (DEBUG_MODE) {
                Debug.Log("examining open node at (" + curr.x + "," + curr.y + ")");
            }

            // 2. close node
            curr.isClosed = true;
            openList.Remove(curr);

            // 3. test for termination condition: if this node is the target, then quit, successfully
            if (curr.x == xTo && curr.y == zTo) {
                isPathFound = true;
            }

            // 4. add all nodes surrounding current to open list, pointing back to current
            for (int deltaRow = -1; deltaRow <= 1; ++deltaRow) {
                if (curr.y + deltaRow == -1 || curr.y + deltaRow == GameManager.instance.rowsX) {
                    continue;
                }
                for (int deltaCol = -1; deltaCol <= 1; ++deltaCol) {
                    if (curr.x + deltaCol == -1 || curr.x + deltaCol == GameManager.instance.colsZ) {
                        continue;
                    }
                    nearby = nodes[curr.x + deltaCol, curr.y + deltaRow];

                    if (!nearby.isClosed) {
                        if (nearby.g == 0) { // first time examining this node
                            nearby.g = curr.g + 1;
                            nearby.h = System.Math.Abs(xTo - nearby.x) + System.Math.Abs(zTo - nearby.y);
                            nearby.f = nearby.g + nearby.h;
                            nearby.parent = curr;
                            openList.AddLast(nearby);
                        } else { // have already examined this node, but it's not yet closed
                            if (curr.g + 1 < nearby.g) { // if already on open list, yet current square would give it a better g: make it part of the current path instead
                                nearby.g = curr.g + 1;
                                // temp.h = this.heuristic(temp.x, temp.y); // don't need to recalc heuristic
                                nearby.f = nearby.g + nearby.h;
                                nearby.parent = curr; // do need to change parent
                            }
                        }
                    }
                }
            }

            // 5. test for termination condition
            if (openList.Count == 0) {
                isMazeSolvable = false;
            }
        } // end algorithmic loop

        // **** if a path was found, save that path externally ****
        if (isMazeSolvable) {
            LinkedList<GameManager.Hop> pathStack = new LinkedList<GameManager.Hop>();
            //path = new Stack<>();
            curr = nodes[xTo, zTo];
            // first item on stack should be xTo,Y
            // then traverse into path
            int prevX = xTo, prevZ = zTo;
            while (curr != null) {
                if (DEBUG_MODE) {
                    Debug.Log("pushing onto stack: (" + curr.x + "," + curr.y + ")");
                }
                pathStack.AddFirst(new GameManager.Hop(curr.x, curr.y, prevX, prevZ));
                //path.push(curr);
                prevX = curr.x;
                prevZ = curr.y;

                curr = curr.parent;
            }
            return pathStack;
        } else {
            // didn't find a path, so let monster just do a dumb run towards the player, into the dead end
            return null;
        }
    }

    // helper class for the A* algorithm
    // all fields are simply publicly accessible!
    private class Node {
        public int x, y;
        public Node parent = null;
        public int g, h, f;
        public bool isClosed = false;
    }
}
