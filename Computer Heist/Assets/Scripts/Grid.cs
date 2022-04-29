using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Grid<TCell> : IEnumerable<TCell>
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public TCell[,] Cells { get; private set; }

    public Vector2 Origin { get; private set; }

    public Grid(int width, int height)
    {
        Width = width;
        Height = height;
        Cells = new TCell[Width, Height];

        Origin = Vector2.zero;
    }

    public Grid(int width, int height, Vector2 origin)
    {
        Width = width;
        Height = height;
        Cells = new TCell[Width, Height];

        Origin = origin;
    }

    public IEnumerator<TCell> GetEnumerator()
    {
        for (int w = 0; w < Width; w++)
        {
            for (int h = 0; h < Height; h++)
            {
                yield return Cells[w, h];
            }
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public TCell GetCell(int w, int h) => Cells[w, h];
    public TCell GetPosition(TCell cell, out int w, out int h)
    {

        for (int wd = 0; wd < Width; wd++)
        {
            for (int ht = 0; ht < Height; ht++)
            {
                if (Cells[wd, ht].Equals(cell))
                {
                    w = wd;
                    h = ht;
                    return Cells[wd, ht];
                }
            }
        }
        throw new System.InvalidOperationException("The TCell " + cell + " was not found in the grid!");
    }

    public Vector2Int WorldToGridPoint(Vector2 worldPoint) => new Vector2Int(Mathf.FloorToInt(worldPoint.x - Origin.x), Mathf.FloorToInt(worldPoint.y - Origin.y));
    public Vector2 GridToWorldPoint(Vector2Int gridPoint) => new Vector2(gridPoint.x + Origin.x + 0.5f, gridPoint.y + Origin.y + 0.5f);
    public static Vector2 RoundToGridWorldPoint(Vector2 worldPoint) => new Vector2(Mathf.Floor(worldPoint.x) + 0.5f, Mathf.Floor(worldPoint.y) + 0.5f);

    public override string ToString()
    {
        string result = "Origin: " + Origin + " Width: " + Width + " Height: " + Height + " containing:";

        foreach (TCell cell in Cells)
            result += "\n" + cell.ToString();

        return result;
    }
}
