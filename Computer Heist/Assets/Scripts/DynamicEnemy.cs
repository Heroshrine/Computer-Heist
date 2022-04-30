using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DynamicEnemy : MonoBehaviour
{
    Rigidbody2D rb2d;

    public float viewRadius = 4f;

    [Header("PathFinder")]
    public int pathGridWidth;
    public int pathGridHeight;
    [Tooltip("The bottom left of the pathfinder's grid")]
    public Vector2 pathGridOrigin;

    PathFinder finder;
    List<PathNode> path;
    int nextNode;

    [Header("Patrolling")]
    public float searchTime = 2;
    public List<Vector2> patrolPoints;

    int nextPatrolPoint;
    bool patrolling = true;

    float searchWaitTime;
    bool searching;

    [Header("Movement")]
    public float movementSpeed = 5f;
    public float chaseMultiplier = 1.5f;

    bool chasing = false;

    private void Awake()
    {
        finder = new PathFinder(pathGridWidth, pathGridHeight, pathGridOrigin);
    }

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        path = finder.FindPath(finder.GetNode(transform.position), finder.GetNode(patrolPoints[nextPatrolPoint]));
    }

    private void Update()
    {
        if (nextNode < path.Count && Vector2.Distance(transform.position, path[nextNode].WorldPosition) <= 0.0025f)
        {
            nextNode++;
            if (nextNode >= path.Count)
            {
                if (patrolling)
                {
                    nextPatrolPoint++;
                    nextNode = 0;

                    if (nextPatrolPoint >= patrolPoints.Count)
                    {
                        patrolPoints.Reverse();
                        nextPatrolPoint = 1;
                    }

                    path = finder.FindPath(finder.GetNode(patrolPoints[nextPatrolPoint - 1]), finder.GetNode(patrolPoints[nextPatrolPoint]));
                }
            }
        }
        else if (searching)
        {
            searchWaitTime += Time.deltaTime;
            if (searchWaitTime >= searchTime && !chasing)
            {
                searching = false;
                patrolling = true;
                searchWaitTime = 0;
                nextNode = 0;
                path = finder.FindPath(finder.GetNode(transform.position), finder.GetNode(patrolPoints[nextPatrolPoint]));
            }
        }
    }

    void FixedUpdate()
    {

        if (chasing)
        {

        }
        else if (searching)
        {

        }

        float localChaseMultiplier = 1f;

        if (chasing)
            localChaseMultiplier = chaseMultiplier;

        if (nextNode < path.Count)
            rb2d.MovePosition((Vector2)transform.position + (path[nextNode].WorldPosition - (Vector2)transform.position).normalized * Time.fixedDeltaTime * movementSpeed * chaseMultiplier);
    }

    public void Alert(Vector2 position)
    {
        if (searching && nextNode < path.Count)
            return;

        patrolling = false;
        searching = true;

        path = finder.FindPath(finder.GetNode(transform.position), finder.GetNode(position));
        nextNode = 0;

    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (patrolPoints.Count < 0)
            return;

        Gizmos.color = new Color(255f, 100f / 255f, 0f);
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (i + 1 < patrolPoints.Count)
            {
                Gizmos.DrawLine(patrolPoints[i], patrolPoints[i + 1]);
            }

            Gizmos.DrawCube(patrolPoints[i], new Vector3(0.25f, 0.25f, 0.25f));

        }
        Gizmos.color = Color.yellow;
        if (path != null && nextNode < path.Count && path[nextNode] != null)
            Gizmos.DrawLine(transform.position, path[nextNode].WorldPosition);
        Gizmos.DrawWireSphere(transform.position, viewRadius);

    }
#endif

    private IEnumerable Search()
    {
        yield return new WaitForSeconds(searchTime);

        if (!chasing)
        {
            searching = false;
            patrolling = true;
        }

    }

}
