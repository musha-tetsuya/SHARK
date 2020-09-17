using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 魚編隊イベントレシーバー
/// </summary>
public interface IFormationEventReceiver
{
    void OnPlay();
    void OnPause();
    void OnQuit();
}
