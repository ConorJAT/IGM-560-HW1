using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Performs search using A*.
/// </summary>
public class AStar : MonoBehaviour
{
    // Colors for the different search categories.
    public static Color openColor = Color.cyan;
    public static Color closedColor = Color.blue;
    public static Color activeColor = Color.yellow;
    public static Color pathColor = Color.yellow;

    // The stopwatch for timing search.
    private static Stopwatch watch = new Stopwatch();

    public static IEnumerator search(GameObject start, GameObject end, Heuristic heuristic, float waitTime, bool colorTiles = false, bool displayCosts = false, Stack<NodeRecord> path = null)
    {
        // Starts the stopwatch.
        watch.Start();

        // Add your A* code here.

        // Initialize record for the start node.
        NodeRecord startRecord = new NodeRecord();
        startRecord.Tile = start;
        startRecord.Connection = null;
        startRecord.CostSoFar = 0;
        startRecord.EstimatedTotalCost = 0;

        // Retrieve number used to scale the game world tiles.
        float scale = startRecord.Tile.transform.localScale.x;

        // Initialize open and closed lists.
        List<NodeRecord> openNodes = new List<NodeRecord>();
        openNodes.Add(startRecord);
        List<NodeRecord> closedNodes = new List<NodeRecord>();

        NodeRecord current = new NodeRecord();

        // Iterate through processing each node.
        while (openNodes.Count > 0)
        {
            // Fine the smallest element in open list.
            current = GetSmallest(openNodes);

            // If colorTiles, update tile color.
            if (colorTiles) { current.ColorTile(activeColor); }

            // Pause animation to show new active tile.
            yield return new WaitForSeconds(waitTime);

            // If current node is the goal node, then break outta the loop.
            if (current.Tile == end) { break; }

            // Otherwise, get outgoing connections.
            Dictionary<Direction, GameObject> connections = current.Tile.GetComponent<Node>().Connections;

            // Loop through each connection in turn.
            foreach (GameObject connection in connections.Values)
            {
                // Get cost estimate for the end node.
                // Cost of each connection is equal to the scale.
                GameObject endNode = connection;
                float endNodeCost = current.CostSoFar + scale;

                NodeRecord endNodeRecord;
                float endNodeHeuristic;

                // If node is closed, may have to skip or remove from closed list.
                if (ContainsNode(closedNodes, endNode))
                {
                    // Find record in the closed list corresponding to end node.
                    endNodeRecord = FindNode(closedNodes, endNode);

                    // If no shorter route is found, skip.
                    if (endNodeRecord.CostSoFar <= endNodeCost) { continue; }

                    // Otherwise, remove it from closed list.
                    closedNodes.Remove(endNodeRecord);

                    // Can use node's old cost values to calculate heursistic value w/o
                    // calling possibly expensive function.
                    endNodeHeuristic = endNodeRecord.EstimatedTotalCost - endNodeRecord.CostSoFar;
                }

                // Skip if node is open and a better route is found.
                else if (ContainsNode(openNodes, endNode))
                {
                    // Find the record in open list corresponding to endNode.
                    endNodeRecord = FindNode(openNodes, endNode);

                    // If route is no better, skip.
                    if (endNodeRecord.CostSoFar <= endNodeCost) { continue; }

                    // Calculate heuristic.
                    endNodeHeuristic = endNodeRecord.EstimatedTotalCost - endNodeRecord.CostSoFar;
                }

                // Otherwise, we've got an unvisited node, so make a record.
                else
                {
                    endNodeRecord = new NodeRecord();
                    endNodeRecord.Tile = endNode;

                    // Caclculate heuristic using function.
                    endNodeHeuristic = heuristic(start, endNode, end);
                }

                // Update cost, estimate and connection of endNodeRecord.
                endNodeRecord.CostSoFar = endNodeCost;
                endNodeRecord.Connection = current;
                endNodeRecord.EstimatedTotalCost = endNodeCost + endNodeHeuristic;
                // endNodeHeuristic += endNodeCost;

                // If displayCosts, update tile display.
                if (displayCosts) { endNodeRecord.Display(endNodeCost); }
                
                // Add record to the open list.
                openNodes.Add(endNodeRecord);

                // If colorTiles, update open tile color.
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

        // Determine whether A* found a path and print it here.

        // Occurs if no goal was found or if we ran out of nodes. Thus, no solution.
        if (current.Tile != end) { UnityEngine.Debug.Log("A* Search Failed!"); }

        else
        {
            path = new Stack<NodeRecord>();

            // Work back along path, accumulating the connections.
            while (current.Tile != start)
            {
                path.Push(current.Connection);
                current = current.Connection;

                // If colorTiles, update path tile color.
                if (colorTiles) { current.ColorTile(pathColor); }

                // Pause animation to show new path tile.
                yield return new WaitForSeconds(waitTime);
            }

            // Print search statistics.
            UnityEngine.Debug.Log("Path Length: " + (path.Count).ToString());
        }

        yield return null;
    }

    public delegate float Heuristic(GameObject start, GameObject tile, GameObject goal);

    public static float Uniform (GameObject start, GameObject tile, GameObject goal)
    {
        return 0f;
    }

    public static float Manhattan (GameObject start, GameObject tile, GameObject goal)
    {
        float dx = Mathf.Abs(tile.transform.position.x - goal.transform.position.x);
        float dy = Mathf.Abs(tile.transform.position.y - goal.transform.position.y);
        return dx + dy;
    }

    public static float CrossProduct (GameObject start, GameObject tile, GameObject goal)
    {
        float dx1 = tile.transform.position.x - goal.transform.position.x;
        float dy1 = tile.transform.position.y - goal.transform.position.y;
        float dx2 = start.transform.position.x - goal.transform.position.x;
        float dy2 = start.transform.position.y - goal.transform.position.y;
        float cross = Mathf.Abs((dx1 * dy2) - (dy1 * dx2));
        return Manhattan(start, tile, goal) + (cross * 0.001f);
    }

    public static NodeRecord GetSmallest(List<NodeRecord> nodeArray)
    {
        NodeRecord current = nodeArray[0];

        foreach (NodeRecord nodeToCheck in nodeArray)
        {
            if (nodeToCheck.EstimatedTotalCost < current.EstimatedTotalCost) { current = nodeToCheck; }
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
