using System.Collections;
using System.Collections.Generic;

using UnityEditor.Experimental.GraphView;

using UnityEngine;
using UnityEngine.SceneManagement;

public class PathFinder
{
    public Lattice2D<bool> wallLattice { get; private set; }

    public readonly Lattice2D<PathNode> Lattice;

    public int maxPathLength = byte.MaxValue;

    public PathFinder(int width, int height)
    {
        Lattice = new Lattice2D<PathNode>(width, height);

        ResetLattice();

        CreateWalls();
    }

    public PathFinder(int width, int height, Vector2 origin)
    {
        Lattice = new Lattice2D<PathNode>(width, height, origin);

        ResetLattice();

        CreateWalls();
    }

    public void ResetLattice()
    {
        for (int x = 0; x < Lattice.Width; x++)
        {
            for (int y = 0; y < Lattice.Height; y++)
            {
                Lattice[x, y] = new PathNode(this, Lattice, x, y);
                Lattice[x, y].GCost = PathNode.VERY_LARGE;
            }
        }
    }

    private void CreateWalls()
    {
        wallLattice = new Lattice2D<bool>(Lattice.Width, Lattice.Height, Lattice.Origin);

        for (int x = 0; x < Lattice.Width; x++)
        {
            for (int y = 0; y < Lattice.Height; y++)
            {
                Collider2D[] cols = Physics2D.OverlapBoxAll(Lattice[x, y].WorldPosition, new Vector2(0.5f, 0.5f), 0f);

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
        System.DateTime startTime = System.DateTime.Now;
        start.GCost = 0;
        start.HCost = PathNode.FindDistanceCost(start, end);

        List<PathNode> open = new List<PathNode>() { start };
        HashSet<PathNode> closed = new HashSet<PathNode>();

        while (open.Count > 0)
        {
            PathNode current = PathNode.FindCheapestNode(open);

            if (current == end)
            {
                ResetLattice();

                System.DateTime endTime = System.DateTime.Now;
                Debug.Log("It took " + endTime.Subtract(startTime).Milliseconds + " milliseconds to find a path.");
                return TracePath(end);
            }

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
    public List<PathNode> FindPath(Vector2 start, Vector2 end) => FindPath(Lattice.WorldToLatticePosition(start), Lattice.WorldToLatticePosition(end));

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

    public PathNode GetNode(Vector2 worldPosition) => Lattice.GetCell(Lattice.WorldToLatticePosition(worldPosition));
    public PathNode GetNode(Vector2Int latticePosition) => Lattice.GetCell(latticePosition);
    public PathNode GetNode(int x, int y) => Lattice.GetCell(x, y);
}
