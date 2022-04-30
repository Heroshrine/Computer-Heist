using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public class SneakyPlayerMovement : PlayerMovement
{

    [Header("Sneaky")]
    public float runSmoothingMultiplier = 4f;
    [Space]
    public float runFootstepScale = 12f;
    public float sneakFootstepScale = 4f;
    [Space]
    [Tooltip("In seconds for now. Will be changed to animation event")]
    public float runFootstepSpeed = 1.5f;
    [Tooltip("In seconds for now. Will be changed to animation event")]
    public float sneakFootstepSpeed = 4f;
    [Space]
    [Tooltip("Shown as a red circle around the player")]
    public float runAlertRadius = 3f;
    [Tooltip("Shown as a yellow circle around the player")]
    public float sneakAlertRadius = 1.5f;
    [Space]
    public GameObject footstepPrefab;

    float sneakMovementMultiplier;
    bool running = false;

    InputAction run;

    protected new void Awake()
    {
        base.Awake();

        run = defaultActions.Run;
    }

    protected new void OnEnable()
    {
        base.OnEnable();

        run.Enable();

        run.started += OnRun;
        run.canceled += CancelRun;
    }

    protected new void Start()
    {
        base.Start();

        sneakMovementMultiplier = movementSmoothingMultiplier;
    }

    protected void OnRun(InputAction.CallbackContext cb)
    {
        if (cb.ReadValue<float>() < 0.1f)
            CancelRun(cb);

        movementSmoothingMultiplier = runSmoothingMultiplier;
        running = true;
    }

    protected void CancelRun(InputAction.CallbackContext cb)
    {
        movementSmoothingMultiplier = sneakMovementMultiplier;
        running = false;
    }

    protected new void OnDestroy()
    {
        base.OnDestroy();

        run.started -= OnRun;
        run.canceled -= CancelRun;
    }

    float time;

    void Update()
    {
        time += Time.deltaTime;
        if (running && time >= runFootstepSpeed && moveVector.magnitude > 0)
        {
            TriggerFootstep(runFootstepScale);
            AlertEnemies(runAlertRadius);
            time = 0;
        }
        else if (time >= sneakFootstepSpeed && moveVector.magnitude > 0)
        {
            TriggerFootstep(sneakFootstepScale);
            AlertEnemies(sneakAlertRadius);
            time = 0;
        }
    }

    public void TriggerFootstep(float scale)
    {
        Footstep d = Instantiate(footstepPrefab, transform.position, Quaternion.identity).AddComponent<Footstep>();
        d.maxSize = scale;
    }

    public void AlertEnemies(float radius)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, radius);

        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i].CompareTag("dynamicEnemy"))
            {
                cols[i].GetComponent<DynamicEnemy>().Alert(transform.position);
            }
        }

    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, runAlertRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sneakAlertRadius);
    }
#endif

    public class Footstep : MonoBehaviour
    {
        public float maxSize = 4;
        private void Update()
        {
            transform.localScale = new Vector3(transform.localScale.x + Time.deltaTime * maxSize, transform.localScale.y + Time.deltaTime * maxSize, transform.localScale.z + Time.deltaTime * maxSize);
        }

        public void Destroy() => Destroy(gameObject);
    }
}
