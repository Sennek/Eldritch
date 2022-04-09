using UnityEngine;

public class FollowObject : MonoBehaviour
{
    [Tooltip("The target object to be followed")]
    [SerializeField] protected GameObject target;
    [Tooltip("An offset from the target in world position")]
    [SerializeField] protected Vector3 offset;
    [Tooltip("The maximum speed this object can go (0 disables this)")]
    [SerializeField] protected float speed;
    [Tooltip("When closer than this setting from our target position, we ignore speed and just set our position directly")]
    [SerializeField] protected float distanceMargin;

    /// <summary>
    /// The target object to be followed
    /// </summary>
    public GameObject Target { get => target; set => target = value; }
    /// <summary>
    /// An offset from the target in world position
    /// </summary>
    public Vector3 Offset { get => offset; set => offset = value; }
    /// <summary>
    /// The maximum speed this object can go (0 disables this)
    /// </summary>
    public float Speed { get => speed; set => speed = value; }
    /// <summary>
    /// When closer than this setting from our target position, we ignore speed and just set our position directly
    /// </summary>
    public float DistanceMargin { get => distanceMargin; set => distanceMargin = value; }

    /// <summary>
    /// Our calculated target world position considering our target object and offset settings
    /// </summary>
    public Vector3 TargetOffsetPosition => target.transform.position + offset;

    protected virtual void FixedUpdate()
    {
        if (target)
        {
            if (speed > 0 && Vector3.Distance(TargetOffsetPosition, transform.position) > distanceMargin)
                transform.position += (TargetOffsetPosition - transform.position).normalized * speed * Time.fixedDeltaTime;
            else
                transform.position = target.transform.position + offset;
        }
    }
}