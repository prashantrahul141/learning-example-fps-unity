using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed = 7;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCoolDown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    private bool remainCrouched;

    [Header("Handle Slope")]
    public float maxSlopeAngle;
    private RaycastHit slopeRaycastHit;
    private bool exitingSlope;

    [Header("KeyBinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask GroundLayer;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState movementState;

    // player states
    public enum MovementState
    {
        WALKING,
        SPRINTING,
        CROUCHING,
        AIR
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        SpeedControl();
        StateHandler();
        rb.drag = grounded ? groundDrag : 0;
    }

    private void FixedUpdate()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.5f, GroundLayer);
        MovePlayer();
    }

    // Gets player inputs
    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCoolDown);
        }

        if (Input.GetKey(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            if (remainCrouched)
            {
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
                remainCrouched = false;
            }
        }

        if (!Input.GetKey(crouchKey))
        {
            remainCrouched = true;
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }


    // Manages player states
    private void StateHandler()
    {

        // set crouching
        if (Input.GetKey(crouchKey))
        {
            movementState = MovementState.CROUCHING;
            moveSpeed = crouchSpeed;
        }

        // set sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            movementState = MovementState.SPRINTING;
            moveSpeed = sprintSpeed;
        }

        // set walking
        else if (grounded)
        {
            movementState = MovementState.WALKING;
            moveSpeed = walkSpeed;
        }

        // set in air
        else
        {
            movementState = MovementState.AIR;
        }

    }

    // adds force to move player
    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope())
        {
            rb.AddForce(10f * moveSpeed * GetSlopeMoveDirection().normalized, ForceMode.Force);
            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 10f, ForceMode.Force);
            }
        }
        else if (grounded)
        {
            rb.AddForce(10f * moveSpeed * moveDirection.normalized, ForceMode.Force);
        }
        else if (!grounded)
        {

            rb.AddForce(10f * airMultiplier * moveSpeed * moveDirection.normalized, ForceMode.Force);
        }

        rb.useGravity = !OnSlope();
    }

    // clamps max speed of player 
    private void SpeedControl()
    {
        // limiting on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 horizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            if (horizontalVel.magnitude > moveSpeed)
            {
                Vector3 horizontalVelDirection = horizontalVel.normalized * moveSpeed;
                rb.velocity = new Vector3(horizontalVelDirection.x, rb.velocity.y, horizontalVelDirection.z);
            }
        }
    }

    // adds force for jumping 
    private void Jump()
    {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    // invoke to reset jump
    private void ResetJump()
    {
        exitingSlope = false;
        readyToJump = true;
    }

    // detects if standing on a float
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeRaycastHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeRaycastHit.normal);
            return angle < maxSlopeAngle && angle > 0;
        }

        return false;

    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeRaycastHit.normal);
    }
}
