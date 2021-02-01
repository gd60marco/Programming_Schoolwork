using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private float _firePeriod = 0.2f;
    [SerializeField] private Targetable _targetable;
    [SerializeField] private Projectile _projectilePrefab;

    public bool CanFire {get; set;} = true;

    private float _lastFireTime;

    public void TryFire(Vector3 aimPosition)
    {
        // ensure we can fire
        if(!CanFire) return;
        if(Time.time < _lastFireTime + _firePeriod) return;
        _lastFireTime = Time.time;

        // find direction to aimPosition
        Vector3 aimDirection = (aimPosition - transform.position).normalized;

        // turn aimDirection into equivalent rotation
        Quaternion aimRotation = Quaternion.LookRotation(aimDirection);

        // fire projectile
        Projectile instantiated = Instantiate(_projectilePrefab, transform.position, aimRotation);
        instantiated.Team = _targetable.Team;
    }
}