using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Performs search using Dijkstra's algorithm.
/// </summary>
public class Dijkstra : MonoBehaviour
{
    // Colors for the different search categories.
    public static Color openColor = Color.cyan;
    public static Color closedColor = Color.blue;
    public static Color activeColor = Color.yellow;
    public static Color pathColor = Color.yellow;

    // The stopwatch for timing search.
    private static Stopwatch watch = new Stopwatch();

    public static IEnumerator search(GameObject start, GameObject end, float waitTime, bool colorTiles = false, bool displayCosts = false, Stack<NodeRecord> path = null)
    {
        // Starts the stopwatch.
        watch.Start();

        // Add your Dijkstra code here.

        // Initialize record for start node.
        NodeRecord startRecord = new NodeRecord();
        startRecord.Node = start;
        startRecord.Connection = null;
        startRecord.CostSoFar = 0;

        // Initialize open and closed lists.
        List<NodeRecord> openNodes = new List<NodeRecord>();
        openNodes.Add(startRecord);
        List<NodeRecord> closedNodes = new List<NodeRecord>();

        // Iterate through processing each node.
        while (openNodes.Count > 0)
        {
            // Find smallest element in open list.
            NodeRecord current = GetSmallest(openNodes);

            // If coloring tiles, update tile color.
            if (colorTiles) { current.ColorTile(activeColor); }

            // Pause animation to show new active tile.
            yield return new WaitForSeconds(waitTime);

            // If current is the goal node, then leave the loop.
            if (current.Node == end) { break; }

            // Else, get its outgoing connections.
            List<NodeRecord> connections = current.Connection;

            // Loop through each connection in turn.
            foreach (NodeRecord connection in connections)
            {
                NodeRecord endNode;
                float endNodeCost = current.CostSoFar;

                //if (closedNodes.Contains(endNode)) { continue; }
                //else if (openNodes.Contains(endNode))
                //{

                //}
            }
        }

        // Stops the stopwatch.
        watch.Stop();

        UnityEngine.Debug.Log("Seconds Elapsed: " + (watch.ElapsedMilliseconds / 1000f).ToString());
        UnityEngine.Debug.Log("Nodes Expanded: " + "print the number of nodes expanded here.");

        // Reset the stopwatch.
        watch.Reset();

        // Determine whether Dijkstra found a path and print it here.

        yield return null;
    }

    public static NodeRecord GetSmallest(List<NodeRecord> nodeArray)
    {
        NodeRecord current = nodeArray[0];

        foreach (NodeRecord nodeToCheck in nodeArray)
        {
            if (nodeToCheck.CostSoFar < current.CostSoFar) { current = nodeToCheck; }
        }

        return current;
    }
}

/// <summary>
/// A class for recording search statistics.
/// </summary>
public class NodeRecord
{
    // The tile game object.
    public GameObject Tile { get; set; } = null;

    // Set the other class properties here.
    public GameObject Node { get; set; } = null;
    public List<NodeRecord> Connection { get; set; } = null;
    public float CostSoFar { get; set; } = float.MaxValue;


    // Sets the tile's color.
    public void ColorTile (Color newColor)
    {
        SpriteRenderer renderer = Tile.GetComponentInChildren<SpriteRenderer>();
        renderer.material.color = newColor;
    }

    // Displays a string on the tile.
    public void Display (float value)
    {
        TextMesh text = Tile.GetComponent<TextMesh>();
        text.text = value.ToString();
    }
}
