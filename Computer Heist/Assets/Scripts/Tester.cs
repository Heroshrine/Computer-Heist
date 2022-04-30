using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Tester : MonoBehaviour
{

    public Vector2Int lattice2DSize;

    public Vector2Int firstNode;
    public Vector2Int lastNode;

    void Start()
    {
        PathFinder pathFinder = new PathFinder(lattice2DSize.x, lattice2DSize.y);

        Debug.Log("Path:");

        System.DateTime start = System.DateTime.Now;
        List<PathNode> pathNodes = pathFinder.FindPath(pathFinder.lattice.GetCell(firstNode.x, firstNode.y), pathFinder.lattice.GetCell(lastNode.x, lastNode.y));
        System.DateTime end = System.DateTime.Now;

        pathNodes.ForEach(x => Debug.Log(x));

        List<Debugger> generated = new List<Debugger>();

        pathFinder.lattice.ForEach(x =>
        {
            GameObject n = new GameObject();
            Debugger dn = n.AddComponent<Debugger>();
            dn.color = Color.white;
            n.transform.position = x.WorldPosition;
            generated.Add(dn);
        });

        pathNodes.ForEach(n =>
        {
            Debugger found = generated.Find(x => (Vector2)x.gameObject.transform.position == n.WorldPosition);
            found.color = Color.red;
        });
        Debug.Log("It took " + end.Subtract(start).Milliseconds + " milliseconds to find a path.");
    }
}

public class Debugger : MonoBehaviour
{
    public Color color;

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, 0.35f);
    }
}