using System;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventRelay : MonoBehaviour
{
    [Serializable] public class AwakeRelay : UnityEvent { }
    [SerializeField] private AwakeRelay awakeRelay;
    private void Awake() => awakeRelay?.Invoke();

    [Serializable] public class OnEnableRelay : UnityEvent { }
    [SerializeField] private OnEnableRelay onEnableRelay;
    private void OnEnable() => onEnableRelay?.Invoke();

    [Serializable] public class StartRelay : UnityEvent { }
    [SerializeField] private StartRelay startRelay;
    private void Start() => startRelay?.Invoke();

    [Serializable] public class OnDisableRelay : UnityEvent { }
    [SerializeField] private OnDisableRelay onDisableRelay;
    private void OnDisable() => onDisableRelay?.Invoke();

    [Serializable] public class OnDestroyRelay : UnityEvent { }
    [SerializeField] private OnDestroyRelay onDestroyRelay;
    private void OnDestroy() => onDestroyRelay?.Invoke();



    [Serializable] public class OnTriggerEnterRelay : UnityEvent { }
    [SerializeField] private OnTriggerEnterRelay onTriggerEnterRelay;
    private void OnTriggerEnter(Collider other) => onTriggerEnterRelay?.Invoke();

    [Serializable] public class OnTriggerExitRelay : UnityEvent { }
    [SerializeField] private OnTriggerExitRelay onTriggerExitRelay;
    private void OnTriggerExit(Collider other) => onTriggerExitRelay?.Invoke();

    [Serializable] public class OnTriggerEnterRelay2D : UnityEvent { }
    [SerializeField] private OnTriggerEnterRelay2D onTriggerEnterRelay2D;
    private void OnTriggerEnter2D(Collider2D collision) => onTriggerEnterRelay2D?.Invoke();

    [Serializable] public class OnTriggerExitRelay2D : UnityEvent { }
    [SerializeField] private OnTriggerExitRelay2D onTriggerExitRelay2D;
    private void OnTriggerExit2D(Collider2D collision) => onTriggerExitRelay2D?.Invoke();



    [Serializable] public class OnCollisionEnterRelay : UnityEvent { }
    [SerializeField] private OnCollisionEnterRelay onCollisionEnterRelay;
    private void OnCollisionEnter(Collision collision) => onCollisionEnterRelay?.Invoke();

    [Serializable] public class OnCollisionExitRelay : UnityEvent { }
    [SerializeField] private OnCollisionExitRelay onCollisionExitRelay;
    private void OnCollisionExit(Collision collision) => onCollisionExitRelay?.Invoke();

    [Serializable] public class OnCollisionEnterRelay2D : UnityEvent { }
    [SerializeField] private OnCollisionEnterRelay2D onCollisionEnterRelay2D;
    private void OnCollisionEnter2D(Collision2D collision) => onCollisionEnterRelay2D?.Invoke();

    [Serializable] public class OnCollisionExitRelay2D : UnityEvent { }
    [SerializeField] private OnCollisionExitRelay2D onCollisionExitRelay2D;
    private void OnCollisionExit2D(Collision2D collision) => onCollisionExitRelay2D?.Invoke();
}
