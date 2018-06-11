using UnityEngine;
using System.Collections;

public interface IUPKUnpack  {
    // streaming upk
    void OnStreamingError(string error);
    void OnStreamingDecompression();
    void OnStreamingProgress(float total, float current);
    void OnStreamingFinished();

    // first upk
    void OnFirstError(string error);
    void OnFirstDecompression();
    void OnFirstProgress(float tatal, float current);
    void OnFirstFinished();
}
