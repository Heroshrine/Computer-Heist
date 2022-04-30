using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Tester : MonoBehaviour
{
    void Start()
    {
        /*/
        Debug.Log("constructor 1: ");
        Lattice2D<PathNode> lattice = new Lattice2D<PathNode>(4, 4, new Vector2(-6, 12));

        for (int x = 0; x < lattice.Width; x++)
        {
            for (int y = 0; y < lattice.Height; y++)
            {
                lattice[x, y] = new PathNode(lattice, x, y);
                lattice[x, y].GCost = PathNode.VERY_LARGE;
            }
        }

        foreach (PathNode i in lattice)
        {
            Debug.Log(i.Position);
        }
        /*/

        Lattice2D<GameObject> gameObjectLattice = new Lattice2D<GameObject>(6, 6, new Vector2(0, 0));

        for (int x = 0; x < gameObjectLattice.Width; x++)
        {
            for (int y = 0; y < gameObjectLattice.Height; y++)
            {

                gameObjectLattice[x, y] = new GameObject();
                gameObjectLattice[x, y].transform.position = gameObjectLattice.LatticeToWorldPoint(x, y);
                gameObjectLattice[x, y].AddComponent<Debugger>();

            }
        }
    }
}

public class Debugger : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.35f);
    }
}