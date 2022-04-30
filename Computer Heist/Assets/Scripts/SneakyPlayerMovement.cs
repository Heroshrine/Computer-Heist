using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public class SneakyPlayerMovement : PlayerMovement
{

    [Header("Sneaky")]
    public float runSmoothingMultiplier = 4f;
    [Space]
    public float footstepSizeRunning = 12f;
    public float footstepSizeSneaking = 2f;
    float currentFoostepSize;
    [Space]
    [Tooltip("In seconds for now. Will be changed to animation event.")]
    public float footstepSpeedRunning = 3f;
    [Tooltip("In seconds for now. Will be changed to animation event.")]
    public float footstepSpeedSneaking = 12f;
    float currentFootstepSpeed;
    [Space]
    public GameObject footstepPrefab;

    float sneakMovementMultiplier;

    InputAction run;

    float stepTime;
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

        currentFootstepSpeed = footstepSpeedSneaking;

        sneakMovementMultiplier = movementSmoothingMultiplier;
        currentFoostepSize = footstepSizeSneaking;
    }

    protected void OnRun(InputAction.CallbackContext cb)
    {
        if (cb.ReadValue<float>() < 0.5f)
            CancelRun(cb);

        movementSmoothingMultiplier = runSmoothingMultiplier;
        currentFootstepSpeed = footstepSpeedRunning;
        currentFoostepSize = footstepSizeRunning;
    }

    protected void CancelRun(InputAction.CallbackContext cb)
    {
        movementSmoothingMultiplier = sneakMovementMultiplier;
        currentFootstepSpeed = footstepSpeedSneaking;
        currentFoostepSize = footstepSizeSneaking;
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
        if (time >= currentFootstepSpeed && moveVector.magnitude > 0)
        {
            TriggerFootstep();
            time = 0;
        }
    }

    public void TriggerFootstep()
    {
        Footstep d = Instantiate(footstepPrefab, transform.position, Quaternion.identity).AddComponent<Footstep>();
        d.maxSize = currentFoostepSize;
    }
}
