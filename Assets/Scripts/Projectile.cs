using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float _velocity = 20f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private bool _friendlyFire;

    public int Team {get; set;}
    
    private Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.velocity = transform.forward * _velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        // check for Targetable component
        if(other.TryGetComponent(out Targetable target))
        {
            // check team
            if(_friendlyFire || Team != target.Team)
            {
                // damage the target
                other.GetComponent<Health>()?.Damage(_damage);
                Destroy(gameObject);
                return;
            }

            // fallback for hitting ally
            // play ally grunt/angry voiceover
            Destroy(gameObject);
            return;
        }

        // fallback for no Targetable present
        Destroy(gameObject);
    }
}