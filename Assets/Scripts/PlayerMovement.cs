using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Variables

    [Range(1,50)][Tooltip("How high in units the player jumps with a full press")]
    [SerializeField] private float jumpHeight;

    [Range(1,50)][Tooltip("Force that keeps the player grounded")]
    [SerializeField] private float groundedGravity;

    [Range(1,50)][Tooltip("Gravity at the start of a jump")]
    [SerializeField] private float airbourneGravity;

    [Range(1,100)][Tooltip("Gravity when falling")]
    [SerializeField] private float fallingGravity;

    [Range(1,100)][Tooltip("Baseline movement acceleration")]
    [SerializeField] private float runAcceleration;

    [Range(1,100)][Tooltip("Movement deceleration when Grounded")]
    [SerializeField] private float groundDeceleration;
    
    [Range(1,100)][Tooltip("Max speed achievable with basic movement")]
    [SerializeField] private float maxRunSpeed;

    [Range(1,10)][Tooltip("Speed cap multiplier")]
    [SerializeField] private float sprintSpeedMult;

    [Range(1,100)][Tooltip("Baseline turn rate")]
    [SerializeField] private float baseTurnSpeed;

    [Range(1,100)][Tooltip("Minimum turn rate")]
    [SerializeField] private float minTurnSpeed;

    private PlayerInput inputActions;
    private Rigidbody _rigidbody;
    private Collider _collider;
    private Vector3 movementDir;
    private float currentTurnSpeed;
    private float currentGravity;
    private bool sprinting;
    private bool grounded;
    private bool falling;

    #endregion

    #region MonoBehaviours

    // Start is called before the first frame update
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        inputActions = new PlayerInput();
        inputActions.Player.Jump.started += OnJumpPressed;
        inputActions.Player.Jump.canceled += OnJumpReleased;
        inputActions.Player.Sprint.started += OnSprintPressed;
        inputActions.Player.Sprint.canceled += OnSprintReleased;
    }

    private void Start()
    {
        currentGravity = airbourneGravity;
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

    //private void OnDrawGizmos()
    //{
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
    //}

    #endregion

    #region Input Functions

    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        if (!grounded) return;
        float jumpForce = Mathf.Sqrt(2 * airbourneGravity * jumpHeight);
        _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        
    }
    private void OnJumpReleased(InputAction.CallbackContext context)
    {
        falling = true;
    }

    private void OnSprintPressed(InputAction.CallbackContext context)
    {
        sprinting = true;
    }

    private void OnSprintReleased(InputAction.CallbackContext context)
    {
        sprinting = false;
    }

    private void HandleMovement()
    {
        Vector3 horizontalVector = GetHorizontalVelocity();
        float horizontalSpeed = horizontalVector.magnitude;
        float maxSpeed = sprinting ? maxRunSpeed * sprintSpeedMult : maxRunSpeed;

        if (movementDir.magnitude < 1) {

            if (horizontalSpeed <= 1) {
                _rigidbody.velocity = new Vector3(0, _rigidbody.velocity.y, 0);
                return;
            }

            _rigidbody.AddForce(horizontalVector.normalized * -1 * groundDeceleration, ForceMode.Acceleration);
            return;
        }

        if (horizontalSpeed < maxSpeed) {
            _rigidbody.AddForce(movementDir.magnitude * transform.forward * runAcceleration, ForceMode.Acceleration);
        } else if (horizontalSpeed > maxSpeed) {
            _rigidbody.AddForce(horizontalVector.normalized * -1 * groundDeceleration, ForceMode.Acceleration);
        } else {
            _rigidbody.velocity = transform.forward * maxSpeed;
        }
    }

    private void HandleRotation()
    {
        if (movementDir.magnitude == 0) return;
        
        Quaternion toRotation = Quaternion.LookRotation(movementDir.normalized, Vector3.up);
        
        float horizontalSpeed = GetHorizontalVelocity().magnitude;
        float scaledTurnSpeed = baseTurnSpeed - (horizontalSpeed * horizontalSpeed / (maxRunSpeed * maxRunSpeed)) + 1;
        
        currentTurnSpeed = (horizontalSpeed <= maxRunSpeed) ? baseTurnSpeed: (scaledTurnSpeed > minTurnSpeed) ? scaledTurnSpeed : minTurnSpeed;

        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, currentTurnSpeed * Time.deltaTime);
    }

    private void HandleGravity()
    {
        _rigidbody.AddForce(Vector3.down * currentGravity, ForceMode.Acceleration);
    }

    #endregion

    #region Utility Functions

    private Vector3 GetHorizontalVelocity()
    {
        return new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
    }

    private bool IsGrounded() {
        const float OFFSET = 0.01f;
        float radius = _collider.bounds.extents.x - OFFSET;
        float maxDistance = (_collider.bounds.extents.y / 2) + (OFFSET * 10);
        grounded = Physics.SphereCast(_collider.bounds.center, radius, -transform.up, out RaycastHit hitInfo, maxDistance);
        return grounded;
    }

    private void AssignGravity()
    {
        if (grounded) {
            currentGravity = groundedGravity;
            falling = false;
        } else {
            falling = (_rigidbody.velocity.y <= 0 || falling);
            currentGravity = (falling) 
                ? fallingGravity 
                : airbourneGravity;
        }
    }

    #endregion
}
