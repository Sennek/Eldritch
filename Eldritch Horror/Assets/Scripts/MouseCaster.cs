using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCaster : SingletonMB<MouseCaster>
{
    [SerializeField] private float _radius = 0.1f;
    [SerializeField] private ClickableObject _cObjectSelected;
    [SerializeField] private ClickableObject _cObjectInRange;

    public static Vector3 MousePos => Camera.main.ScreenToWorldPoint(Input.mousePosition);

    public static ClickableObject CObjectInRange
    {
        get => Instance._cObjectInRange;
        set
        {
            if (Instance._cObjectInRange && value != Instance._cObjectInRange)
                Instance._cObjectInRange.MouseExited();

            Instance._cObjectInRange = value;
            Instance._cObjectInRange.MouseEntered();
        }
    }

    public static ClickableObject CObjectSelected
    {
        get => Instance._cObjectSelected;
        set => Instance._cObjectSelected = value;
    }

    void Update()
    {
        RayTrace();
        FollowMouse();
        ClickEvents();
    }
    private void FollowMouse()
    {
        transform.position = new Vector3(MousePos.x, MousePos.y);
    }
    private void ClickEvents()
    {
        if (CObjectInRange)
        {
            if (Input.GetMouseButtonDown(0))
                CObjectInRange.MouseHold();
            else
                CObjectInRange.MouseUp();
        }
    }
    private void RayTrace()
    {
        Collider2D overlapped = Physics2D.OverlapCircle(transform.position, _radius);

        if (overlapped && TryGetComponent(out ClickableObject cObject))
            CObjectInRange = cObject;
        else
            CObjectInRange = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
