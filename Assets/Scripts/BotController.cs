using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterMovement _characterMovement;
    [SerializeField] private Transform _characterHead;

    [Header("Tuning")]
    [SerializeField] private float _attackDistance = 5f;
    [SerializeField] private float _visionHalfAngle = 60f;
    [SerializeField] private LayerMask _occlusionMask;

    private GameObject _player;

    //UNITY METHODS _______________________________________________
    private void Start()
    {
        _player = GameObject.FindWithTag("Player");
        _characterMovement = GetComponent<CharacterMovement>();
    }

    private void Update()
    {
        if(_player != null)
        {
            float distance = Vector3.Distance(transform.position, _player.transform.position);
            if (distance > _attackDistance || !CanSeePoint(_player.transform.position))
            {
                _characterMovement.MoveTo(_player.transform.position);
                Debug.DrawLine(_characterHead.position, _player.transform.position, Color.red);
            }
            else
            {
                _characterMovement.Stop();
                Debug.DrawLine(_characterHead.position, _player.transform.position, Color.green);
            }
        }
    }
    //CUSTOOM METHODS ____________________________________________________
    private bool CanSeePoint(Vector3 point)
    {
        //Find direction to point
        Vector3 dirToPoint = (point - _characterHead.position).normalized;

        //Find angle to point (fails in angle > _visionHalfAngle)
        float angle = Vector3.Angle(dirToPoint, _characterHead.forward);
        if (angle > _visionHalfAngle) return false;

        float distance = Vector3.Distance(point, _characterHead.position);

        //check fails in raycast hits object on occlusion mask layer;
        return !Physics.Raycast(_characterHead.position, dirToPoint, distance, _occlusionMask);
    }
}
