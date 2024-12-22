using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerInput inputActions;
    [SerializeField] private float speed;
    private Rigidbody rb;
    private Vector2 movement;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new PlayerInput();
        inputActions.Player.Enable();
    }

    private void Update()
    {
        movement = inputActions.Player.Movement.ReadValue<Vector2>();
        //movement.x = Input.GetAxisRaw("Horizontal");
        //movement.z = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector3(movement.x, 0, movement.y) * speed;
    }

}
