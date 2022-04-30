using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class SecurityCamera : MonoBehaviour
{
    public CameraType cameraBehavior;

    [Space]
    public float alertRadius = 8;

    [Space]
    public float maxLeftRotation = 25;
    public float maxRightRotation = 25;

    [Header("Debug")]
    public bool showAlertRadius;

    Transform player;

    private void Start()
    {
        maxLeftRotation = -Mathf.Abs(maxLeftRotation);
        maxRightRotation = Mathf.Abs(maxLeftRotation);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (showAlertRadius)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, alertRadius);
        }
    }
#endif

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (player == null && collision.gameObject.layer == 6 && !collision.isTrigger)
        {
            player = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (player != null && collision.gameObject.layer == 6 && !collision.isTrigger)
        {
            player = null;
        }
    }

    public enum CameraType
    {
        Stationary,
        Watchful,
        Patrol
    }

}
