using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleportation : MonoBehaviour
{
    [Header("Inspector References")]
    [SerializeField] private GameObject _teleportMarker;

    [Header("Attributes")]
    [SerializeField] private float _teleportDistance;
    [SerializeField] private float _teleportMaxHeight;

    [Header("Raycast")]
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float _raycastRadius;
    [SerializeField] private float _raycastVerticalLenght;

    private Vector3 teleportCheckPosition;
    private GameObject _marker;

    public void TryTeleport()
    {
        //Instantate marker, called by player controller
        _marker = Instantiate(_teleportMarker, teleportCheckPosition, Quaternion.identity);
    }

    private void Update()
    {
        if (_marker != null)
        {
            MoveMarker();
        }
    }

    public void EndTeleport()
    {
        this.transform.position = _marker.transform.position;
        Object.Destroy(_marker);
    }
    private void MoveMarker()
    {
        //Get the new position for maker based on look direciton
        teleportCheckPosition = (transform.forward * _teleportDistance) + transform.position;
        teleportCheckPosition.y += _teleportMaxHeight;

        RaycastHit hitInfo;
        if (Physics.SphereCast(teleportCheckPosition, _raycastRadius, Vector3.down, out hitInfo, _raycastVerticalLenght, _layerMask))
        {
            _marker.SetActive(true);
            teleportCheckPosition = new Vector3(teleportCheckPosition.x, hitInfo.point.y, teleportCheckPosition.z);
        }
        else //Fallback
        {
            _marker.SetActive(false);
            teleportCheckPosition = this.transform.position;
        }

        _marker.transform.position = teleportCheckPosition;
    }
}
