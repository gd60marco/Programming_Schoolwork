using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private NavMeshAgent _navMeshAgent;

    [Header("Movement")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _turnSpeed = 10f;

    [Header("Jumping")]
    [SerializeField] private float _gravity = 20f;
    [SerializeField] private float _jumpHeight = 1f;
    [SerializeField] private float _airControl = 0.1f;

    [Header("Grounding")]
    [SerializeField] private float _groundCheckRadius = 0.25f;
    [SerializeField] private Vector3 _groundCheckStart = new Vector3(0f, 0.35f, 0f);
    [SerializeField] private Vector3 _groundCheckEnd = new Vector3(0f, 0.1f, 0f);
    [SerializeField] private LayerMask _groundMask;

    // this is a property, similar to field (normal variable), but we have made it publicly readable, but only settable privately
    public Vector3 MoveInput {get; private set;}
    public Vector3 LookDirection {get; private set;}
    public bool IsGrounded {get; private set;}
    public Vector3 GroundNormal {get; private set;}
    public bool CanMove { get; set; } = true;

    private void Start()
    {
        // disable NavMeshAgent of the character
        _navMeshAgent.updatePosition = false;
        _navMeshAgent.updateRotation = false;
        LookDirection = transform.forward;
    }

    public void MoveTo(Vector3 position)
    {
        // tell NavMeshAgent to move to position
        _navMeshAgent.SetDestination(position);
    }

    public void Stop()
    {
        // clear NavMeshAgent path
        _navMeshAgent.ResetPath();
        // zero movement input
        SetMoveInput(Vector3.zero);
    }

    public void SetMoveInput(Vector3 input)
    {
        // flatten input
        input.y = 0f;
        // clamp and set input
        MoveInput = Vector3.ClampMagnitude(input, 1f);
    }

    public void SetLookDirection(Vector3 direction)
    {
        direction.y = 0f;
        LookDirection = direction.normalized;
    }

    public void Jump()
    {
        // stop if cant move
        if (!CanMove) return;
        // stop if not grounded
        if(!IsGrounded) return;

        // calculate velocity required to reach jump height
        float jumpVelocity = Mathf.Sqrt(2f * _gravity * _jumpHeight);
        // override vertical velocity with jumpVelocity
        Vector3 velocity = _rigidbody.velocity;
        velocity.y = jumpVelocity;
        _rigidbody.velocity = velocity;
    }

    private void Update()
    {
        // use NavMeshAgent's path to determine movement direction for rigidbody
        if(_navMeshAgent.hasPath)
        {
            // get next point on path
            Vector3 nextPathPoint = _navMeshAgent.path.corners[1];
            // get direction to next point
            Vector3 dirToPoint = (nextPathPoint - transform.position).normalized;
            // set MoveInput and LookDirection
            SetMoveInput(dirToPoint);
            SetLookDirection(dirToPoint);
        }

        // move NavMeshAgent to character position
        _navMeshAgent.nextPosition = transform.position;

    }

    private void FixedUpdate()
    {
        IsGrounded = CheckGrounded();

        //zero all input in cant move
        if (!CanMove)
        {
            SetMoveInput(Vector3.zero);
            SetLookDirection(transform.forward);
        }

        // calculate target velocity and difference from current
        Vector3 targetVelocity = MoveInput * _speed;
        Vector3 velocityDiff = targetVelocity - _rigidbody.velocity;
        velocityDiff.y = 0f;

        // get acceleration towards target velocity
        float control = IsGrounded ? 1f : _airControl;
        Vector3 acceleration = velocityDiff * _acceleration * control;
        acceleration -= GroundNormal * _gravity;
        _rigidbody.AddForce(acceleration);

        // rotate the character
        Quaternion targetRotation = Quaternion.LookRotation(LookDirection);     // creates a (quaternion) rotation based on an input direction
        Quaternion rotation = Quaternion.Slerp(transform.rotation, targetRotation, _turnSpeed * Time.fixedDeltaTime);
        _rigidbody.MoveRotation(rotation);
    }

    private bool CheckGrounded()
    {
        // get world space positions from local offset
        Vector3 start = transform.TransformPoint(_groundCheckStart);
        Vector3 end = transform.TransformPoint(_groundCheckEnd);
        float distance = Vector3.Distance(start, end);

        // perform a spherecast!
        if(Physics.SphereCast(start, _groundCheckRadius, -transform.up, out RaycastHit hit, distance, _groundMask))
        {
            // we hit the ground
            GroundNormal = hit.normal;
            return true;
        }

        // we DID NOT hit the ground
        GroundNormal = Vector3.up;
        return false;
    }

    // this function ONLY executes when the GameObject is selected in the Hierarchy
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // get world space positions from local offset
        Vector3 start = transform.TransformPoint(_groundCheckStart);
        Vector3 end = transform.TransformPoint(_groundCheckEnd);
        // draw spheres
        Gizmos.DrawWireSphere(start, _groundCheckRadius);
        Gizmos.DrawWireSphere(end, _groundCheckRadius);
    }
}
