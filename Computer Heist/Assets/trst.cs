using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class trst : MonoBehaviour
{

    private void Start()
    {
        PathFinder finder = new PathFinder(10, 10, "testGrid");
        List<PathNode> list = finder.FindPath(0, 0, 6, 6);
        list.ForEach(x => Debug.Log(x));
    }

}
