using UnityEngine;
using System.Collections;

public class VersionInfo {

    public enum BUILD_TYPE
    {
        DEVELOP,
        DEVELOP_SELECT_SERVER,
        DEVELOP_SPECITFY_SERVER,
        LIVE,
        LIVE_LOG,
    }

    public static BUILD_TYPE BType = BUILD_TYPE.DEVELOP;
    public static bool IsCompression = false;
    public static string BundleVersion = "0.1.1";
}
