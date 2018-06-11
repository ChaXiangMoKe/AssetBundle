using UnityEngine;
using System.Collections;

public class PGameTools  {

    // 获取设备存储空间
    public static long GetStorageSize()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject statFs = new AndroidJavaObject("android.os.StatFs",Application.persistentDataPath);
        long free = (long)statFs.Call<long>("getBlockZizeLong")*(long)statFs.Call<long>("getAvailableBoacksLong");
        return free;
#elif UNITY_IPHONE && !UNITY_EDITOR
        return 1 * 1024 * 1024 * 1024;
#else
        return 1 * 1024 * 1024 * 1024;
#endif
    }
}
