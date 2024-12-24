using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float jumpHeight;
    [SerializeField] private float gravity;
    [SerializeField] private float runAcceleration;
    [SerializeField] private float maxRunSpeed;
    private PlayerInput inputActions;
    private Rigidbody rb;
    private Collider _collider;
    private Vector3 movement;
    private bool grounded;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        inputActions = new PlayerInput();
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += Jump;
    }

    private void Update()
    {
        IsGrounded();
        Vector2 move = inputActions.Player.Movement.ReadValue<Vector2>();
        movement = new Vector3(move.x, 0, move.y);
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleGravity();
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

    private void Jump(InputAction.CallbackContext context)
    {
        if (!grounded) return;
        float jumpForce = Mathf.Sqrt(2 * gravity * jumpHeight);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        
    }

    private void HandleMovement()
    {
        float horizontalSpeed = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z);
        if (horizontalSpeed > maxRunSpeed) return;
        rb.AddForce(movement * runAcceleration, ForceMode.Acceleration);
    }

    private void HandleGravity()
    {
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    private void IsGrounded() {
        const float OFFSET = 0.01f;
        float radius = _collider.bounds.extents.x - OFFSET;
        float maxDistance = (_collider.bounds.extents.y / 2) + (OFFSET * 10);
        grounded = Physics.SphereCast(_collider.bounds.center, radius, -transform.up, out RaycastHit hitInfo, maxDistance);
        print(grounded);
    }

}
