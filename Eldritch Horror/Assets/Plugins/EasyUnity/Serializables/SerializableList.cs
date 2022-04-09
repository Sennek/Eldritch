using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableList<T> : List<T>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<T> items = new List<T>();

    public void OnBeforeSerialize()
    {
        items.Clear();
        items.AddRange(this);
    }

    public void OnAfterDeserialize()
    {
        Clear();
        AddRange(items);
    }
}
