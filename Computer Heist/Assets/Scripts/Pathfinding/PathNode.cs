using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PathNode
{
    public const int CARDINAL = 10;
    public const int DIAGONAL = 14;
    public const int VERY_LARGE = 1050000000;

    public Lattice2D<PathNode> lattice { get; private set; }
    public PathFinder Finder { get; private set; }
    private int x;
    private int y;

    public PathNode previous;

    public int GCost
    {
        get => gCost;
        set
        {
            gCost = value;
            FCost = gCost + hCost;
        }
    }
    private int gCost; // do not access
    public int HCost
    {
        get => hCost;
        set
        {
            hCost = value;
            FCost = hCost + gCost;
        }
    }
    private int hCost; // do not access
    public int FCost { get; private set; }

    public Vector2Int LatticePosition
    {
        get
        {
            return new Vector2Int(x, y);
        }
    }
    public Vector2 WorldPosition
    {
        get
        {
            return lattice.LatticeToWorldPosition(x, y);
        }
    }

    public PathNode(PathFinder finder, Lattice2D<PathNode> lattice, int x, int y)
    {
        if (!lattice.TrySetCell(this, x, y))
            throw new System.ArgumentNullException("This cell already existed at " + x + ", " + y);

        this.x = x;
        this.y = y;
        this.lattice = lattice;
        Finder = finder;
    }

    public static int FindDistanceCost(PathNode a, PathNode b)
    {
        int xd = Mathf.Abs(a.x - b.x);
        int yd = Mathf.Abs(a.y - b.y);
        int remainder = Mathf.Abs(xd - yd);

        return DIAGONAL * Mathf.Min(xd, yd) + CARDINAL * remainder;
    }

    public override string ToString()
    {
        return "node (" + x + ", " + y + ")\n" +
            "with costs g:" + GCost + " h:" + HCost + " f:" + FCost + "\n" +
            "on lattice: " + lattice.name;
    }

    /*/
    public List<PathNode> FindPath(PathNode start, PathNode end)
    {
        start.GCost = 0;
        start.HCost = FindDistanceCost(start, end);

        List<PathNode> open = new List<PathNode>() { start };
        HashSet<PathNode> closed = new HashSet<PathNode>();

        while (open.Count > 0)
        {
            PathNode current = FindCheapestNode(open);

            if (current == end)
                return TracePath(end);

            open.Remove(current);
            closed.Add(current);

            foreach (PathNode border in GetBorderingNodes(current))
            {
                if (closed.Contains(border))
                    continue;

                int newGCost = current.GCost + FindDistanceCost(current, border);
                if (newGCost < border.GCost)
                {
                    border.previous = current;
                    border.GCost = newGCost;
                    border.HCost = FindDistanceCost(border, end);

                    if (!open.Contains(border))
                        open.Add(border);
                }
            }
        }
        return null;
    }
    /*/
    /*/
    private List<PathNode> TracePath(PathNode node)
    {
        List<PathNode> results = new List<PathNode>();
        results.Add(node);
        int attempts = 0;

        while (node.previous != null)
        {
            results.Add(node.previous);
            node = node.previous;

            if (attempts > Finder.maxPathLength)
            {
                Debug.LogError("There was a loop in the pathfinding!");
                return null;
            }
            attempts++;
        }
        results.Reverse();
        return results;
    }
    /*/

    public static PathNode FindCheapestNode(List<PathNode> nodes)
    {
        PathNode result = nodes[0];
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].FCost < result.FCost)
                result = nodes[i];
        }
        return result;
    }

    public HashSet<PathNode> GetBorderingNodes(PathNode node)
    {
        HashSet<PathNode> results = new HashSet<PathNode>();

        if (node.x + 1 < lattice.Width && !Finder.wallLattice.GetCell(node.x + 1, node.y))
        {
            results.Add(lattice.GetCell(node.x + 1, node.y));

            if (node.y + 1 < lattice.Height && !Finder.wallLattice.GetCell(node.x + 1, node.y + 1) && !Finder.wallLattice.GetCell(node.x, node.y + 1))
                results.Add(lattice.GetCell(node.x + 1, node.y + 1));
            if (node.y - 1 >= 0 && !Finder.wallLattice.GetCell(node.x + 1, node.y - 1) && !Finder.wallLattice.GetCell(node.x, node.y - 1))
                results.Add(lattice.GetCell(node.x + 1, node.y - 1));
        }

        if (node.x - 1 >= 0 && !Finder.wallLattice.GetCell(node.x - 1, node.y))
        {
            results.Add(lattice.GetCell(node.x - 1, node.y));

            if (node.y + 1 < lattice.Height && !Finder.wallLattice.GetCell(node.x - 1, node.y + 1) && !Finder.wallLattice.GetCell(node.x, node.y + 1))
                results.Add(lattice.GetCell(node.x - 1, node.y + 1));
            if (node.y - 1 >= 0 && !Finder.wallLattice.GetCell(node.x - 1, node.y - 1) && !Finder.wallLattice.GetCell(node.x, node.y - 1))
                results.Add(lattice.GetCell(node.x - 1, node.y - 1));
        }

        if (node.y + 1 < lattice.Height && !Finder.wallLattice.GetCell(node.x, node.y + 1))
            results.Add(lattice.GetCell(node.x, node.y + 1));
        if (node.y - 1 >= 0 && !Finder.wallLattice.GetCell(node.x, node.y - 1))
            results.Add(lattice.GetCell(node.x, node.y - 1));
        return results;
    }
}
