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

    public List<PathNode> FindPath(PathNode end)
    {
        if (lattice.Contains(end))
        {
            return Finder.FindPath(this, end);
        }
        else
            throw new System.ArgumentException("The lattice did not contain the path node");
    }

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

        if (node.x + 1 < lattice.Width && !Finder.WallLattice.GetCell(node.x + 1, node.y))
        {
            results.Add(lattice.GetCell(node.x + 1, node.y));

            if (node.y + 1 < lattice.Height && !Finder.WallLattice.GetCell(node.x + 1, node.y + 1) && !Finder.WallLattice.GetCell(node.x, node.y + 1))
                results.Add(lattice.GetCell(node.x + 1, node.y + 1));
            if (node.y - 1 >= 0 && !Finder.WallLattice.GetCell(node.x + 1, node.y - 1) && !Finder.WallLattice.GetCell(node.x, node.y - 1))
                results.Add(lattice.GetCell(node.x + 1, node.y - 1));
        }

        if (node.x - 1 >= 0 && !Finder.WallLattice.GetCell(node.x - 1, node.y))
        {
            results.Add(lattice.GetCell(node.x - 1, node.y));

            if (node.y + 1 < lattice.Height && !Finder.WallLattice.GetCell(node.x - 1, node.y + 1) && !Finder.WallLattice.GetCell(node.x, node.y + 1))
                results.Add(lattice.GetCell(node.x - 1, node.y + 1));
            if (node.y - 1 >= 0 && !Finder.WallLattice.GetCell(node.x - 1, node.y - 1) && !Finder.WallLattice.GetCell(node.x, node.y - 1))
                results.Add(lattice.GetCell(node.x - 1, node.y - 1));
        }

        if (node.y + 1 < lattice.Height && !Finder.WallLattice.GetCell(node.x, node.y + 1))
            results.Add(lattice.GetCell(node.x, node.y + 1));
        if (node.y - 1 >= 0 && !Finder.WallLattice.GetCell(node.x, node.y - 1))
            results.Add(lattice.GetCell(node.x, node.y - 1));
        return results;
    }
}
