using System.Collections;
using System.Collections.Generic;

using UnityEditor.Experimental.GraphView;

using UnityEngine;
using UnityEngine.SceneManagement;

public class PathFinder
{
    public Lattice2D<bool> wallLattice { get; private set; }

    public readonly Lattice2D<PathNode> lattice;

    public int maxPathLength = byte.MaxValue;

    public PathFinder(int width, int height)
    {
        lattice = new Lattice2D<PathNode>(width, height);

        for (int x = 0; x < lattice.Width; x++)
        {
            for (int y = 0; y < lattice.Height; y++)
            {
                lattice[x, y] = new PathNode(this, lattice, x, y);
                lattice[x, y].GCost = PathNode.VERY_LARGE;
            }
        }

        CreateWalls();

        for (int x = 0; x < wallLattice.Width; x++)
        {
            for (int y = 0; y < wallLattice.Height; y++)
            {
                if (wallLattice[x, y])
                    Debug.Log(wallLattice[x, y] + " " + x + " " + y);
            }
        }

    }

    public PathFinder(int width, int height, Vector2 origin)
    {
        lattice = new Lattice2D<PathNode>(width, height, origin);

        for (int x = 0; x < lattice.Width; x++)
        {
            for (int y = 0; y < lattice.Height; y++)
            {
                lattice[x, y] = new PathNode(this, lattice, x, y);
                lattice[x, y].GCost = PathNode.VERY_LARGE;
            }
        }

        CreateWalls();
    }

    private void CreateWalls()
    {
        wallLattice = new Lattice2D<bool>(lattice.Width, lattice.Height, lattice.Origin);

        for (int x = 0; x < lattice.Width; x++)
        {
            for (int y = 0; y < lattice.Height; y++)
            {
                Collider2D[] cols = Physics2D.OverlapBoxAll(lattice[x, y].WorldPosition, new Vector2(0.5f, 0.5f), 0f);

                for (int i = 0; i < cols.Length; i++)
                {
                    if (!cols[i].isTrigger && cols[i].gameObject.layer == 3)
                    {
                        wallLattice[x, y] = true;
                        break;
                    }
                }
            }
        }
    }

    public List<PathNode> FindPath(PathNode start, PathNode end)
    {
        start.GCost = 0;
        start.HCost = PathNode.FindDistanceCost(start, end);

        List<PathNode> open = new List<PathNode>() { start };
        HashSet<PathNode> closed = new HashSet<PathNode>();

        while (open.Count > 0)
        {
            PathNode current = PathNode.FindCheapestNode(open);

            if (current == end)
                return TracePath(end);

            open.Remove(current);
            closed.Add(current);

            foreach (PathNode border in current.GetBorderingNodes(current))
            {
                if (closed.Contains(border))
                    continue;

                int newGCost = current.GCost + PathNode.FindDistanceCost(current, border);
                if (newGCost < border.GCost)
                {
                    border.previous = current;
                    border.GCost = newGCost;
                    border.HCost = PathNode.FindDistanceCost(border, end);

                    if (!open.Contains(border))
                        open.Add(border);
                }
            }
        }
        Debug.Log("No Path");
        return null;
    }

    private List<PathNode> TracePath(PathNode node)
    {
        List<PathNode> results = new List<PathNode>();
        results.Add(node);
        int attempts = 0;

        while (node.previous != null)
        {
            results.Add(node.previous);
            node = node.previous;

            if (attempts > maxPathLength)
            {
                Debug.LogError("There was a loop in the pathfinding!");
                return null;
            }
            attempts++;
        }
        results.Reverse();
        return results;
    }
}
