using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using EasyUnityInternals;
using System.Collections;

[Serializable]
public class EasyPool<T> : IDisposable, IEnumerable<T> where T : Component
{
    [SerializeField] protected List<T> list;

    /// <summary>
    /// The template of T from which copies will be instantiated
    /// </summary>
    [Obsolete("If necessary, use InstantiateFunctions instead")]
    public T Template { get; set; }

    /// <summary>
    /// A function that returns a newly instantiated element for this pool to use
    /// </summary>
    public Func<T> InstantiateFunction { get; set; }

    /// <summary>
    /// The transform that will be used as the parent of the newly instantiated objects
    /// <para>Default: null</para>
    /// </summary>
    [Obsolete("If necessary, use InstantiateFunctions instead")]
    public Transform Parent { get; set; }

    /// <summary>
    /// If set, limits the maximum amount of objects in the pool.
    /// </summary>
    public int? CountLimit { get; set; }

    /// <summary>
    /// The current count of objects in the pool.
    /// </summary>
    public int Count => list.Count;

    /// <summary>
    /// Indicates whether the pool is currently full. (only applicable if a CountLimit is set)
    /// <para>see also: <seealso cref="CountLimit"/></para>
    /// </summary>
    public bool IsFull => CountLimit.HasValue && list.Count >= CountLimit;

    /// <summary>
    /// The criteria for when a pooled object is available to be reused.
    /// <code>Default: (T t) => !t.gameObject.activeSelf;</code>
    /// </summary>
    public Func<T, bool> UsableCriteria { get; set; } = (T t) => !t.gameObject.activeSelf;

    /// <summary>
    /// The action invoked on objects when they fill their <see cref="ReturnCriteria"/>.
    /// <br>This is intented to be used to return the object to the pool (by turning the condition on Usable Criteria to true).</br>
    /// <code>Default: (T t) => t.gameObject.SetActive(false);</code>
    /// </summary>
    public Action<T> ReturnAction { get; set; } = (T t) => t.gameObject.SetActive(false);

    /// <summary>
    /// If set, when this criteria first evaluates to true, <see cref="ReturnAction"/> is invoked.
    /// <br>This is intented to be used to automatically return the object to the pool.</br>
    /// <code>Default: null</code>
    /// </summary>
    public Func<T, bool> ReturnCriteria { get; set; }

    /// <summary>
    /// Minimum amount of seconds before the ReturnCriteria is enabled
    /// <code>Default: 0</code>
    /// </summary>
    public float ReturnMinDelay { get; set; }

    /// <summary>
    /// Creates a new EasyPool with a template and a parent.
    /// <para>This sets <see cref="InstantiateFunction"/> as Object.Instantiate(Template, Parent)</para>
    /// </summary>
    /// <param name="template">The template of T from which copies will be instantiated</param>
    /// <param name="parent">The transform that will be used as the parent of the newly instantiated objects</param>
    public EasyPool(T template, Transform parent)
    {
        list = new List<T>();
        Template = template;
        Parent = parent;
        InstantiateFunction = () => Object.Instantiate(Template, Parent);
    }

    /// <summary>
    /// Creates a new EasyPool with a specific ready-to-use Instantiate Function
    /// <br>See also: <seealso cref="InstantiateFunction"/></br>
    /// </summary>
    /// <param name="instantiateFunction">A function that returns a newly instantiated element for this pool to use</param>
    public EasyPool(Func<T> instantiateFunction)
    {
        list = new List<T>();
        InstantiateFunction = instantiateFunction;
    }

    /// <summary>
    /// Gets the first object in the pool that satisfies the <see cref="UsableCriteria"/>.
    /// <br>If none is found, this instantiates a new one.</br>
    /// </summary>
    /// <returns></returns>
    public T Get()
    {
        list.RemoveAll(a => !a);
        T t = list.FirstOrDefault(a => UsableCriteria(a));
        if (!t && !IsFull) list.Add(t = InstantiateFunction());
        if (t)
        {
            t.gameObject.SetActive(true);
            if (ReturnCriteria != null) ReturnWhen(t, ReturnMinDelay, ReturnCriteria);
        }
        return t;
    }

