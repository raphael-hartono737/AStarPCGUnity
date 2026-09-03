# Procedural Content Generation using A* Algorithm with Unity Engine C#

This project is a flexible implementation of the A* (A-Star) pathfinding algorithm, commonly used in Unity projects to navigate grids. It calculates the most efficient route between a start and end point while accounting for obstacles or movement penalties. 

**CORE COMPONENTS** 
1. The Node Class: Represents a specific coordinate on the grid. Each node tracks three values: the GCost (actual distance traveled from the start), the HCost (the "heuristic" or estimated distance to the goal), and the FCost (the sum of G and H). It also tracks a Parent node to remember how it got there.
2. PriorityQueue: Because older versions of .NET lack a native priority queue, this script includes a custom Min-Heap data structure. It automatically sorts inserted items using the provided comparison so the node with the lowest FCost is always at the front for immediate processing during a Dequeue.
3. FindPath Logic: The main function uses an openSet (nodes to check) and a closedSet (nodes already checked). It continuously pulls the cheapest node from the openSet. If that node is the destination, the search ends; otherwise, it marks the node as evaluated in the closedSet.
4. Path Retracing: Once the end node is found, RetracePath walks backward through the Parent references until it hits the start, reversing the list to output the final step-by-step Vector2Int path.
