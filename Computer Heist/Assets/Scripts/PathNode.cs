using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PathNode
{
    public const int CARDINAL = 10;
    public const int DIAGNAL = 14;
    public const int VERY_LARGE = 1050000000;

    private Lattice2D<PathNode> lattice;
    private int x;
    private int y;

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
    public int hCost; // do not access
    public int FCost { get; private set; }

    public Vector2 Position
    {
        get
        {
            return lattice.LatticeToWorldPoint(x, y);
        }
    }

    public PathNode(Lattice2D<PathNode> lattice, int x, int y)
    {
        if (!lattice.Add(this, x, y))
            throw new System.ArgumentNullException("This cell already existed at " + x + ", " + y);

        this.x = x;
        this.y = y;
        this.lattice = lattice;
    }

    public static int FindDistanceCost(PathNode a, PathNode b)
    {
        int xd = Mathf.Abs(a.x - b.x);
        int yd = Mathf.Abs(a.y - b.y);
        int remainder = Mathf.Abs(xd - yd);

        return DIAGNAL * Mathf.Min(xd, yd) + CARDINAL * remainder;
    }

    public override string ToString()
    {
        return "node (" + x + ", " + y + ")\n" +
            "with costs g:" + GCost + " h:" + HCost + " f:" + FCost + "\n" +
            "on lattice: " + lattice.name;
    }

}
