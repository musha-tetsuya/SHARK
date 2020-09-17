using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

/// <summary>
/// アニメーション系ユーティリティ
/// </summary>
public static class AnimatorUtility
{
#if UNITY_EDITOR
    /// <summary>
    /// Editor専用：AnimatorController内の指定ステートを取得する
    /// </summary>
    public static AnimatorState GetAnimatorState(this AnimatorController animatorController, int layerNo, string stateName)
    {
        return animatorController.layers[layerNo].stateMachine.states.Select(x => x.state).FirstOrDefault(x => x.name == stateName);
    }

    /// <summary>
    /// Editor専用：指定ディレクトリからAnimatorControllerを探す
    /// </summary>
    public static AnimatorController FindAnimatorController(string directory)
    {
        string[] guids = AssetDatabase.FindAssets("t:AnimatorController", new string[]{ directory });
        if (guids != null && guids.Length > 0)
        {
            return AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        return null;
    }
#endif
}

/// <summary>
/// アニメーションを待つ
/// </summary>
public class WaitAnimation : CustomYieldInstruction
{
    /// <summary>
    /// アニメーター
    /// </summary>
    private Animator animator = null;

    /// <summary>
    /// レイヤー番号
    /// </summary>
    private int layer = 0;

    /// <summary>
    /// 待機中かどうか
    /// </summary>
    public override bool keepWaiting => this.animator.GetCurrentAnimatorStateInfo(this.layer).normalizedTime < 1f;

    /// <summary>
    /// construct
    /// </summary>
    public WaitAnimation(Animator animator, int layer = 0)
    {
        this.animator = animator;
        this.layer = layer;
    }
}
