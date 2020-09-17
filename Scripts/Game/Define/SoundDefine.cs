using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

/// <summary>
/// SE名
/// </summary>
public static class SeName
{
    //メニュー系
    public const string YES = "common_yes";
    public const string NO = "common_no";
    public const string PARTS_CHANGE = "parts_change";
    public const string WAVE = "wave";

    //バトル系
    public const string CAPTURE_SINGLE = "capture_single";
    public const string CAPTURE_MULTI = "capture";
    public const string ITEM_AUTO = "item_auto";
    public const string ITEM_ICE = "item_ice";
    public const string ITEM_SUMMON = "item_summon";
    public const string FVATTACK_OK = "fva_ok";
    public const string FVATTACK_START = "fva_start";
    public const string FVATTACK_LASERBEAM = "fva_beam";
    public const string FVATTACK_BOMB = "fva_bomb";
    public const string FVATTACK_ALLRANGE = "fva_bomb_all";
    public const string FVATTACK_PENETRATE = "fva_penetrate";
    public const string SINGLEBATTLE_START = "singlegame_start";
    public const string SINGLEBATTLE_WIN = "singlegame_win";
    public const string SINGLEBATTLE_LOSE = "singlegame_lose";
    public const string BALL_DROP = "ball_drop";
    public const string BALL_GET = "ball_get";
    public const string SOUL_DROP = "soul_drop";
    public const string SOUL_GET = "soul_get";
    public const string SOUL_COMP = "soul_comp";
    public const string WHALE_SLOT = "whale_slot";
    public const string SLOT_STOP = "slot_stop";
    public const string WIN_0 = "win_0";
    public const string WIN_1 = "win_1";
    public const string BUTTON_BET = "button_bet";

    //FVアタックタイプからSE名への辞書
    public static readonly ReadOnlyDictionary<int, string> FvAttackSeName = new ReadOnlyDictionary<int, string>(new Dictionary<int, string>
    {
        { (int)FvAttackType.AllRange,    FVATTACK_ALLRANGE  },
        { (int)FvAttackType.Bomb,        FVATTACK_BOMB      },
        { (int)FvAttackType.LaserBeam,   FVATTACK_LASERBEAM },
        { (int)FvAttackType.MultiWay,    null               },
        { (int)FvAttackType.Penetration, FVATTACK_PENETRATE },
    });
}

/// <summary>
/// BGM名
/// </summary>
public static class BgmName
{
    //メニュー系
    public const string SELECT = "bgm_select";
    public const string HOME = "bgm_home";

    //バトル系
    public const string WHALEDIVE = "bgm_whaledive";
}
