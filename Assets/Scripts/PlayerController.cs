using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterMovement _characterMovement;
    [SerializeField] private Teleportation _teleportation;
    [SerializeField] private Weapon _weapon;
    [SerializeField] private Vector2 _moveInput;

    private bool _isFiring;

    // callback from PlayerInput component
    public void OnMove(InputValue value)
    {
        // read vector2 value from InputValue
        _moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        _characterMovement.Jump();
    }

    public void OnFire(InputValue value)
    {
        // true when value > 0.5f, false when <= 0.5f
        _isFiring = value.Get<float>() > 0.5f;
    }
    public void OnTeleportStart()
    {
        _teleportation.TryTeleport();
    }
    public void OnTeleportEnd()
    {
        _teleportation.EndTeleport();
    }

    private void Update()
    {
        // find move input based on camera direction
        Vector3 up = Vector3.up;
        Vector3 right = Camera.main.transform.right;
        Vector3 forward = Vector3.Cross(right, up);
        Debug.DrawRay(Camera.main.transform.position, forward * 3f, Color.green);

        Vector3 localMovement = right * _moveInput.x + forward * _moveInput.y;

        _characterMovement.SetMoveInput(localMovement);
        _characterMovement.SetLookDirection(forward);

        if(!_isFiring) return;
        // fire weapon at point 100m in front of camera
        Vector3 aimPosition = Camera.main.transform.position + Camera.main.transform.forward * 100f;
        _weapon.TryFire(aimPosition);
    }
}
