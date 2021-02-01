using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterMovement _characterMovement;
    [SerializeField] private Transform _characterHead;
    [SerializeField] private Targetable _targetable;

    [Header("Tuning")]
    [SerializeField] private float _attackDistance = 5f;
    [SerializeField] private float _visionHalfAngle = 60f;
    [SerializeField] private float _visionDistance = 10f;
    [SerializeField] private LayerMask _occlusionMask;
    [SerializeField] private LayerMask _targetingMask;

    private Targetable _target;

    private void Update()
    {
        // find a target if target is null
        if(_target == null)
        {
            // find all colliders within vision radius that are on targetingMask layer
            Collider[] hits = Physics.OverlapSphere(_characterHead.position, _visionDistance, _targetingMask);

            // find appropriate target in hits array
            foreach (Collider hit in hits)
            {
                // check for targetable component on collider, and compare against team and visibility
                if (hit.TryGetComponent(out Targetable possibleTarget) && 
                    possibleTarget.Team != _targetable.Team && 
                    CanSeePoint(possibleTarget.Center))
                {
                    // assign target and break out of foreach loop
                    _target = possibleTarget;
                    break;
                }
            }
        }

        // move to player
        if(_target != null)
        {
            // move if further than attack distance
            float distance = Vector3.Distance(transform.position, _target.transform.position);
            if(distance > _attackDistance || !CanSeePoint(_target.Center))
            {
                _characterMovement.MoveTo(_target.transform.position);
                Debug.DrawLine(_characterHead.position, _target.Center, Color.red);
            }
            else
            {
                _characterMovement.Stop();
                Debug.DrawLine(_characterHead.position, _target.Center, Color.green);
            }
        }
    }

    private bool CanSeePoint(Vector3 point)
    {
        // find direction to point
        Vector3 dirToPoint = (point - _characterHead.position).normalized;

        // find angle to point, check fails if angle > _visionHalfAngle
        float angle = Vector3.Angle(dirToPoint, _characterHead.forward);
        if(angle > _visionHalfAngle) return false;

        // find distance to point
        float distance = Vector3.Distance(_characterHead.position, point);
        // check fails if raycast hits object on _occlusionMask layer
        return !Physics.Raycast(_characterHead.position, dirToPoint, distance, _occlusionMask);
    }

    private void OnDrawGizmosSelected()
    {
        // debug vision radius
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(_characterHead.position, _visionDistance);
    }
}