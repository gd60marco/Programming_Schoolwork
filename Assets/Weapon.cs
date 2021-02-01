using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private float _firePeriod = 0.2f;
    [SerializeField] private Targetable _targetable;
    [SerializeField] private Projectile _projectilePrefab;

    public bool CanFire { get; set; }

    private float _lastFireTime;

    public void TryFire(Vector3 aimPosition)
    {
        // ensure can dire
        if (!CanFire) return;
        if (Time.time < _lastFireTime + _firePeriod) return;

        //find direction to aim position
        Vector3 aimDirection = (aimPosition - transform.position).normalized;

        //turn aimDirection into equivalent rotation
        Quaternion aimRotation = Quaternion.LookRotation(aimDirection, Vector3.up);

        //fire projectile
        Projectile instantiated = Instantiate(_projectilePrefab, transform.position, aimRotation);
        instantiated.Team = _targetable.Team;
    }


}
