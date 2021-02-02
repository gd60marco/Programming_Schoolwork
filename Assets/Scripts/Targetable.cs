using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targetable : MonoBehaviour
{
    [SerializeField] private int _team = 1;
    [SerializeField] private Transform _centerTransform;
    [SerializeField] private bool _isTargetable = true;

    // expose fields as properties
    public int Team => _team;
    public Vector3 Center => _centerTransform.position;

    public bool isTargetable
    {
        get => _isTargetable;
        set => _isTargetable = value;
    }
}