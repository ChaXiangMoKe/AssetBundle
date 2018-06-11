using UnityEngine;
using System;
using System.Collections;

public interface IHotEngineUI {

    // 初始化界面
    void HOT_InitUI();

    // streaming upk
    void HOT_InitUnpackStream();
    void HOT_UnpackStreamingDecompression();
    void HOT_SetUnpackStreamProgress(float total,float current);
    void HOT_UnpackStreamFinished();
    void HOT_UnpackStreamError(string error,HotEngine engine);

    // download first zip
    void HOT_InitFirst();
    void HOT_InitDownloadFirst();
    void HOT_SetDownloadFirstProgress(float progress);
    void HOT_DownloadFirstFinished();
    void HOT_DownloadFirstError(string error, HotEngine engine);

    // first upk
    void HOT_InitUnpackFirst();
    void HOT_UnpackFirstDecompression();
    void HOT_SetUnpackFirstProgress(float tatal, float current);
    void HOT_UnpackFirstFirstFinished();
    void HOT_UnpackFirstError(string error, HotEngine engine);

    // 初始化检查更新界面
    void HOT_InitCheckUpdate();
    void HOT_CheckUpdateSure(Action sureAction,Action cancelAction,HotEngine engine);
    void HOT_CheckHotUpdateError(string error, HotEngine engine);

    // 下载 hot 文件
    void HOT_InitHot();
    void HOT_SetHotProgress(float total, float current, float progress);
    void HOT_HotFinished();
    void HOT_HotError(string error, HotEngine engine);

    // 进入游戏
    void EnterGame();

    // 内存不足提示
    void OutOfMemory(long memorySize);

    // 断网提示
    void NetworkInterruption(HotEngine engine);

    // popup
    void ShowPopup(string title, string info, Action sureAction, Action cancelAction, string sureBtnText, string cancelBtnText);
    void ShowPopupSure(string title, string info, Action sureAction, string sureBtnText);
    void ShowPopupCancel(string title,string info,Action cancelAction,string cancelBtnText);
}
