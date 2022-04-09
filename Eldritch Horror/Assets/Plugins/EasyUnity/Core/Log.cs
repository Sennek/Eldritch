using NoxLibrary;

using System;
using System.Text;

using UnityEngine;

public static class Log
{
    /// <summary>
    /// Multiple debug levels so you can filter what you want to log.
    /// </summary>
    public enum EDebugLevel { None, Exception, Error, Warning, Message, QuickDebug, DeepDebug, RawData };

    /// <summary>
    /// The deepest level you want printed to Unity's Console.
    /// </summary>
    public static EDebugLevel ConsoleDebugLevel { get; set; } = EDebugLevel.QuickDebug;

    /// <summary>
    /// The deepest level you want written to the file.
    /// </summary>
    public static EDebugLevel FileDebugLevel { get; set; } = EDebugLevel.RawData;

    /// <summary>
    /// The deepest level you want the StackTrace written to the file.
    /// </summary>
    public static EDebugLevel StackTraceFileLevel { get; set; } = EDebugLevel.Error;

    public static bool Initialize(EDebugLevel consoleDebugLevel, EDebugLevel fileDebugLevel, EDebugLevel stackTraceFileLevel, string filePath, TimeSpan flushInterval, bool handleUnityLogs)
    {
        if (LogManager.IsInitialized) return false;

        ConsoleDebugLevel = consoleDebugLevel;
        FileDebugLevel = fileDebugLevel;
        StackTraceFileLevel = stackTraceFileLevel;

        if (flushInterval.Ticks > 0)
            LogManager.Initialize(filePath, flushInterval);
        else LogManager.Initialize(filePath, autoFlush: true);

        if (handleUnityLogs)
            Application.logMessageReceived += HandleUnityLog;

        return true;
    }

    private static void WriteLine(EDebugLevel level, object message)
    {
        if (FileDebugLevel >= level && LogManager.IsInitialized)
            LogManager.WriteLine($"[{DateTime.Now:HH:mm:ss:fff}] {level}: {message}", false, true);
    }

    public static void None(object message)
    {
        WriteLine(EDebugLevel.None, message.ToString());
        if (ConsoleDebugLevel >= EDebugLevel.None)
            Debug.Log(message);
    }
    public static void Exception(Exception e)
    {
        WriteLine(EDebugLevel.Exception, e.ToString());
        if (ConsoleDebugLevel >= EDebugLevel.Exception)
            Debug.LogException(e);
    }
    public static void Exception(Exception e, UnityEngine.Object context)
    {
        WriteLine(EDebugLevel.Exception, e.ToString());
        if (ConsoleDebugLevel >= EDebugLevel.Exception)
            Debug.LogException(e, context);
    }
    public static void Error(object message)
    {
        WriteLine(EDebugLevel.Error, message.ToString());
        if (ConsoleDebugLevel >= EDebugLevel.Error)
            Debug.LogError(message);
    }
    public static void Error(object message, UnityEngine.Object context)
    {
        WriteLine(EDebugLevel.Error, message.ToString());
        if (ConsoleDebugLevel >= EDebugLevel.Error)
            Debug.LogError(message, context);
    }
    public static void Warning(object message)
    {
        WriteLine(EDebugLevel.Warning, message.ToString());
        if (ConsoleDebugLevel >= EDebugLevel.Warning)
            Debug.LogWarning(message);
    }
    public static void Warning(object message, UnityEngine.Object context)
    {
        WriteLine(EDebugLevel.Warning, message.ToString());
        if (ConsoleDebugLevel >= EDebugLevel.Warning)
            Debug.LogWarning(message, context);
    }
    public static void Message(object message)
    {
        WriteLine(EDebugLevel.Message, message.ToString());
        if (ConsoleDebugLevel >= EDebugLevel.Message)
            Debug.Log(message);
    }
    public static void Message(object message, UnityEngine.Object context)
    {
        WriteLine(EDebugLevel.Message, message.ToString());
        if (ConsoleDebugLevel >= EDebugLevel.Message)
            Debug.Log(message, context);
    }
    public static void QuickDebug(object message)
    {
        WriteLine(EDebugLevel.QuickDebug, message.ToString());
        if (ConsoleDebugLevel >= EDebugLevel.QuickDebug)
            Debug.Log(message);
    }
    public static void QuickDebug(object message, UnityEngine.Object context)
    {
        WriteLine(EDebugLevel.QuickDebug, message.ToString());
        if (ConsoleDebugLevel >= EDebugLevel.QuickDebug)
            Debug.Log(message, context);
    }
    public static void DeepDebug(object message)
    {
        WriteLine(EDebugLevel.DeepDebug, message.ToString());
        if (ConsoleDebugLevel >= EDebugLevel.DeepDebug)
            Debug.Log(message);
    }
    public static void DeepDebug(object message, UnityEngine.Object context)
    {
        WriteLine(EDebugLevel.DeepDebug, message.ToString());
        if (ConsoleDebugLevel >= EDebugLevel.DeepDebug)
            Debug.Log(message, context);
    }
    public static void RawData(object message)
    {
        WriteLine(EDebugLevel.RawData, message.ToString());
        if (ConsoleDebugLevel >= EDebugLevel.RawData)
            Debug.Log(message);
    }
    public static void RawData(object message, UnityEngine.Object context)
    {
        WriteLine(EDebugLevel.RawData, message.ToString());
        if (ConsoleDebugLevel >= EDebugLevel.RawData)
            Debug.Log(message, context);
    }

    public static void HandleUnityLog(string text, string stackTrace, LogType logType)
    {
        switch (logType)
        {
            case LogType.Error: if (FileDebugLevel < EDebugLevel.Error) return; break;
            case LogType.Warning: if (FileDebugLevel < EDebugLevel.Warning) return; break;
            case LogType.Exception: if (FileDebugLevel < EDebugLevel.Exception) return; break;
            case LogType.Assert:
            case LogType.Log:
            default: if (FileDebugLevel < EDebugLevel.Message) return; break;
        }
        if (stackTrace.Contains("Log.cs")) return;

        LogManager.WriteLine($"[{DateTime.Now:HH:mm:ss:fff}] Unity {logType}: {text}", false, true);
        if (logType.EqualsAny(LogType.Error, LogType.Exception))
            LogManager.WriteLine($"[{DateTime.Now:HH:mm:ss:fff}] StackTrace: {stackTrace}", false, true);
    }

    public static void Method(string className, string methodName, params (string name, object value)[] variables)
    {
        DeepDebug($"---{className}.{methodName}---");
        foreach ((string name, object value) in variables)
            DeepDebug($"{name}: {value}");
        DeepDebug($"------------------------------");
    }

    public static void MethodInvoked(string className, string methodName)
        => QuickDebug($"Method Invoked: {className}.{methodName}");

    public static void MethodInvoked(string className, string methodName, params object[] parameters)
    {
        StringBuilder sb = new StringBuilder($"Method Invoked: {className}.{methodName}(");
        foreach (object value in parameters)
            sb.Append($"{value}, ");
        QuickDebug(sb.Remove(sb.Length - 2, 2).Append(")").ToString());
    }

    public static void MethodInvoked(string className, string methodName, params (string name, object value)[] parameters)
    {
        StringBuilder sb = new StringBuilder($"Method Invoked: {className}.{methodName}(");
        foreach ((string name, object value) in parameters)
            sb.Append($"{name}: {value}, ");
        QuickDebug(sb.Remove(sb.Length - 2, 2).Append(")").ToString());
    }
}
