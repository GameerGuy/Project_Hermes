using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Mathematics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public class PlayerMovement : NetworkBehaviour
{
    #region Variables
    [Header("")]
    [Range(1,50)][Tooltip("How high in units the player jumps with a full press")]
    [SerializeField] private float jumpHeight;

    [Range(1,50)][Tooltip("Force that keeps the player grounded")]
    [SerializeField] private float groundedGravity;

    [Range(1,50)][Tooltip("Gravity at the start of a jump")]
    [SerializeField] private float airbourneGravity;

    [Range(1,100)][Tooltip("Gravity when falling")]
    [SerializeField] private float fallingGravity;

    [Header("")]
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

    [Header("")]
    [Range(1,100)][Tooltip("Baseline turn rate")]
    [SerializeField] private float baseTurnSpeed;

    [Range(0, 100)][Tooltip("Turn speed scaling factor")]
    [SerializeField] private float turnScaling;

    [Range(1,100)][Tooltip("Minimum turn rate")]
    [SerializeField] private float minTurnSpeed;

    [Header("")]
    [Range(0,10)][Tooltip("Multiplyer for the initial speed boost when sliding (proportional to current speed)")]
    [SerializeField] private float slideMult;

    [Range(0, 5000)][Tooltip("Minimum time between slides")]
    [SerializeField] private int slideCooldown;

    [Header("")] 
    [Range(1,100)][Tooltip("Downwards force applied when diving")]
    [SerializeField] private float diveForce;

    [Range(0, 10)][Tooltip("Horizontal speed penalty applied when diving")]
    [SerializeField] private float divePenalty;

    [Header("")]
    [SerializeField] private CustomCamera _customCamera;
    public CustomCamera customCamera => _customCamera;
    [SerializeField] private GameObject respawnPoint;
    [SerializeField] private TrailRenderer[] Trails;

    private event EventHandler OnGroundedEvent;
    private event EventHandler OnAirbourneEvent;
    private event EventHandler<SlideEventArgs> OnSlideEvent;
    private class SlideEventArgs : EventArgs {
        public bool powerSlide;
    }

    private OwnerNetworkAnimator networkAnimator;
    private SplineProjector respawnProjector;
    private CapsuleCollider _collider;
    private PlayerInput inputActions;
    private Rigidbody _rigidbody;
    private Animator animator;
    private Vector3 movementDir;
    private float currentTurnSpeed;
    private float currentDeceleration;
    private float currentGravity;
    private float fallingTimer;
    private bool toggleSprint;
    private bool sprinting;
    private bool crouching;
    private bool grounded;
    private bool canJump;
    private bool falling;
    private bool sliding;
    private bool diving;
    #endregion



    #region MonoBehaviours
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();

        animator = GetComponentInChildren<Animator>();
        networkAnimator = GetComponentInChildren<OwnerNetworkAnimator>();
        inputActions = InputManager.inputActions;
        
    }

    private void Start()
    {
        currentGravity = fallingGravity;
        currentDeceleration = airbourneDeceleration;
        currentTurnSpeed = baseTurnSpeed;
        fallingTimer = 0.5f;
        toggleSprint = PlayerPrefs.GetInt(InputManager.TOGGLE_SPRINT_KEY) != 0;
        DisableTrails();

        if (IsOwner) {
            SpawnDependents();
            IsGrounded();
            if (grounded){
                OnGroundedEvent?.Invoke(this, EventArgs.Empty);
            } else {
                OnAirbourneEvent?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            if (sprinting) { EnableTrailsServerRpc(); }
            else { DisableTrails(); }
            return;
        }
        IsGrounded();
        AssignGravity();
        Vector2 move = inputActions.Player.Movement.ReadValue<Vector2>();
        movementDir = new Vector3(move.x, 0, move.y);
    }

    private void FixedUpdate()
    {   
        if (!IsOwner) return;

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

    private void OnCollisionEnter(Collision other)
    {
        // if (other.gameObject.CompareTag("Path")) {
        //     respawnProjector.spline = other.gameObject.GetComponent<SplineComputer>();
        //     respawnProjector.projectTarget = transform;
        //     return;
        // }

        if (other.gameObject.CompareTag("Respawner")) {
            Respawn();
            return;
        }

    }

    private void OnDrawGizmos()
    {
        const float OFFSET = 0.03f;
        float radius = _collider.bounds.extents.x - OFFSET;
        float maxDistance = (_collider.bounds.extents.y / 2) + (OFFSET * 10);
        Physics.SphereCast(_collider.bounds.center, radius, -transform.up, out RaycastHit hitInfo, maxDistance);
        if (grounded) {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_collider.bounds.center, -transform.up * hitInfo.distance);
            Gizmos.DrawWireSphere(_collider.bounds.center + -transform.up * hitInfo.distance, radius);
        } else {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(_collider.bounds.center, -transform.up * maxDistance);
            Gizmos.DrawWireSphere(_collider.bounds.center + -transform.up * maxDistance, radius);
        }
    }
    #endregion



    #region NetworkBehaviours

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.RegisterPlayer(OwnerClientId, this);
        if (!IsOwner) {
            return; 
        }

        inputActions.Player.Jump.started += OnJumpPressed;
        inputActions.Player.Jump.canceled += OnJumpReleased;
        inputActions.Player.Sprint.started += OnSprintPressed;
        inputActions.Player.Sprint.canceled += OnSprintReleased;
        inputActions.Player.Crouch.started += OnCrouchPressed;
        inputActions.Player.Crouch.canceled += OnCrouchReleased;
        inputActions.Player.Respawn.started += OnRespawnPressed;

        OnGroundedEvent += OnGrounded;
        OnAirbourneEvent += OnAirbourne;
        OnSlideEvent += OnSlide;
    }

    public override void OnDestroy()
    {
        inputActions.Player.Jump.started -= OnJumpPressed;
        inputActions.Player.Jump.canceled -= OnJumpReleased;
        inputActions.Player.Sprint.started -= OnSprintPressed;
        inputActions.Player.Sprint.canceled -= OnSprintReleased;
        inputActions.Player.Crouch.started -= OnCrouchPressed;
        inputActions.Player.Crouch.canceled -= OnCrouchReleased;
        inputActions.Player.Respawn.started -= OnRespawnPressed;
        base.OnDestroy();
    }

    private void SpawnDependents()
    {
        respawnPoint = Instantiate(respawnPoint, transform.position, quaternion.identity);
        respawnProjector = respawnPoint.GetComponent<SplineProjector>();

        _customCamera = Instantiate(customCamera, transform.position, Quaternion.identity).GetComponent<CustomCamera>();
        _customCamera.SetTargetForAll(transform);
    }
    #endregion



    #region Input Functions
    public void EnableInput()
    {
        inputActions.Enable();
    }

    public void DisableInput()
    {
        print("disabled");
        inputActions.Disable();
    }
    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        if (!canJump) return;
        float jumpForce = Mathf.Sqrt(2 * airbourneGravity * jumpHeight);
        _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        canJump = false;

        if (!sliding) {
            networkAnimator.SetTrigger("Jump");
            return;
        }
        networkAnimator.SetTrigger("SlideJump");
        sliding = false;
        SetColliderToStand();
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
        if (!toggleSprint) {
            sprinting = true;
            EnableTrailsServerRpc();
            return;
        }

        sprinting = !sprinting;
        if (sprinting) { EnableTrailsServerRpc(); }
        else { DisableTrails(); }
    }

    private void OnSprintReleased(InputAction.CallbackContext context)
    {
        if (!toggleSprint)
        {
            sprinting = false;
            DisableTrails();
            return;
        }
    }

    private void OnCrouchPressed(InputAction.CallbackContext context)
    {
        crouching = true;
        animator.SetBool("IsCrouching", true);  

        Vector3 horizontalVector = GetHorizontalVelocity();
        if (grounded) {
            SetColliderToCrounch();

            float horizontalSpeed = horizontalVector.magnitude;
            if (horizontalSpeed < maxRunSpeed / 2 || sliding) return;

            OnSlideEvent?.Invoke(this, new SlideEventArgs {powerSlide =  horizontalSpeed >= maxRunSpeed / 2} );
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
        animator.SetBool("IsCrouching", false);
        if (!sliding) SetColliderToStand();
    }

    private void OnRespawnPressed(InputAction.CallbackContext context)
    {
        Respawn();
    }
    #endregion



    #region Handler Functions
    private void HandleMovement()
    {
        Vector3 verticalVector = GetVerticalVelocity();
        Vector3 horizontalVector = GetHorizontalVelocity();
        float cameraAngle = _customCamera.GetRotation() * Mathf.Deg2Rad;
        float horizontalSpeed = horizontalVector.magnitude;
        float maxSpeed = sprinting ? maxRunSpeed * sprintSpeedMult : maxRunSpeed;
        //float turningLeniency = Mathf.Sin(Vector3.Angle(horizontalVector, transform.forward) * Mathf.Deg2Rad);

        if (movementDir.magnitude == 0 || crouching) {

            if (horizontalSpeed <= 1) {
                _rigidbody.velocity = verticalVector;
                animator.SetBool("IsMoving", false);
                return;
            }

            _rigidbody.AddForce(horizontalVector.normalized * -1 * currentDeceleration, ForceMode.Acceleration);
            return;
        }

        animator.SetBool("IsMoving", true);
        animator.SetFloat("runAnimSpeed",(sprinting) ? 2 : 1);
    
        float rotatedX = movementDir.x * MathF.Cos(cameraAngle) + movementDir.z * MathF.Sin(cameraAngle);
        float rotatedZ = movementDir.z * MathF.Cos(cameraAngle) - movementDir.x * MathF.Sin(cameraAngle);

        Vector3 finalMoveDir = new Vector3(rotatedX , 0, rotatedZ);

        if (horizontalSpeed < maxSpeed) {
            _rigidbody.AddForce(finalMoveDir.normalized * runAcceleration, ForceMode.Acceleration);
        } else if (horizontalSpeed > maxSpeed) {
            _rigidbody.AddForce(horizontalVector.normalized * -1 * currentDeceleration, ForceMode.Acceleration);
        } else {
            _rigidbody.velocity = transform.forward * finalMoveDir.magnitude * maxSpeed + verticalVector;
        }
    }

    private void HandleRotation()
    {
        if (movementDir.magnitude == 0) return;
        float cameraRotation = _customCamera.GetRotation();
        Quaternion toRotation = Quaternion.LookRotation(movementDir.normalized, Vector3.up);
        toRotation.eulerAngles += new Vector3(0, cameraRotation, 0);
        
        float horizontalSpeed = GetHorizontalVelocity().magnitude;
        float scaledTurnSpeed = baseTurnSpeed - turnScaling * (horizontalSpeed * horizontalSpeed / (maxRunSpeed * maxRunSpeed)) + turnScaling;
        
        currentTurnSpeed = (horizontalSpeed <= maxRunSpeed) ? baseTurnSpeed: (scaledTurnSpeed > minTurnSpeed) ? scaledTurnSpeed : minTurnSpeed;
        
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, currentTurnSpeed * Time.deltaTime);
    }

    private void HandleGravity()
    {
        _rigidbody.AddForce(Vector3.down * currentGravity, ForceMode.Acceleration);
    }


    private void Respawn()
    {
        _rigidbody.velocity = Vector3.zero;
        transform.position = respawnPoint.transform.position + new Vector3(0, 1, 0);
        transform.rotation = respawnPoint.transform.rotation;
    }
    #endregion



    #region Event Functions
    private void OnGrounded(object sender, EventArgs e)
    {
        canJump = true;

        currentGravity = groundedGravity;
        currentDeceleration = groundDeceleration;
        respawnProjector.projectTarget = transform;

        falling = false;
        diving = false;
        fallingTimer = 0.5f;
        animator.SetBool("IsFalling", false);

    }

    private void OnAirbourne(object sender, EventArgs e)
    {
        grounded = false;
        JumpLenience();
        currentDeceleration = airbourneDeceleration;
        respawnProjector.projectTarget = respawnPoint.transform;
    }

    private void OnSlide(object sender, SlideEventArgs e)
    {
        networkAnimator.SetTrigger("Slide");
        if (e.powerSlide)   {
            Vector3 horizontalVector = GetHorizontalVelocity();
            float slideForce = horizontalVector.magnitude * slideMult;

            if (slideForce + horizontalVector.magnitude >= trueSpeedCap) {
                _rigidbody.velocity -= horizontalVector;
                slideForce = trueSpeedCap;
            }

            //float turnSoftening = movementDir.magnitude > 0 ? Mathf.Abs(Vector3.Dot(horizontalVector.normalized, movementDir)): 1;
            _rigidbody.AddForce(transform.forward * slideForce, ForceMode.VelocityChange);
        }
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

    private void SetColliderToStand()
    {
        _collider.center = Vector3.zero;
        _collider.height = 2;
    }

    private void SetColliderToCrounch()
    {
        _collider.center = new Vector3(0, -0.5f, 0);
        _collider.height = 1;
    }

    [ServerRpc(RequireOwnership = false)]
    private void EnableTrailsServerRpc(ServerRpcParams serverRpcParams = default)
    {
        EnableTrailsClientRpc();
    }

    [ClientRpc]
    private void EnableTrailsClientRpc()
    {
        foreach (TrailRenderer t in Trails)
        {
            t.enabled = true;
        }
    }

    private void DisableTrails()
    {
        foreach(TrailRenderer t in Trails) {
            t.Clear();
            t.enabled = false;
        }
    }

    private bool IsGrounded() {
        bool wasGrounded = grounded;

        const float OFFSET = 0.03f;
        float radius = _collider.bounds.extents.x - OFFSET;
        float maxDistance = (_collider.bounds.extents.y / 2) + (OFFSET * 10);
        grounded = Physics.SphereCast(_collider.bounds.center, radius, -transform.up, out RaycastHit hitInfo, maxDistance);
        //Collider[] colliders = Physics.OverlapSphere(_collider.bounds.center - (transform.up * maxDistance), radius);

        if (wasGrounded == grounded) return grounded;

        animator.SetBool("IsGrounded", grounded);
        if (grounded) {
            respawnProjector.spline = hitInfo.collider.GetComponent<SplineComputer>();
            OnGroundedEvent?.Invoke(this, EventArgs.Empty);
        } else {
            OnAirbourneEvent?.Invoke(this, EventArgs.Empty);
        }
        return grounded;
    }

    private void AssignGravity()
    {
        if (grounded) return;

        falling = (fallingTimer <= 0 || falling);

        fallingTimer -= Time.deltaTime;

        animator.SetBool("IsFalling", falling);

        currentGravity = (!falling) ? airbourneGravity
                       : (!diving) ?  fallingGravity
                       : fallingGravity * 3;

    }

    private async void JumpLenience() 
    { 
        await Task.Delay(300);
        canJump = grounded;
    }

    private async void SlideCooldown()
    {
        sliding = true;
        animator.SetBool("IsSliding", true);
        await Task.Delay(slideCooldown);

        animator.speed = 0;

        float horzontalSpeed = GetHorizontalVelocity().magnitude;
        while (horzontalSpeed > maxRunSpeed * sprintSpeedMult && sliding) {
            horzontalSpeed = GetHorizontalVelocity().magnitude;
            await UniTask.Yield();
            continue;
        }

        animator.speed = 1;

        sliding = false;
        animator.SetBool("IsSliding", false);
        if (!crouching) SetColliderToStand();
    }

    

    #endregion
}
