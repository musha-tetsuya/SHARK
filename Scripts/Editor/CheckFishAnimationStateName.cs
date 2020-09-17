using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static class CheckFishAnimationStateName
{
    /// <summary>
    /// 魚のアニメーション名が正しいかどうか一括チェック
    /// </summary>
    [MenuItem("Tools/Check FishAnimation StateName")]
    private static void CheckFishAnimation()
    {
        //魚FBXのパス一覧を取得
        var fishFbxPathList = AssetDatabase
            .FindAssets("t:GameObject", new string[]{"Assets/Sunchoi/AssetbundleResources/Resources/Models/Fish"})
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path => Path.GetExtension(path).Equals(".fbx", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        int errorCount = 0;

        foreach (string path in fishFbxPathList)
        {
            var animatorController = AnimatorUtility.FindAnimatorController(Path.GetDirectoryName(path));
            if (animatorController != null)
            {
                foreach (var childAnimatorState in animatorController.layers[0].stateMachine.states)
                {
                    string stateName = childAnimatorState.state.name;
                    if (stateName != "oyogi"
                    &&  stateName != "hirumi"
                    &&  stateName != "hokakuseikou")
                    {
                        errorCount++;
                        Debug.LogWarningFormat("{0} : {1}", path, stateName);
                    }
                }
            }
        }

        Debug.LogFormat("魚のアニメーション名エラー数{0}件", errorCount);
    }
}
