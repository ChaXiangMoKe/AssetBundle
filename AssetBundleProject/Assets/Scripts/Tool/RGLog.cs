using UnityEngine;
using System.Collections;
using System.IO;
using System.Diagnostics;

public class RGLog
{


    // log 输出路径
    private static string _LogFilePath = "";
    private static string LogFilePath
    {
        get
        {
            if (string.IsNullOrEmpty(_LogFilePath))
            {
                var localfile = "Rhealog.txt";
                var logPath = Application.persistentDataPath + "/slg/";
                var localfilepath = Path.Combine(logPath, localfile);
                _LogFilePath = localfilepath;
            }
            return _LogFilePath;
        }
    }
    // 是否开启日志过滤
    public static bool isOpenFilter = false;
    // 是否开启日志
    public static bool EnableLog = true;
    // 是否开启Debug日志
    public static bool EnableDebugLog = true;
    // 是否开启日志写入记录
    public static bool EnbleDebugWriter = false;
    // 日志写入流
    private static StreamWriter _OutputStream;

    public static void HandleLogCallBack(string condition, string stacktrace, LogType type)
    {
        if (!EnableLog)
            return;

        if (!EnableLog)
            return;

        if (_OutputStream == null)
        {
            _OutputStream = new StreamWriter(LogFilePath, false);
        }

        if (type == LogType.Exception)
        {
            _OutputStream.WriteLine("<B>Exception<B>");
            _OutputStream.WriteLine();
            _OutputStream.WriteLine(stacktrace);
            _OutputStream.WriteLine();
            _OutputStream.WriteLine("<B>Exception<B>");
        }

        if (condition.StartsWith("<color"))
        {
            _OutputStream.WriteLine(string.Format("-{0}-", Time.realtimeSinceStartup.ToString("f3")));
            _OutputStream.WriteLine(condition);
        }

        _OutputStream.Flush();
    }

    public static void Close()
    {
        if(_OutputStream != null)
        {
            _OutputStream.Close();
            _OutputStream = null;
        }
    }

    /// <summary>
    /// Log the specified format and args.
    /// </summary>
    /// <param name="format">Format.</param>
    /// <param name="args">Arguments.</param>
    public static void Debug(object format, params object[] args)
    {
        if (!EnableDebugLog)
            return;

        if (GlobaFilter(format, args))
        {
            UnityEngine.Debug.LogFormat(string.Format("<color=red>[Debug]</color>") + format.ToString(), args);
        }

    }

    /// <summary>
    /// Debugs the error.
    /// </summary>
    /// <param name="format">Format.</param>
    /// <param name="args">Arguments.</param>
    public static void DebugError(object format, params object[] args)
    {
        if (!EnableDebugLog)
            return;

        if (GlobaFilter(format, args))
        {
            UnityEngine.Debug.LogErrorFormat(string.Format("<color=red>[ERROR]</color>") + format.ToString(), args);
        }

    }

    /// <summary>
    /// Log the specified format and args.
    /// </summary>
    /// <param name="format">Format.</param>
    /// <param name="args">Arguments.</param>
    public static void Log(object format, params object[] args)
    {
        if (!EnableDebugLog)
            return;

        if (GlobaFilter(format, args))
        {
            UnityEngine.Debug.LogFormat(string.Format("<color=green>[LOG]</color>") + format.ToString(), args);
        }

    }

    /// <summary>
    /// Debugs the error.
    /// </summary>
    /// <param name="format">Format.</param>
    /// <param name="args">Arguments.</param>
    public static void Warn(object format, params object[] args)
    {
        if (!EnableDebugLog)
            return;

        if (GlobaFilter(format, args))
        {
            UnityEngine.Debug.LogWarningFormat(string.Format("<color=yellow>[WARN]</color>") + format.ToString(), args);
        }

    }

    /// <summary>
    /// Error the specified format and args.
    /// </summary>
    /// <param name="format">Format.</param>
    /// <param name="args">Arguments.</param>
    public static void Error(object format, params object[] args)
    {
        if (!EnableLog)
            return;

        UnityEngine.Debug.LogErrorFormat("<color=red>[ERROR]</color>" + format.ToString(), args);
    }

    public static void Exception(System.Exception e)
    {
        if (!EnableLog)
            return;

        UnityEngine.Debug.LogException(e);
    }

    public static bool GlobaFilter(object format, params object[] args)
    {
        if (!isOpenFilter)
            return true;

        var st = new StackTrace(1, true);
        var txt = st.ToString();
        if (txt.Contains("ClientEvent"))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    ///  资源加载Debug Log
    /// </summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    [ConditionalAttribute("UNITY_EDITOR")]
    public static void DebugResLoad(string format,params object[] args)
    {
        if (!EnableDebugLog)
            return;

        #if UNITY_EDITOR
                UnityEngine.Debug.LogFormat(string.Intern("<color=red>[ResLoad]</color>-") + format, args);
        #endif
    }
}
