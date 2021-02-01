using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targetable : MonoBehaviour
{
    [SerializeField] private int _team = 1;
    [SerializeField] private Transform _centerTransform;

    // expose fields as properties
    public int Team => _team;
    public Vector3 Center => _centerTransform.position;
}