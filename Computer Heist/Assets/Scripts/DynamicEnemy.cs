using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DynamicEnemy : MonoBehaviour
{
    Rigidbody2D rb2d;

    public float viewRadius = 4f;

    public LayerMask playerLayerMask;
    public LayerMask viewLayerMask;

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
    public float chaseMultiplier = 2f;

    bool chasing = false;
    bool stopMoving;

    [Header("Shooting")]
    public float shootRadius = 2.5f;
    public float shootSpeed = 0.5f;

    float shootWaitTime;

    Transform shootTarget;

    private void Awake()
    {
        finder = new PathFinder(pathGridWidth, pathGridHeight, pathGridOrigin);
    }

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        path = finder.FindPath(finder.GetNode(transform.position), finder.GetNode(patrolPoints[nextPatrolPoint]));
        shootWaitTime = shootSpeed / 1.5f;
    }

    private void Update()
    {

        if (path == null)
            return;

        if (shootTarget != null)
        {
            if (shootWaitTime >= shootSpeed)
            {
                Debug.Log("shoot");
                shootWaitTime = 0;
            }

            shootWaitTime += Time.deltaTime;
            return;
        }

        if (nextNode < path.Count && Vector2.Distance(transform.position, path[nextNode].WorldPosition) <= 0.0035f)
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
        else if (searching && !chasing && nextNode >= path.Count)
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

    private Transform player = null;
    void FixedUpdate()
    {
        if (path == null)
            return;

        Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, viewRadius, playerLayerMask);
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && !players[i].isTrigger)
            {
                player = players[i].transform;
                break;
            }
            player = null;
            shootTarget = null;
        }

        if (player != null)
        {
            Collider2D col = Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, viewRadius, viewLayerMask).collider;
            if (col != null && col.gameObject.layer == 6)
            {
                chasing = true;
                searching = false;
                patrolling = false;
            }
            else
            {
                if (chasing)
                {
                    searching = true;
                }
                chasing = false;
                player = null;
                shootTarget = null;
            }

            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, shootRadius, playerLayerMask);
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null && !targets[i].isTrigger && col != null && col.gameObject.layer == 6)
                {
                    shootTarget = targets[i].transform;
                    break;
                }
                shootTarget = null;
            }

            if (shootTarget == null)
                shootWaitTime = shootSpeed / 1.5f;
        }
        else
        {
            if (chasing)
            {
                searching = true;
            }
            chasing = false;
            player = null;
            shootTarget = null;
        }

        if (chasing)
        {
            path = finder.FindPath(finder.GetNode(transform.position), finder.GetNode(player.transform.position));
            nextNode = 1;
        }

        float localChaseMultiplier = 1f;

        if (chasing || searching)
            localChaseMultiplier = chaseMultiplier;

        if (path == null)
            path = finder.FindPath(finder.GetNode(transform.position), finder.GetNode(transform.position));

        if (nextNode < path.Count && !stopMoving)
            rb2d.MovePosition((Vector2)transform.position + (path[nextNode].WorldPosition - (Vector2)transform.position).normalized * Time.fixedDeltaTime * movementSpeed * localChaseMultiplier);
    }

    public void Alert(Vector2 position)
    {
        if (chasing || (searching && nextNode < path.Count))
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

        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRadius);

    }
#endif

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6 && !collision.isTrigger && Physics2D.Raycast(transform.position, (collision.transform.position - transform.position).normalized, viewRadius, viewLayerMask).collider != null)
        {
            stopMoving = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6 && !collision.isTrigger)
            stopMoving = false;
    }

}
