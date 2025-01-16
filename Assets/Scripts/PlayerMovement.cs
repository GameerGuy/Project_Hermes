using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Variables

    [SerializeField] private float jumpHeight;

    [SerializeField] private float groundedGravity;
    [SerializeField] private float airbourneGravity;
    [SerializeField] private float fallingGravity;

    [SerializeField] private float runAcceleration;
    [SerializeField] private float groundDeceleration;
    [SerializeField] private float maxRunSpeed;
    [SerializeField] private float turnSpeed;

    private PlayerInput inputActions;
    private Rigidbody _rigidbody;
    private Collider _collider;
    private Vector3 movementDir;
    private float currentGravity;
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
    }

    private void Start()
    {
        currentGravity = airbourneGravity;
    }

    private void Update()
    {
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

    private void HandleMovement()
    {
        Vector3 horizontalVector = GetHorizontalVelocity();
        float horizontalSpeed = horizontalVector.magnitude;

        if (movementDir.magnitude < 1) {

            if (horizontalSpeed <= 1) {
                _rigidbody.velocity = new Vector3(0, _rigidbody.velocity.y, 0);
                return;
            }

            _rigidbody.AddForce(horizontalVector.normalized * -1 * groundDeceleration, ForceMode.Acceleration);
            return;
        }

        _rigidbody.AddForce(movementDir.magnitude * transform.forward * runAcceleration, ForceMode.Acceleration);

        if (horizontalSpeed > maxRunSpeed) {
            _rigidbody.velocity = _rigidbody.velocity.normalized * maxRunSpeed;
        }
    }

    private void HandleRotation()
    {
        if (movementDir.magnitude < 1) return;

        Quaternion toRotation = Quaternion.LookRotation(movementDir.normalized, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, turnSpeed * Time.deltaTime);
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
        if (IsGrounded()) {
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
