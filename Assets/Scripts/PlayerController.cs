using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //COMPONENT REFERENCES _____________________________________________________________
    [SerializeField] private CharacterMovement _characterMovement;
    //CLASS PROPERTIES _________________________________________________________________
    [SerializeField] private Vector2 _moveInput;

    //UNITY METHODS ____________________________________________________________________
    private void Update()
    {
        Vector3 forward = GetForwardCross();
        Debug.DrawRay(Camera.main.transform.position, forward * 3f, Color.green);
        Vector3 localMove = Camera.main.transform.right * _moveInput.x + forward * _moveInput.y;

        _characterMovement.SetMoveInput(localMove);
        _characterMovement.SetLookDiregtion(forward);
    }

    //CONTROL MESSAGES _________________________________________________________________
    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }
    public void OnJump()
    {
        _characterMovement.Jump();
    }

    //CUSTOM METHODS ____________________________________________________________________

    private Vector3 GetForwardCross()
    {
        Vector3 up = Vector3.up;
        Vector3 right = Camera.main.transform.right;
        return Vector3.Cross(right, up);

    }
}
