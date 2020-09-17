using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// バトルシーン基底
/// </summary>
public class BattleSceneBase : SceneBase
{
    /// <summary>
    /// 魚生成時
    /// </summary>
    public virtual void OnCreateFish(Fish fish){}

    /// <summary>
    /// 魚捕獲確率計算
    /// </summary>
    public virtual bool FishCatchingCalculation(Fish fish){ return false; }

    /// <summary>
    /// 魚捕獲時
    /// </summary>
    public virtual void OnFishCatched(Fish fish, Bullet bullet){}

    /// <summary>
    /// 自動照準時のターゲット選定処理
    /// </summary>
    public virtual Fish FindTargetFish(){ return null; }

    /// <summary>
    /// 魚捕獲時のFVポイント獲得値計算
    /// </summary>
    protected int CalcFvPointOnFishCatched(Fish fish, Bullet bullet)
    {
        //FVポイント獲得％
        uint fvRate = 0;

        //魚によるFVポイント獲得％増加
        if (fish.statusMaster is Master.IStageFishData)
        {
            fvRate += (fish.statusMaster as Master.IStageFishData).fvRate;
        }

        //弾丸（台座＋ギア）によるFVポイント獲得％増加
        fvRate += bullet.fvRate;

        //FVポイント増加：小数点は四捨五入
        int fvPoint = Mathf.RoundToInt(bullet.bet * fvRate * Masters.PercentToDecimal);

        if (this is MultiBattleScene)
        {
            //スキルによるFVポイント追加獲得（マルチのみ）
            fvPoint += Mathf.RoundToInt(fvPoint * BattleGlobal.instance.userData.skill.FvGetUp() * 10) / 10;
        }

        //最小１は獲得
        fvPoint = Mathf.Max(1, fvPoint);

        return fvPoint;
    }

    /// <summary>
    /// 砲台制御機能変更時
    /// </summary>
    public virtual void OnSetTurretController(ITurretController turretController){}

#if DEBUG
    /// <summary>
    /// GUIボタンスタイル
    /// </summary>
    private GUIStyle buttonStyle = null;

    /// <summary>
    /// GUI描画
    /// </summary>
    public override void DrawGUI()
    {
        if (this.buttonStyle == null)
        {
            this.buttonStyle = UIUtility.CreateButtonStyle(300f, 30);
        }

        if (GUILayout.Button("ワンキル:" + BattleDebug.isOneKill, this.buttonStyle))
        {
            BattleDebug.isOneKill = !BattleDebug.isOneKill;
        }

        if (GUILayout.Button("FVMAX", this.buttonStyle))
        {
            this.OnDebugFvMax();
        }
    }

    /// <summary>
    /// デバッグ：FVMAX
    /// </summary>
    protected virtual void OnDebugFvMax(){}
#endif
}

#if DEBUG
public static class BattleDebug
{
    public static bool isOneKill = false;
    public static bool isConfirmGetBall = false;
    public static bool isConfirmGetSoul = false;
}
#endif
}