    /// <summary>
    /// Invokes the ReturnAction on the object to return it to the pool
    /// </summary>
    /// <param name="t">The object which is being returned to the pool</param>
    public void Return(T t) => ReturnAction(t);

    /// <summary>
    /// Automatically returns an object to the pool when the provided criteria first evaluates to true.
    /// <br>This is automatically set when you get an object and <see cref="ReturnCriteria"/> had already been set.</br>
    /// </summary>
    /// <param name="t">The object which will be returned to the pool.</param>
    /// <param name="criteria">The criteria that evaluates when the object is ready to be returned.</param>
    public void ReturnWhen(T t, Func<T, bool> criteria) => ReturnWhen(t, 0, () => criteria(t));

    /// <summary>
    /// Automatically returns an object to the pool when the provided criteria first evaluates to true.
    /// <br>This is automatically set when you get an object and <see cref="ReturnCriteria"/> had already been set.</br>
    /// </summary>
    /// <param name="t">The object which will be returned to the pool.</param>
    /// <param name="minDelay">Minimum amount of seconds before the criteria is enabled</param>
    /// <param name="criteria">The criteria that evaluates when the object is ready to be returned.</param>
    public void ReturnWhen(T t, float minDelay, Func<T, bool> criteria) => ReturnWhen(t, minDelay, () => criteria(t));

    /// <summary>
    /// Automatically returns an object to the pool when the provided criteria first evaluates to true.
    /// <br>This is automatically set when you get an object and <see cref="ReturnCriteria"/> had already been set.</br>
    /// </summary>
    /// <param name="t">The object which will be returned to the pool.</param>
    /// <param name="criteria">The criteria that evaluates when the object is ready to be returned.</param>
    public void ReturnWhen(T t, Func<bool> criteria) => ReturnWhen(t, 0, criteria);

    /// <summary>
    /// Automatically returns an object to the pool when the provided criteria first evaluates to true.
    /// <br>This is automatically set when you get an object and <see cref="ReturnCriteria"/> had already been set.</br>
    /// </summary>
    /// <param name="t">The object which will be returned to the pool.</param>
    /// <param name="minDelay">Minimum amount of seconds before the criteria is enabled</param>
    /// <param name="criteria">The criteria that evaluates when the object is ready to be returned.</param>
    public void ReturnWhen(T t, float minDelay, Func<bool> criteria) =>
        EasySingleton.GetInstance.InvokeDelayed(minDelay,
        () => EasySingleton.GetInstance.InvokeDelayed(new WaitUntil(criteria), () => Return(t)));

    /// <summary>
    /// Invokes <see cref="Return(T)"/> on all objects instantiated by this pool.
    /// </summary>
    public void ReturnAll()
    {
        foreach (T t in list)
            if (t) Return(t);
    }

    /// <summary>
    /// Invokes <see cref="Return(T)"/> on all objects instantiated by this pool that currently return <see cref="UsableCriteria"/> = false.
    /// </summary>
    public void ReturnAllActive()
    {
        foreach (T t in list)
            if (t && !UsableCriteria(t))
                Return(t);
    }

    /// <summary>
    /// Deletes all instantiated objects and clears the list of any references.
    /// </summary>
    public void Clear()
    {
        foreach (T t in list)
            if (t) Object.Destroy(t.gameObject);
        list.Clear();
    }

    /// <summary>
    /// Invokes <see cref="Clear"/>
    /// </summary>
    public void Dispose() => Clear();

    /// <summary>
    /// Invokes <see cref="Clear"/>
    /// </summary>
    void IDisposable.Dispose() => Clear();

    /// <summary>
    /// Gets all currently active objects (those who return false to <see cref="UsableCriteria"/>).
    /// </summary>
    /// <returns></returns>
    public IEnumerator<T> GetEnumerator() => list.Where(t => !UsableCriteria(t)).GetEnumerator();

    /// <summary>
    /// Gets all currently active objects (those who return false to <see cref="UsableCriteria"/>).
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator() => list.Where(t => !UsableCriteria(t)).GetEnumerator();
}