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
        startRecord.Tile = start;
        startRecord.Connection = null;
        startRecord.CostSoFar = 0;

        // Initialize open and closed lists.
        List<NodeRecord> openNodes = new List<NodeRecord>();
        openNodes.Add(startRecord);
        List<NodeRecord> closedNodes = new List<NodeRecord>();

        NodeRecord current = new NodeRecord();

        // Iterate through processing each node.
        while (openNodes.Count > 0)
        {
            // Find smallest element in open list.
            current = GetSmallest(openNodes);

            // If coloring tiles, update tile color.
            if (colorTiles) { current.ColorTile(activeColor); }

            // Pause animation to show new active tile.
            yield return new WaitForSeconds(waitTime);

            // If current is the goal node, then leave the loop.
            if (current.Tile == end) { break; }

            // Else, get its outgoing connections.
            Dictionary<Direction, GameObject> connections = current.Tile.GetComponent<Node>().Connections;

            // Loop through each connection in turn.
            foreach (GameObject connection in connections.Values)
            {
                // Get cost estimate for end node.
                GameObject endNode = connection;
                float endNodeCost = current.CostSoFar + 1;

                NodeRecord endNodeRecord;

                // Skip this node if closed.
                if (ContainsNode(closedNodes, endNode)) { continue; }

                // If node is open, check to see if route is worse than current route.
                else if (ContainsNode(openNodes, endNode))
                {
                    endNodeRecord = FindNode(openNodes, endNode);
                    if (endNodeRecord.CostSoFar <= endNodeCost) { continue; }
                }

                // Else, we have an unvisited node, thus make a record.
                else
                {
                    endNodeRecord = new NodeRecord();
                    endNodeRecord.Tile = endNode;
                }

                // Update end node record's cost and connection.
                endNodeRecord.CostSoFar = endNodeCost;
                endNodeRecord.Connection = current;

                // Update tile display, if displayCosts active.
                if (displayCosts) { endNodeRecord.Display(endNodeCost);  }

                // Add record to open nodes list.
                openNodes.Add(endNodeRecord);

                // Update open tile display, if displayCosts active.
                if (colorTiles) { endNodeRecord.ColorTile(openColor); }

                // Pause animation to show new open tile.
                yield return new WaitForSeconds(waitTime);
            }

            // Once finished looking at all connections of current node, put the record
            // into the closed list and remove it from the open list.
            openNodes.Remove(current);
            closedNodes.Add(current);

            // Update closed tile display, if displayCosts active.
            if (colorTiles) current.ColorTile(closedColor);
        }

        // Stops the stopwatch.
        watch.Stop();

        UnityEngine.Debug.Log("Seconds Elapsed: " + (watch.ElapsedMilliseconds / 1000f).ToString());
        UnityEngine.Debug.Log("Nodes Expanded: " + (closedNodes.Count + openNodes.Count).ToString());

        // Reset the stopwatch.
        watch.Reset();

        // Determine whether Dijkstra found a path and print it here.

        // Occurs if no goal was found or if we ran out of nodes. Thus, no solution.
        if (current.Tile != end) { UnityEngine.Debug.Log("Dijkstra's Search Failed!");  }

        else
        {
            path = new Stack<NodeRecord>();

            // Work back along the path, accumulating the connections.
            while (current.Tile != start)
            {
                path.Push(current.Connection);
                current = current.Connection;

                // If colorTilesm update path tile color.
                if (colorTiles) { current.ColorTile(pathColor); }

                // Pause animation to show new path tile.
                yield return new WaitForSeconds(waitTime);
            }

            // Print the statistics.
            UnityEngine.Debug.Log("Path Length: " + (path.Count).ToString());
        }

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

    public static bool ContainsNode(List<NodeRecord> nodeArray, GameObject node)
    {
        foreach (NodeRecord record in nodeArray)
        {
            if (record.Tile == node) { return true; }
        }

        return false;
    }

    public static NodeRecord FindNode(List<NodeRecord> nodeArray, GameObject node)
    {
        foreach (NodeRecord record in nodeArray)
        {
            if (record.Tile == node) { return record; }
        }

        return null;
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
    public NodeRecord Connection { get; set; } = null;
    public float CostSoFar { get; set; } = float.MaxValue;
    public float EstimatedTotalCost { get; set; } = float.MaxValue;


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
