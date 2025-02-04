using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Variables

    [Range(1,50)][Tooltip("How high in units the player jumps with a full press")]
    [SerializeField] private float jumpHeight;

    [Range(-50, 50)][Tooltip("Velocity threshold for the player to start falling faster")]
    [SerializeField] private float fallingThreshold;

    [Range(1,50)][Tooltip("Force that keeps the player grounded")]
    [SerializeField] private float groundedGravity;

    [Range(1,50)][Tooltip("Gravity at the start of a jump")]
    [SerializeField] private float airbourneGravity;

    [Range(1,100)][Tooltip("Gravity when falling")]
    [SerializeField] private float fallingGravity;

    [Range(1,100)][Tooltip("Baseline movement acceleration")]
    [SerializeField] private float runAcceleration;

    [Range(1,100)][Tooltip("Movement deceleration when grounded")]
    [SerializeField] private float groundDeceleration;

    [Range(1,100)][Tooltip("Movement deceleration when in the air")]
    [SerializeField] private float airbourneDeceleration;
    
    [Range(1,100)][Tooltip("Max speed achievable with basic movement")]
    [SerializeField] private float maxRunSpeed;

    [Range(1,10)][Tooltip("Speed cap multiplier")]
    [SerializeField] private float sprintSpeedMult;

    [Range(1, 1000)][Tooltip("Max speed achievable with basic movement")]
    [SerializeField] private float trueSpeedCap;

    [Range(1,100)][Tooltip("Baseline turn rate")]
    [SerializeField] private float baseTurnSpeed;

    [Range(0, 100)][Tooltip("Turn speed scaling factor")]
    [SerializeField] private float turnScaling;

    [Range(1,100)][Tooltip("Minimum turn rate")]
    [SerializeField] private float minTurnSpeed;
    
    [Range(0,10)][Tooltip("Multiplyer for the initial speed boost when sliding (proportional to current speed)")]
    [SerializeField] private float slideMult;

    [Range(0, 5000)][Tooltip("Minimum time between slides")]
    [SerializeField] private int slideCooldown;

    [Range(1,100)][Tooltip("Downwards force applied when diving")]
    [SerializeField] private float diveForce;

    [Range(0, 10)][Tooltip("Horizontal speed penalty applied when diving")]
    [SerializeField] private float divePenalty;

    [Tooltip("Does the print button need to be held or is it a toggle?")]
    [SerializeField] private bool toggleSprint;

    private event EventHandler OnGroundedEvent;
    private event EventHandler OnAirbourneEvent;
    private event EventHandler OnSlideEvent;

    private PlayerInput inputActions;
    private Rigidbody _rigidbody;
    private Collider _collider;
    private Vector3 movementDir;
    private float currentTurnSpeed;
    private float currentDeceleration;
    private float currentGravity;
    private bool sprinting;
    private bool crouching;
    private bool grounded;
    private bool falling;
    private bool sliding;
    private bool diving;
    #endregion



    #region MonoBehaviours
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        inputActions = new PlayerInput();

        inputActions.Player.Jump.started += OnJumpPressed;
        inputActions.Player.Jump.canceled += OnJumpReleased;
        inputActions.Player.Sprint.started += OnSprintPressed;
        inputActions.Player.Sprint.canceled += OnSprintReleased;
        inputActions.Player.Crouch.started += OnCrouchPressed;
        inputActions.Player.Crouch.canceled += OnCrouchReleased;

        OnGroundedEvent += OnGrounded;
        OnAirbourneEvent += OnAirbourne;
        OnSlideEvent += OnSlide;
    }

    private void Start()
    {
        currentGravity = fallingGravity;
        currentDeceleration = airbourneDeceleration;
        currentTurnSpeed = baseTurnSpeed;
    }

    private void Update()
    {
        IsGrounded();
        AssignGravity();
        Vector2 move = inputActions.Player.Movement.ReadValue<Vector2>();
        movementDir = new Vector3(move.x, 0, move.y);
    }

    private void FixedUpdate()
    {   
        HandleRotation();
        HandleMovement();
        HandleGravity();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    // private void OnDrawGizmos()
    // {
    //    const float OFFSET = 0.01f;
    //    float radius = _collider.bounds.extents.x - OFFSET;
    //    float maxDistance = (_collider.bounds.extents.y / 2) + (OFFSET * 10);
    //    Physics.SphereCast(_collider.bounds.center, radius, -transform.up, out RaycastHit hitInfo, maxDistance);
    //    if (grounded)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawRay(_collider.bounds.center, -transform.up * hitInfo.distance);
    //        Gizmos.DrawWireSphere(_collider.bounds.center + -transform.up * hitInfo.distance, radius);
    //    }
    //    else
    //    {
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawRay(_collider.bounds.center, -transform.up * maxDistance);
    //        Gizmos.DrawWireSphere(_collider.bounds.center + -transform.up * maxDistance, radius);
    //    }
    // }
    #endregion



    #region Input Functions
    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        if (!grounded) return;
        float jumpForce = Mathf.Sqrt(2 * airbourneGravity * jumpHeight);
        _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

        if (!sliding) return;
        sliding = false;
        transform.localScale = new Vector3(1, 1, 1);
    }

    private void OnJumpReleased(InputAction.CallbackContext context)
    {
        // if (sliding){
        //     sliding = false;
        //     return;
        // }
        falling = !grounded;
    }

    private void OnSprintPressed(InputAction.CallbackContext context)
    {
        sprinting = (!toggleSprint) ? true : !sprinting;
    }

    private void OnSprintReleased(InputAction.CallbackContext context)
    {
        sprinting = (!toggleSprint) ? false : sprinting;
    }

    private void OnCrouchPressed(InputAction.CallbackContext context)
    {
        crouching = true;
        Vector3 horizontalVector = GetHorizontalVelocity();
        if (grounded) {
            transform.localScale = new Vector3(1, .5f, 1);

            float horizontalSpeed = horizontalVector.magnitude;
            if (horizontalSpeed < maxRunSpeed - 1 || sliding) return;

            OnSlideEvent?.Invoke(this, EventArgs.Empty);
            return;
        } 
        if (diving) return;

        falling = true;
        diving = true;

        Vector3 finalDiveForce = (transform.up * diveForce + horizontalVector * divePenalty) * -1;
        _rigidbody.AddForce(finalDiveForce, ForceMode.VelocityChange);
        
    }

    private void OnCrouchReleased(InputAction.CallbackContext context)
    {
        crouching = false;
        if (!sliding) transform.localScale = new Vector3(1, 1, 1);
    }
    #endregion



    #region Handler Functions
    private void HandleMovement()
    {
        Vector3 verticalVector = GetVerticalVelocity();
        Vector3 horizontalVector = GetHorizontalVelocity();
        float horizontalSpeed = horizontalVector.magnitude;
        float maxSpeed = sprinting ? maxRunSpeed * sprintSpeedMult : maxRunSpeed;
        //float turningLeniency = Mathf.Sin(Vector3.Angle(movementDir, transform.forward) * Mathf.Deg2Rad);

        if (movementDir.magnitude == 0 || crouching) {

            if (horizontalSpeed <= 1) {
                _rigidbody.velocity = verticalVector;
                return;
            }

            _rigidbody.AddForce(horizontalVector.normalized * -1 * currentDeceleration, ForceMode.Acceleration);
            return;
        }

        if (horizontalSpeed < maxSpeed) {
            _rigidbody.AddForce(movementDir.magnitude * transform.forward * runAcceleration, ForceMode.Acceleration);
        } else if (horizontalSpeed > maxSpeed) {
            _rigidbody.AddForce(horizontalVector.normalized * -1 * currentDeceleration, ForceMode.Acceleration);
        } else {
            _rigidbody.velocity = transform.forward * maxSpeed + verticalVector;
        }
    }

    private void HandleRotation()
    {
        if (movementDir.magnitude == 0) return;
        
        Quaternion toRotation = Quaternion.LookRotation(movementDir.normalized, Vector3.up);
        
        float horizontalSpeed = GetHorizontalVelocity().magnitude;
        float scaledTurnSpeed = baseTurnSpeed - turnScaling * (horizontalSpeed * horizontalSpeed / (maxRunSpeed * maxRunSpeed)) + turnScaling;
        
        currentTurnSpeed = (horizontalSpeed <= maxRunSpeed) ? baseTurnSpeed: (scaledTurnSpeed > minTurnSpeed) ? scaledTurnSpeed : minTurnSpeed;
        
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, currentTurnSpeed * Time.deltaTime);
    }

    private void HandleGravity()
    {
        _rigidbody.AddForce(Vector3.down * currentGravity, ForceMode.Acceleration);
    }
    #endregion



    #region Event Functions
    private void OnGrounded(object sender, EventArgs e)
    {
        currentGravity = groundedGravity;
        currentDeceleration = groundDeceleration;
        falling = false;
        diving = false;
    }

    private void OnAirbourne(object sender, EventArgs e)
    {
        currentDeceleration = airbourneDeceleration;
    }

    private void OnSlide(object sender, EventArgs e)
    {
        Vector3 horizontalVector = GetHorizontalVelocity();
        float slideForce = horizontalVector.magnitude * slideMult;

        if (slideForce + horizontalVector.magnitude >= trueSpeedCap) {
            _rigidbody.velocity -= horizontalVector;
            slideForce = trueSpeedCap;
        }

        //float turnSoftening = movementDir.magnitude > 0 ? Mathf.Abs(Vector3.Dot(horizontalVector.normalized, movementDir)): 1;
        _rigidbody.AddForce(transform.forward * slideForce, ForceMode.VelocityChange);
        SlideCooldown();
    }
    #endregion



    #region Utility Functions
    private Vector3 GetHorizontalVelocity()
    {
        return new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
    }

    private Vector3 GetVerticalVelocity()
    {
        return new Vector3(0, _rigidbody.velocity.y, 0);
    }

    private bool IsGrounded() {
        bool wasGrounded = grounded;

        const float OFFSET = 0.01f;
        float radius = _collider.bounds.extents.x - OFFSET;
        float maxDistance = (_collider.bounds.extents.y / 2) + (OFFSET * 10);
        grounded = Physics.SphereCast(_collider.bounds.center, radius, -transform.up, out RaycastHit hitInfo, maxDistance);

        if (wasGrounded == grounded) return grounded;

        if (grounded) {
            OnGroundedEvent?.Invoke(this, EventArgs.Empty);
        } else {
            OnAirbourneEvent?.Invoke(this, EventArgs.Empty);
        }
        return grounded;
    }

    private void AssignGravity()
    {
        if (!grounded) {
            falling = (_rigidbody.velocity.y <= fallingThreshold || falling);
            currentGravity = 
                  (!falling) ? airbourneGravity
                : (!diving) ?  fallingGravity
                : fallingGravity * 3;
        }
    }

    private async void SlideCooldown()
    {
        sliding = true;
        await Task.Delay(slideCooldown);

        float horzontalSpeed = GetHorizontalVelocity().magnitude;
        while (horzontalSpeed > maxRunSpeed * sprintSpeedMult && sliding) {
            horzontalSpeed = GetHorizontalVelocity().magnitude;
            await Task.Delay(20);
            continue;
        }

        sliding = false;
        if (!crouching) transform.localScale = new Vector3(1, 1, 1);
    }
    #endregion
}
