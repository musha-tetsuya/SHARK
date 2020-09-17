using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace Sunchoi {

/// <summary>
/// 共通FBXインポーター
/// </summary>
public static class FBXImporter
{
    /// <summary>
    /// 魚FBXディレクトリ
    /// </summary>
    private const string FishFbxDirectory = AssetManager.AssetBundleResourcesDirectory + "/Models/Fish";

    /// <summary>
    /// FBXファイルのパスかどうか
    /// </summary>
    private static bool IsFBX(string path)
    {
        return Path.GetExtension(path).Equals(".fbx", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// FBXインポート
    /// </summary>
    [MenuItem("Assets/Sunchoi/Import FBX")]
    private static void ImportFBX()
    {
        var fbxPaths = new List<string>();

        foreach (int instanceId in Selection.instanceIDs)
        {
            string selectPath = AssetDatabase.GetAssetPath(instanceId);

            //フォルダを選択していたら
            if (AssetDatabase.IsValidFolder(selectPath))
            {
                //フォルダ内のFBXのパスをリストに追加
                fbxPaths.AddRange(AssetDatabase
                    .FindAssets("t:GameObject", new string[]{ selectPath })
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Where(assetPath => IsFBX(assetPath))
                );
            }
            else if (IsFBX(selectPath))
            {
                fbxPaths.Add(selectPath);
            }
        }

        //選択しているFBXに対してインポート実行
        foreach (var path in fbxPaths.Distinct().ToArray())
        {
            var importer = AssetImporter.GetAtPath(path);
            if (importer is ModelImporter)
            {
                ImportFBX((ModelImporter)importer);
            }
        }
    }

    /// <summary>
    /// FBXインポート
    /// </summary>
    private static void ImportFBX(ModelImporter importer)
    {
        //魚FBXかどうか
        bool isFish = importer.assetPath.Contains(FishFbxDirectory);

        //ファイル名、ディレクトリ名
        string fbxName = Path.GetFileNameWithoutExtension(importer.assetPath);
        string directory = Path.GetDirectoryName(importer.assetPath);

        //テクスチャ吐き出し
        //importer.ExtractTextures(directory);

        //アニメーション分割データがあるかチェック
        AnimationSplitData splitData = null;
        string[] guids = AssetDatabase.FindAssets("t:AnimationSplitData", new string[]{ directory });
        if (guids.Length > 0)
        {
            splitData = AssetDatabase.LoadAssetAtPath<AnimationSplitData>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        //新規クリップ情報
        ModelImporterClipAnimation[] newClips = importer.defaultClipAnimations;

        //アニメーション分割データがあるなら
        if (splitData != null)
        {
            //アニメーション分割データでクリップ情報を上書き
            newClips = splitData.clips
                .Select(x => new ModelImporterClipAnimation
                {
                    name = x.name,
                    takeName = x.name,
                    firstFrame = x.startFrame,
                    lastFrame = x.endFrame,
                    loopTime = x.loop,
                })
                .ToArray();
        }

        //魚FBXなら
        if (isFish)
        {
            foreach (var clip in newClips)
            {
                //泳ぎのモーションはループするように
                if (clip.name == "oyogi")
                {
                    clip.loopTime = true;
                }
            }
        }

        //吐き出したテクスチャとアニメーション分割情報を保存
        importer.clipAnimations = newClips;
        importer.SaveAndReimport();

        //アニメーターコントローラが無いなら作る
        var animatorController = AnimatorUtility.FindAnimatorController(directory);
        if (animatorController == null)
        {
            animatorController = AnimatorController.CreateAnimatorControllerAtPath(Path.ChangeExtension(importer.assetPath, "controller"));
        }

        //レイヤーが消えていた場合の保険
        if (animatorController.layers == null || animatorController.layers.Length == 0)
        {
            animatorController.AddLayer("Base Layer");
        }

        //FBXに内包されているモーションをアニメーターコントローラに割り当てる
        foreach (var subAsset in AssetDatabase.LoadAllAssetRepresentationsAtPath(importer.assetPath))
        {
            if (subAsset is AnimationClip)
            {
                var animatorState = animatorController.GetAnimatorState(0, subAsset.name);
                if (animatorState == null)
                {
                    //ステートが無いなら作る
                    animatorState = animatorController.layers[0].stateMachine.AddState(subAsset.name);
                }
                animatorState.motion = (AnimationClip)subAsset;
            }
        }

        //魚FBXなら
        if (isFish)
        {
            AnimatorState oyogiState = animatorController.GetAnimatorState(0, "oyogi");
            if (oyogiState != null)
            {
                //泳ぎのモーションをデフォルトにする
                animatorController.layers[0].stateMachine.defaultState = oyogiState;

                AnimatorState hirumiState = animatorController.GetAnimatorState(0, "hirumi");
                if (hirumiState != null)
                {
                    //怯みのモーションから泳ぎのモーションへのトランジションを作成
                    AnimatorStateTransition transition = hirumiState.transitions.FirstOrDefault(x => x.destinationState == oyogiState);
                    if (transition == null)
                    {
                        transition = hirumiState.AddTransition(oyogiState);
                    }
                    transition.hasExitTime = true;
                }
            }

            //コライダデータのパス
            string colliderDataPath = SharkDefine.GetFishColliderDataPath(fbxName);
            colliderDataPath = Path.Combine(AssetManager.AssetBundleResourcesDirectory, colliderDataPath);
            colliderDataPath += ".asset";
            
            //コライダデータが無いなら作る
            var colliderData = AssetDatabase.LoadAssetAtPath<FishColliderData>(colliderDataPath);
            if (colliderData == null)
            {
                colliderData = ScriptableObject.CreateInstance<FishColliderData>();
                AssetDatabase.CreateAsset(colliderData, colliderDataPath);
            }
        }
    }

}//class FBXImporter

}//namespace Sunchoi