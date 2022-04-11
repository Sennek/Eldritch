using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
public abstract class Entity : MonoBehaviour
{
    [SerializeField] MapLocation _currentLocation;

    public void MoveToLocation(MapLocation mapLocation)
    {
        _currentLocation = mapLocation;
    }
}
