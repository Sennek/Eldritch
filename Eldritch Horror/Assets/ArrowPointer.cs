using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowPointer : MonoBehaviour
{
    [SerializeField] private LineRenderer _lr;

    public void UpdateArrowPositions(Vector3 targetPos)
    {
        _lr.SetPosition(0, transform.position);
        _lr.SetPosition(1, targetPos);
    }
    public void Show() { gameObject.SetActive(true); }
    public void Hide() { gameObject.SetActive(false); }
}
