using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
public class ClickableObject : MonoBehaviour
{
    [SerializeField] private UnityEvent _onMouseEnter;
    [SerializeField] private UnityEvent _onMouseExit;
    [SerializeField] private UnityEvent _onMouseDown;
    [SerializeField] private UnityEvent _onMouseUp;
    [SerializeField] private UnityEvent _onMouseHold;

    [ReadOnly, SerializeField] private bool _mouseEntered;
    [ReadOnly, SerializeField] private bool _mouseDown;

    [SerializeField] private ArrowPointer _arrowPointer;

    private void Update()
    {
        if (_arrowPointer && MouseCaster.CObjectSelected == this)
        {
            _arrowPointer.UpdateArrowPositions(MouseCaster.MousePos);
        }
    }

    #region Mouse Events
    public void MouseEntered()
    {
        if (!_mouseEntered)
        {
            _onMouseEnter?.Invoke();
            _mouseEntered = true;
        }
    }
    public void MouseExited()
    {
        if (_mouseEntered)
        {
            _onMouseExit?.Invoke();
            _mouseEntered = false;
        }
    }
    public void MouseHold()
    {
        _onMouseHold?.Invoke();
        MouseDown();
    }
    public void MouseDown()
    {
        if (!_mouseDown)
        {
            _onMouseDown?.Invoke();
            _mouseDown = true;
        }
    }
    public void MouseUp()
    {
        _onMouseUp?.Invoke();
        _mouseDown = false;
    }
    #endregion
}