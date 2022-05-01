using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;

using UnityEngine;

using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(Rigidbody2D))]
public class DynamicEnemy : MonoBehaviour
{
    Rigidbody2D rb2d;

    public float viewRadius = 4f;

    public LayerMask playerLayerMask;
    public LayerMask viewLayerMask;

    private Transform player = null;

    [Header("PathFinder")]
    public int pathGridWidth;
    public int pathGridHeight;
    [Tooltip("The bottom left of the pathfinder's grid")]
    public Vector2 pathGridOrigin;
    public LayerMask wallLayerMask;

    PathFinder finder;
    List<PathNode> path;
    int nextNode;

    [Header("Patrolling")]
    public float searchTime = 2f;
    [Tooltip("How long the enemy will wait at the end of their patrol before continuing. Additive with endOfRoutTime.")]
    public float endOfRouteTime = 0f;
    [Tooltip("How long the enemy will wait at each patrol point")]
    public float reachedPatrolPointTime = 0f;

    public List<Vector2> patrolPoints;

    int nextPatrolPoint;
    bool patrolling = true;

    float searchWaitTime;
    bool searching;

    float endOfRouteWaitTime;
    float reachedPatrolPointWaitTime;
    bool waiting;

    [Header("Movement")]
    public float movementSpeed = 5f;
    public float chaseMultiplier = 2f;
    public float stopDistance = 1.25f;

    bool chasing = false;
    bool stopMoving;

    [Header("Shooting")]
    public float shootRadius = 2.5f;
    public float shootSpeed = 0.5f;
    public float shotVelocity = 4f;

    public Bullet bullet;

    float shootWaitTime;

    Transform shootTarget;

    [Header("Debug")]
    public bool showPath = true;
    public bool showDirection = true;
    public bool showViewDistance;
    public bool showFireRange;
    public bool showStopDistance;

    private void Awake()
    {
        finder = new PathFinder(pathGridWidth, pathGridHeight, wallLayerMask, pathGridOrigin);
    }

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        path = finder.FindPath(finder.GetNode(transform.position), finder.GetNode(patrolPoints[nextPatrolPoint]));
        shootWaitTime = shootSpeed / 1.5f;
    }

    private void Update()
    {
        if (player != null)
        {
            if (Vector2.Distance(transform.position, player.position) <= stopDistance)
                stopMoving = true;
            else if (stopMoving)
                StartCoroutine(StartMoving());
        }

        if (shootTarget != null)
        {
            if (shootWaitTime >= shootSpeed)
            {
                Shoot();
                Debug.Log("shoot");
                shootWaitTime = 0;
            }

            shootWaitTime += Time.deltaTime;
            return;
        }

        if (path == null)
        {
            path = finder.FindPath(finder.GetNode(transform.position), finder.GetNode(transform.position));
            nextNode = 0;
        }

        if (nextNode < path.Count && Vector2.Distance(transform.position, path[nextNode].WorldPosition) <= 0.0035f)
        {
            nextNode++;
            if (nextNode >= path.Count)
            {
                if (patrolling)
                {
                    if (reachedPatrolPointWaitTime < reachedPatrolPointTime)
                    {
                        nextNode--;
                        reachedPatrolPointWaitTime += Time.deltaTime;
                        waiting = true;
                        transform.position = path[nextNode].WorldPosition;
                        return;
                    }

                    nextPatrolPoint++;

                    if (nextPatrolPoint >= patrolPoints.Count)
                    {
                        if (endOfRouteWaitTime < endOfRouteTime)
                        {
                            nextNode--;
                            endOfRouteWaitTime += Time.deltaTime;
                            nextPatrolPoint--;
                            waiting = true;
                            transform.position = path[nextNode].WorldPosition;
                            return;
                        }

                        patrolPoints.Reverse();
                        nextPatrolPoint = 1;
                    }

                    reachedPatrolPointWaitTime = 0;
                    endOfRouteWaitTime = 0;
                    nextNode = 0;
                    waiting = false;
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

    void FixedUpdate()
    {
        if (path == null)
        {
            path = finder.FindPath(finder.GetNode(transform.position), finder.GetNode(transform.position));
            nextNode = 0;
        }

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
                waiting = false;
                player.GetComponent<SneakyPlayerMovement>().seen = true;
            }
            else if (!patrolling)
            {
                if (chasing)
                    searching = true;

                player.GetComponent<SneakyPlayerMovement>().seen = false;

                chasing = false;
                player = null;
                shootTarget = null;
                waiting = false;
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

            if (path == null)
                return;

            if (path.Count > 1)
                nextNode = 1;
            else
                nextNode = 0;
        }

        float localChaseMultiplier = 1f;

        if (chasing || searching)
            localChaseMultiplier = chaseMultiplier;

        if (path == null)
            path = finder.FindPath(finder.GetNode(transform.position), finder.GetNode(transform.position));

        if (nextNode < path.Count && !stopMoving && !waiting)
            rb2d.MovePosition((Vector2)transform.position + (path[nextNode].WorldPosition - (Vector2)transform.position).normalized * Time.fixedDeltaTime * movementSpeed * localChaseMultiplier);
    }

    public void Alert(Vector2 position)
    {
        if (chasing) //(searching && nextNode < path.Count)
            return;

        patrolling = false;
        searching = true;

        path = finder.FindPath(finder.GetNode(transform.position), finder.GetNode(position));
        if (path.Count > 1)
            nextNode = 1;
        else
            nextNode = 0;

    }

    public void Shoot()
    {
        if (shootTarget == null)
            return;

        Bullet newBullet = Instantiate(bullet, (transform.position + (shootTarget.position - transform.position).normalized / 2f), transform.rotation);
        newBullet.transform.right = shootTarget.position - newBullet.transform.position;
        newBullet.bulletSpeed = shotVelocity;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (patrolPoints.Count < 0)
            return;

        if (showPath)
        {
            Gizmos.color = new Color(255f, 100f / 255f, 0f);
            for (int i = 0; i < patrolPoints.Count; i++)
            {
                if (i + 1 < patrolPoints.Count)
                {
                    Gizmos.DrawLine(patrolPoints[i], patrolPoints[i + 1]);
                }

                Gizmos.DrawCube(patrolPoints[i], new Vector3(0.25f, 0.25f, 0.25f));

            }
        }

        if (showViewDistance)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, viewRadius);
        }

        if (player != null && showFireRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }

        if (showFireRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, shootRadius);
        }

        if (showStopDistance)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stopDistance);
        }

        if (showDirection && path != null && nextNode < path.Count && path[nextNode] != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, path[nextNode].WorldPosition);
            Gizmos.color = new Color(1f, 215f / 255f, 0f, 0.8f);
            Gizmos.DrawLine(transform.position, path[^1].WorldPosition);
        }
    }
#endif

    private IEnumerator StartMoving()
    {
        yield return new WaitForSeconds(0.4f);
        if (player != null && Vector2.Distance(transform.position, player.position) > stopDistance)
            stopMoving = false;
        else if (player == null)
            stopMoving = false;
    }
}