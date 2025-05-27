using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    public class Node
    {
        public Vector2Int Position;
        public float GCost;
        public float HCost;
        public Node Parent;
        public float FCost => GCost + HCost;

        public override bool Equals(object obj)
        {
            return obj is Node other && Position.Equals(other.Position);
        }
        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }

    public static List<Vector2Int> FindPath(
        Vector2Int start,
        Vector2Int end,
        System.Func<Vector2Int, List<Vector2Int>> getNeighbors,
        System.Func<Vector2Int, Vector2Int, float> getCost,
        System.Func<Vector2Int, Vector2Int, float> getHeuristic)
    {
        var openSet = new PriorityQueue<Node>((a, b) => a.FCost.CompareTo(b.FCost));
        var allNodes = new Dictionary<Vector2Int, Node>();
        var closedSet = new HashSet<Vector2Int>();

        var startNode = new Node
        {
            Position = start,
            GCost = 0,
            HCost = getHeuristic(start, end),
            Parent = null
        };
        openSet.Enqueue(startNode);
        allNodes[start] = startNode;

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            if (closedSet.Contains(current.Position))
                continue;

            // Mark as evaluated
            closedSet.Add(current.Position);

            // Check goal
            if (current.Position == end)
                return RetracePath(current);

            // Explore neighbors
            foreach (var neighborPos in getNeighbors(current.Position))
            {
                if (closedSet.Contains(neighborPos))
                    continue;

                float moveCost = getCost(current.Position, neighborPos);
                float newG = current.GCost + moveCost;

                if (!allNodes.TryGetValue(neighborPos, out var neighborNode) || newG < neighborNode.GCost)
                {
                    var newNode = new Node
                    {
                        Position = neighborPos,
                        GCost = newG,
                        HCost = getHeuristic(neighborPos, end),
                        Parent = current
                    };

                    // Replace existing node in open set if present
                    if (neighborNode != null)
                        openSet.Remove(neighborNode);

                    openSet.Enqueue(newNode);
                    allNodes[neighborPos] = newNode;
                }
            }
        }

        // No path
        return new List<Vector2Int>();
    }

    private static List<Vector2Int> RetracePath(Node endNode)
    {
        var path = new List<Vector2Int>();
        var current = endNode;
        while (current != null)
        {
            path.Add(current.Position);
            current = current.Parent;
        }
        path.Reverse();
        return path;
    }
}

public class PriorityQueue<T>
{
    private List<T> data = new List<T>();
    private System.Comparison<T> comparison;

    public int Count => data.Count;

    public PriorityQueue(System.Comparison<T> comparison)
    {
        this.comparison = comparison;
    }

    public void Enqueue(T item)
    {
        data.Add(item);
        int ci = data.Count - 1;
        while (ci > 0)
        {
            int pi = (ci - 1) / 2;
            if (comparison(data[ci], data[pi]) >= 0) break;
            (data[ci], data[pi]) = (data[pi], data[ci]);
            ci = pi;
        }
    }

    public T Dequeue()
    {
        int li = data.Count - 1;
        var front = data[0];
        data[0] = data[li];
        data.RemoveAt(li);
        li--;
        int pi = 0;
        while (true)
        {
            int ci = pi * 2 + 1;
            if (ci > li) break;
            int rc = ci + 1;
            if (rc <= li && comparison(data[rc], data[ci]) < 0)
                ci = rc;
            if (comparison(data[pi], data[ci]) <= 0) break;
            (data[pi], data[ci]) = (data[ci], data[pi]);
            pi = ci;
        }
        return front;
    }

    public void Remove(T item)
    {
        int idx = data.IndexOf(item);
        if (idx < 0) return;
        int li = data.Count - 1;
        data[idx] = data[li];
        data.RemoveAt(li);
        // rebuild heap
        for (int i = data.Count / 2 - 1; i >= 0; i--)
            Heapify(i);
    }

    private void Heapify(int i)
    {
        int smallest = i;
        int l = 2 * i + 1;
        int r = 2 * i + 2;
        if (l < data.Count && comparison(data[l], data[smallest]) < 0) smallest = l;
        if (r < data.Count && comparison(data[r], data[smallest]) < 0) smallest = r;
        if (smallest != i)
        {
            (data[i], data[smallest]) = (data[smallest], data[i]);
            Heapify(smallest);
        }
    }
}
