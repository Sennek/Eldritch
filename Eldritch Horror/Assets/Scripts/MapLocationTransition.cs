using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class MapLocationTransition : MonoBehaviour
{
    public Color[] transitionColors;
    [SerializeField] private MapLocation _mapA;
    [SerializeField] private MapLocation _mapB;
    [SerializeField, OnValueChanged("UpdateColors")] private MapTransitionType _transitionType;
    [SerializeField] public LineRenderer lr;
    public void UpdateObjectName()
    {
        name = $"Transition - {_mapA.locationName} - {_mapB.locationName}";
    }
    public bool LeadsToLocation(MapLocation location)
    {
        return _mapA == location || _mapB == location;
    }
    public void SetMapLocations(MapLocation mapA, MapLocation mapB)
    {
        _mapA = mapA;
        _mapB = mapB;
    }
    public void SyncTransitions(MapLocationTransition transitionToSyncWith)
    {
        _transitionType = transitionToSyncWith._transitionType;
        Vector3[] newPos = new Vector3[transitionToSyncWith.lr.positionCount];
        transitionToSyncWith.lr.GetPositions(newPos);
        lr.positionCount = newPos.Length;
        lr.SetPositions(newPos);
        UpdateColors();
    }
    [Button]
    private void UpdateColors()
    {
        lr.endColor = lr.startColor = transitionColors[(int)_transitionType];
    }
    public void Highlight(bool active) => gameObject.SetActive(active);
}

public enum MapTransitionType
{
    Uncharted,
    Train,
    Ship
}