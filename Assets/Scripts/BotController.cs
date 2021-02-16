using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterMovement _characterMovement;
    [SerializeField] private Transform _characterHead;
    [SerializeField] private Targetable _targetable;
    [SerializeField] private Weapon _weapon;
    [SerializeField] private Health _health;

    [Header("Tuning")]
    [SerializeField] private float _attackDistance = 5f;
    [SerializeField] private float _visionHalfAngle = 60f;
    [SerializeField] private float _visionDistance = 10f;
    [SerializeField] private LayerMask _occlusionMask;
    [SerializeField] private LayerMask _targetingMask;

    [Header("States")]
    [SerializeField] private string _patrolPointTag = "PatrolPoint";
    [SerializeField] private float _patrolPointReachedDistance = 1f;
    [SerializeField] private float _fleePointReachedDistance = 1f;
    [SerializeField] private float _healthFleePercentage = 0.5f;
    [SerializeField] private float _amountHealedAfterFlee = 50f;
    [SerializeField] private float _fleeSpeedMultiplier = 2f;
 
    private Targetable _target;
    private float TargetDistance => Vector3.Distance(_target.Center, _targetable.Center);
    public bool IsTargetValid => _target != null && _target.isTargetable;

    private IEnumerator _currentState; //Reference to current coroutine executing
    private GameObject[] _patrolPoints; //Moved patrolPoints here since more than one function references it
    private float _originalSpeed;


    //AI STATE MACHINE _______________________________________________________________________
    private void NextState(IEnumerator nextState)
    {
        //stop current state
        if (_currentState != null) StopCoroutine(_currentState);

        //start next state
        _currentState = nextState;
        StartCoroutine(_currentState);
    }
    private IEnumerator FleeState()
    {
        Debug.Log("Entered flee state!");
        //Make an array of patrol points for the sake of avoiding randomness
        GameObject patrolPoint = _patrolPoints[Random.Range(0, _patrolPoints.Length)];
        _characterMovement.SetSpeed(_originalSpeed * _fleeSpeedMultiplier); //Flee speed! Run like the wind!

        while (true)
        {
            //Check distance to flee point
            float distance = Vector3.Distance(patrolPoint.transform.position, transform.position);
            if (distance < _fleePointReachedDistance)
            {
                if(TargetsFound() == true) //If can still see player, set next point and continue fleeing
                {
                    patrolPoint = _patrolPoints[Random.Range(0, _patrolPoints.Length)];
                }
                else //If can't see player, heal and switch to patrol state
                {
                    _health.Heal(_amountHealedAfterFlee);
                    _characterMovement.SetSpeed(_originalSpeed); //Back to regular speed
                    NextState(PatrolState());
                }
            }

            //move to current patrol point
            _characterMovement.MoveTo(patrolPoint.transform.position);

            yield return null;
        }
    }
    private IEnumerator PatrolState()
    {
        GameObject patrolPoint = _patrolPoints[Random.Range(0, _patrolPoints.Length)];

        //loop forever in patrol state
        while(true)
        {
            //Check distance to patrolPoint
            float distance = Vector3.Distance(patrolPoint.transform.position, transform.position);
            if (distance < _patrolPointReachedDistance)
            {
                patrolPoint = _patrolPoints[Random.Range(0, _patrolPoints.Length)];
            }

            //Move to patrol point
            _characterMovement.MoveTo(patrolPoint.transform.position);

            //will exit patrol state if target is found
            TryFindTarget();

            // wait for next frame (yield return null waits for next time Update is executed)
            yield return null;
        }
    }
    private IEnumerator ChaseState()
    {
        while (IsTargetValid)
        {
            // move if further than attack distance
            if (TargetDistance > _attackDistance || !CanSeePoint(_target.Center))
            {
                _characterMovement.MoveTo(_target.transform.position);
                Debug.DrawLine(_characterHead.position, _target.Center, Color.yellow);
            }
            else
            {
                //Flee if wounded!
                if (_health.Percentage < _healthFleePercentage) NextState(FleeState());
                //In range and visible (and not wounded)!!
                NextState(AttackState());
            }

            yield return null;
        }

        //Target becomes invalid
        NextState(PatrolState());
    }

    private IEnumerator AttackState()
    {
        while(IsTargetValid)
        {
            //Ensure target is within range and visible 
            Vector3 vectorToTarget = _target.Center - _targetable.Center;
            Vector3 directionToTarget = vectorToTarget.normalized;
            if(CanSeePoint(_target.Center) && TargetDistance < _attackDistance)
            {
                Debug.DrawLine(_targetable.Center, _target.Center, Color.red);

                //Stop and shoot!
                _characterMovement.Stop();
                _characterMovement.SetLookDirection(directionToTarget);
                _weapon.TryFire(_target.Center);

                //Flee if wounded!
                if (_health.Percentage < _healthFleePercentage) NextState(FleeState());
            }
            else
            {
                //enter chase state if target is out of range or not visible
                NextState(ChaseState());
            }
            yield return null;
        }
        //If target is dead, null...
        NextState(PatrolState());
    }

    //UNITY FUNCTIONS ___________________________________________________________________________________
    private void Start()
    {
        _patrolPoints = GameObject.FindGameObjectsWithTag(_patrolPointTag); //Moved setting array here since more than one function references it
        NextState(PatrolState());
        _weapon = GetComponentInChildren<Weapon>();
        _originalSpeed = _characterMovement.Speed;
}

    //Custom Methods ___________________________________________________________________________

    private bool TargetsFound()
    {
        // find all colliders within vision radius that are on targetingMask layer
        Collider[] hits = Physics.OverlapSphere(_characterHead.position, _visionDistance, _targetingMask);

        // find appropriate target in hits array
        foreach (Collider hit in hits)
        {
            // check for targetable component on collider, and compare against team and visibility
            if (hit.TryGetComponent(out Targetable possibleTarget) &&
            possibleTarget.Team != _targetable.Team &&
            possibleTarget.isTargetable &&
            CanSeePoint(possibleTarget.Center))
            {
            // assign target and break out of foreach loop
            _target = possibleTarget;
                return true;
            }
        }
        return false;
    }

    private void TryFindTarget()
    {
        if(TargetsFound() == true)
          NextState(ChaseState());
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