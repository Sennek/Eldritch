using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
public class MapLocation : MonoBehaviour
{
    public string locationName;
    [SerializeField, OnValueChanged("LinkBothWays")] private List<MapLocation> _linkedLocations;
    [SerializeField, OnValueChanged("LinkBothWays")] private List<MapLocationTransition> _transitions;

    public MapLocationTransition GetTransitionTo(MapLocation location)
    {
        return _transitions.FirstOrDefault(x => x.LeadsToLocation(location));
    }

    [Button]
    public void SetHighlight(bool active) => GetComponent<SpriteRenderer>().enabled = active;

    public void HighlightPathToLocation(MapLocation location)
    {

    }
    #region System
    [Button]
    private void UpdateObjectName()
    {
        name = $"Location - {locationName}";
    }
    [Button]
    private void UpdateLinkedTransitions()
    {
        foreach (MapLocation location in _linkedLocations)
        {
            location.GetTransitionTo(this).SyncTransitions(GetTransitionTo(location));
        }
    }
    [Button]
    private void GenerateTransitions(GameObject transitionPrefab)
    {
        foreach (MapLocation location in _linkedLocations)
        {
            GameObject transition = Instantiate(transitionPrefab, transform);
            transform.position = transform.position;

            MapLocationTransition transitionScript = transition.GetComponent<MapLocationTransition>();
            LineRenderer transitionLR = transition.GetComponent<LineRenderer>();

            transitionScript.SetMapLocations(this, location);
            transitionScript.UpdateObjectName();

            transitionLR.SetPosition(0, transform.position);
            transitionLR.SetPosition(1, location.transform.position);

            _transitions.Add(transitionScript);
        }
    }
    private void LinkBothWays()
    {
        foreach (MapLocation location in _linkedLocations)
        {
            if (!location._linkedLocations.Contains(this))
                location._linkedLocations.Add(this);
        }
    }
    private void OnDrawGizmosSelected()
    {
        foreach (var transition in _transitions)
        {
            if (transition.lr && transition.lr.positionCount > 1)
                for (int i = 1; i < transition.lr.positionCount; i++)
                    Gizmos.DrawLine(transition.lr.GetPosition(i - 1), transition.lr.GetPosition(i));
        }
    }
    #endregion
}
