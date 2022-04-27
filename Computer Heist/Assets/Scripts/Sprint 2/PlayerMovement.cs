using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A class that gives basic top-down controls.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// The current instance of this object
    /// </summary>
    public static PlayerMovement Instance { get; private set; }

    /// <summary>
    /// A property that can be used by the BasicInteraction events list.
    /// Teleports the player to the position of the transform it is set equal to.
    /// </summary>
    public Transform NewPosition { set { transform.position = value.position; } }

    Inputs inputs;
    Inputs.DefaultActions defaultActions;

    [Header("Movement")]
    [Tooltip("Higher number is slower but smoother movement")]
    public float movementSmoothingMultiplier = 1f;

    Vector2 moveVector;

    Rigidbody2D rb2d;
    InputAction movement;

    [Header("Animations")]
    [Tooltip("The animation that will play when moving to the side")]
    public AnimationClip side;
    [Tooltip("The animation that will play when moving up")]
    public AnimationClip up;
    [Tooltip("The animation that will play when moving down")]
    public AnimationClip down;
    [Tooltip("The animation that will play when not moving")]
    public AnimationClip idle;

    Animator anim;

    private void Awake()
    {
        Instance = this;

        inputs = new Inputs();
        defaultActions = inputs.Default;
        movement = defaultActions.Movement;
    }

    private void OnEnable()
    {
        inputs.Enable();
        defaultActions.Enable();
        movement.Enable();

        movement.performed += OnMovement;
        movement.canceled += CancelMovement;
    }

    private void OnMovement(InputAction.CallbackContext cb)
    {
        if (Time.timeScale == 0)
            return;

        moveVector = cb.ReadValue<Vector2>();

        if (anim == null)
            return;

        else if (moveVector.y > 0)
        {
            anim.Play(up.name, 0);
            return;
        }
        else if (moveVector.y < 0)
        {
            anim.Play(down.name, 0);
            return;
        }
        else if (moveVector.x > 0)
        {
            anim.Play(side.name, 0);
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            return;
        }
        else if (moveVector.x < 0)
        {
            anim.Play(side.name, 0);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            return;
        }
    }
    private void CancelMovement(InputAction.CallbackContext cb)
    {
        moveVector = Vector2.zero;
        if (anim != null)
            anim.Play(idle.name, 0);
    }

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        Vector2 rb2dVelocity = rb2d.velocity;
        Vector2 movement = Vector2.SmoothDamp(transform.position, (Vector2)transform.position + moveVector, ref rb2dVelocity, Time.fixedDeltaTime * movementSmoothingMultiplier);
        rb2d.MovePosition(movement);
    }

    private void OnDestroy()
    {
        movement.performed -= OnMovement;
        movement.canceled -= CancelMovement;
    }
}
