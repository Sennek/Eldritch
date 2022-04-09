using UnityEngine;
using UnityEngine.Events;

public class CustomEventRelay : MonoBehaviour
{
    [SerializeField] private UnityEvent eventRelay;
    private void Invoke() => eventRelay.Invoke();
}
