using System;
using System.Reflection;
using System.Linq;
using System.ComponentModel;

public static class NoxConsoleReflection
{
    private static readonly Type[] types;

    [NoxCommand(Alias = "get", Description = "gets the value of a field or property")]
    public static void GetValue(string text)
    {
        try
        {
            string[] path = text.Split('.');

            Type t = types.First(a => a.Name.Equals(path[0]));
            object data = GetValue(t, path[1]);

            for (int i = 2; i < path.Length; i++)
            {
                string name = SeparateIndex(path[i], out int index);
                data = GetValue(data, name);
                if (index >= 0)
                {
                    //TODO: Solve reflection using indexes and/or array and/or iEnumerables 
                }
            }

           NoxConsole.Write($"get {text} = {data?.ToString()}");
        }
        catch (Exception e)
        {
            Log.Exception(e);
            NoxConsole.Write(e.Message);
        }
    }

    [NoxCommand(Alias = "set", Description = "sets the value of a field or property")]
    public static void SetValue(string text, string value)
    {
        string[] path = text.Split('.');
        Type type = types.First(a => a.Name.Equals(path[0]));

        FieldInfo field = type.GetField(path[1]);
        PropertyInfo property = type.GetProperty(path[1]);

        if (field != null) field.SetValue(null, TypeDescriptor.GetConverter(field.FieldType).ConvertFromString(value));
        else if (property != null) property.SetValue(null, TypeDescriptor.GetConverter(field.FieldType).ConvertFromString(value), null);

        if (field != null) NoxConsole.Write("set " + text + " = " + field.GetValue(null).ToString());
        else if (property != null) NoxConsole.Write("set " + text + " = " + property.GetValue(null, null).ToString());
    }

    private static FieldInfo GetField(Type t, string name) => t?.GetField(name);
    private static object GetFieldValue(Type t, string name) => GetFieldValue(GetField(t, name));
    private static object GetFieldValue(Type t, string name, object data) => GetFieldValue(GetField(t, name), data);
    private static object GetFieldValue(FieldInfo f) => f?.GetValue(null);
    private static object GetFieldValue(FieldInfo f, object data) => f?.GetValue(data);

    private static PropertyInfo GetProperty(Type t, string name) => t?.GetProperty(name);
    private static object GetPropertyValue(Type t, string name) => GetPropertyValue(GetProperty(t, name));
    private static object GetPropertyValue(Type t, string name, object[] index) => GetPropertyValue(GetProperty(t, name), index);
    private static object GetPropertyValue(Type t, string name, object data) => GetPropertyValue(GetProperty(t, name), data);
    private static object GetPropertyValue(Type t, string name, object data, object[] index) => GetPropertyValue(GetProperty(t, name), data, index);
    private static object GetPropertyValue(PropertyInfo p) => p?.GetValue(null, null);
    private static object GetPropertyValue(PropertyInfo p, object[] index) => p?.GetValue(null, index);
    private static object GetPropertyValue(PropertyInfo p, object data) => p?.GetValue(data);
    private static object GetPropertyValue(PropertyInfo p, object data, object[] index) => p?.GetValue(data, index);

    private static object GetValue(object data, string name) => GetValue(data?.GetType(), name, data);
    private static object GetValue(Type t, string name) => GetFieldValue(t, name) ?? GetPropertyValue(t, name);
    private static object GetValue(Type t, string name, object data) => GetFieldValue(t, name, data) ?? GetPropertyValue(t, name, data);

    private static bool HasIndex(string name) => name.Contains('[');
    private static string SeparateIndex(string name, out int index)
    {
        int startIndex = name.IndexOf('[') + 1;
        if (startIndex > 0)
        {
            int endIndex = name.IndexOf(']');
            string sub = name.Substring(startIndex, endIndex - startIndex);
            int.TryParse(sub, out index);
            return name.Remove(startIndex - 1);
        }
        index = -1;
        return name;
    }

    private static BindingFlags BindingFlags => BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
}
