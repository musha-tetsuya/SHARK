using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// 魚の状態異常タイプ
/// </summary>
public enum FishConditionType
{
    Dead,       //死亡
    Freezing,   //氷結
    PauseEvent, //一時停止イベント
    Damaged,    //被弾
    SpeedDown,  //移動速度低下
    Summon,     //召喚中
}

}