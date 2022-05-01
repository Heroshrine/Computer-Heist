using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class SecurityCamera : MonoBehaviour
{
    public CameraType cameraBehavior;

    [Space]
    public float alertRadius = 8;
    public float alertRepeatTime = 0.3f;
    float alertRepeatWaitTime;

    [Space]
    public float cameraSpeed = 3f;
    public float minRotationAngle = -25;
    public float maxRotationAngle = 25;
    public float angleStopTime = 1f;

    float angleStopWaitTime;

    Quaternion startingRotation;
    Quaternion nextRotation;

    Transform player;

    [Header("Debug")]
    public bool showAlertRadius;

    private void Start()
    {
        minRotationAngle = -Mathf.Abs(minRotationAngle);
        maxRotationAngle = Mathf.Abs(maxRotationAngle);
        alertRepeatWaitTime = alertRepeatTime / 1.1f;
        startingRotation = transform.rotation;

        minRotationAngle += transform.rotation.eulerAngles.z;
        maxRotationAngle += transform.rotation.eulerAngles.z;

        nextRotation = Quaternion.Euler(new Vector3(0, 0, maxRotationAngle));
    }

    private void Update()
    {
        if (alertRepeatWaitTime < alertRepeatTime)
            alertRepeatWaitTime += Time.deltaTime;

        switch (cameraBehavior)
        {
            case CameraType.Stationary:
                UpdateStationary();
                break;
            case CameraType.Watchful:
                UpdateWatchful();
                break;
            case CameraType.Active:
                UpdateActive();
                break;
        }

        float zAngle = transform.localEulerAngles.z;
        if (zAngle > 180f)
            zAngle -= 360f;
        transform.localEulerAngles = new Vector3(0, 0, Mathf.Clamp(zAngle, minRotationAngle, maxRotationAngle));

        if (player != null)
        {
            if (alertRepeatWaitTime >= alertRepeatTime)
            {
                Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, alertRadius);
                for (int i = 0; i < cols.Length; i++)
                {
                    if (cols[i].CompareTag("dynamicEnemy"))
                        cols[i].GetComponent<DynamicEnemy>().Alert(player.position);
                }
            }
        }

    }

    private void UpdateStationary()
    {
        //nothing to see here
    }

    private void UpdateWatchful()
    {
        if (player != null)
        {
            Vector2 look = ((Vector2)(player.position - transform.position)).normalized;
            float lookAngle = Mathf.Atan2(look.y, look.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(transform.forward * (lookAngle + 90)), Time.deltaTime * cameraSpeed);
        }
        else
        {
            transform.localRotation = Quaternion.RotateTowards(transform.rotation, startingRotation, Time.deltaTime * cameraSpeed / 2);
        }
    }

    private void UpdateActive()
    {
        if (transform.localRotation == Quaternion.Euler(new Vector3(0, 0, minRotationAngle)))
        {
            if (angleStopWaitTime > angleStopTime)
            {
                nextRotation = Quaternion.Euler(new Vector3(0, 0, maxRotationAngle));
                angleStopWaitTime = 0;
            }
            angleStopWaitTime += Time.deltaTime;
        }

        if (transform.localRotation == Quaternion.Euler(new Vector3(0, 0, maxRotationAngle)))
        {
            if (angleStopWaitTime > angleStopTime)
            {
                nextRotation = Quaternion.Euler(new Vector3(0, 0, minRotationAngle));
                angleStopWaitTime = 0;
            }
            angleStopWaitTime += Time.deltaTime;
        }

        transform.localRotation = Quaternion.RotateTowards(transform.rotation, nextRotation, Time.deltaTime * cameraSpeed);
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
        Active
    }
}