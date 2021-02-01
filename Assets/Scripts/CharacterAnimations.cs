using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimations : MonoBehaviour
{
    [SerializeField] private CharacterMovement _characterMovement;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _dampTime = 0.1f;

    private void Update()
    {
        // convert world MoveInput to local direction
        Vector3 moveDirection = transform.InverseTransformDirection(_characterMovement.MoveInput);

        // set animator values
        _animator.SetFloat("Forward", moveDirection.z, _dampTime, Time.deltaTime);
        _animator.SetFloat("Strafe", moveDirection.x, _dampTime, Time.deltaTime);
    }
}
