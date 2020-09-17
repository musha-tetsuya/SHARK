using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Newtonsoft.Json;

/// <summary>
/// バッチビルド
/// </summary>
public class BatchBuild : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    /// <summary>
    /// ビルドモード
    /// </summary>
    private enum BuildMode
    {
        Debug,      //デバッグ
        Release,    //リリース
    }

    /// <summary>
    /// コマンドライン引数
    /// </summary>
    private class CommandLineArgs
    {
        public BuildTargetGroup buildTargetGroup = BuildTargetGroup.Android;
        public BuildTarget buildTarget = BuildTarget.Android;
        public BuildMode buildMode = BuildMode.Debug;
        public Language language = Language.Ja;
        public bool isProduction = false;
        public bool isOffline = false;
        public bool useAssetbundle = false;
        public string locationPathName = null;
        public int buildNumber = 0;
        public string bundleVersion = "1.0.0";
        public string resourceVersion = null;
    }

    /// <summary>
    /// オフラインモード用マスターデータ作成
    /// </summary>
    private static void CreateOfflineMasterData()
    {
        //不要Jsonの破棄
        string[] wasteJsonPaths = AssetDatabase
            .FindAssets("", new[]{"Assets/Sunchoi/BuiltinResources/Resources/Json"})
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path => !path.Contains("BuiltinLocalizeTextData"))
            .ToArray();

        foreach (string path in wasteJsonPaths)
        {
            AssetDatabase.DeleteAsset(path);
        }

        //オフラインモード用マスターデータJson作成
        var args = GetCommandLineArgs();

        if (args.isOffline)
        {
            CsvToJsonConverter.Convert(
                Application.dataPath.Replace("Assets", "FishShootingMasterData/Csv"),
                Application.dataPath + "/Sunchoi/BuiltinResources/Resources/Json"
            );
        }
    }

    /// <summary>
    /// ビルド
    /// </summary>
    private static void Build()
    {
        //コマンドライン引数解析
        var args = GetCommandLineArgs();

        Debug.LogFormat("########## Start Build {0} ##########", args.buildTarget);

        //ビルドターゲット切り替え
        EditorUserBuildSettings.SwitchActiveBuildTarget(args.buildTargetGroup, args.buildTarget);

        //アプリ名、ビルド番号設定
        PlayerSettings.productName = GetProductName(args.language, args.buildMode);
        PlayerSettings.bundleVersion = args.bundleVersion;
        PlayerSettings.Android.bundleVersionCode = args.buildNumber;
        PlayerSettings.iOS.buildNumber = args.buildNumber.ToString();

        //ビルド情報保存
        var buildData = AssetDatabase.LoadAssetAtPath<BuildData>("Assets/Sunchoi/BuiltinResources/References/ScriptableObject/BuildData.asset");
        buildData.buildNumber = args.buildNumber;
        EditorUtility.SetDirty(buildData);
        AssetDatabase.SaveAssets();

        //AndroidのKeyStore設定
        if (args.buildTarget == BuildTarget.Android)
        {
            SetAndroidKeyStore(args.buildMode);
        }

        //Define定義
        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            args.buildTargetGroup,
            GetDefineSymbols(args.language, args.isProduction, args.isOffline, args.useAssetbundle)
        );

        //ビルド実行
        var report = BuildPipeline.BuildPlayer(
            EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray(),
            args.locationPathName,
            args.buildTarget,
            GetBuildOptions(args.buildTarget, args.buildMode)
        );

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.LogFormat("########## Success Build {0} ##########", args.buildTarget);
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogFormat("########## Failed Build {0} ##########", args.buildTarget);
            EditorApplication.Exit(-1);
        }
    }

    /// <summary>
    /// アセットバンドルビルド
    /// </summary>
    private static void BuildAssetbundles()
    {
        //コマンドライン引数解析
        var args = GetCommandLineArgs();

        Debug.LogFormat("########## Start BuildAssetbundles {0} ##########", args.buildTarget);

        //ディレクトリ作成
        string directoryName = "AssetBundle";
        string directoryPath = Application.dataPath.Replace("Assets", directoryName);
        Directory.CreateDirectory(directoryPath);

        //アセットバンドルビルド
        var manifest = BuildPipeline.BuildAssetBundles(
            directoryPath,
            BuildAssetBundleOptions.None,
            args.buildTarget
        );

        //アセットバンドル情報リスト
        var infoList = manifest
            .GetAllAssetBundles()
            .Select(assetBundleName =>
            {
                string path = Path.Combine(directoryPath, assetBundleName);
                uint crc;
                BuildPipeline.GetCRCForAssetBundle(path, out crc);
                var fileInfo = new FileInfo(path);

                return new AssetBundleInfo {
                    assetBundleName = assetBundleName,
                    crc = crc,
                    dependencies = manifest.GetAllDependencies(assetBundleName),
                    fileSize = fileInfo.Length,
                };
            })
            .ToList();

        //アセットバンドル情報Json吐き出し
        File.WriteAllText(Path.Combine(directoryPath, "infoList.json"), JsonConvert.SerializeObject(infoList, Formatting.Indented));

        //バージョン別ディレクトリ作成
        string verDirPath = Application.dataPath.Replace("Assets", args.resourceVersion);
        Directory.CreateDirectory(verDirPath);

        //アセットバンドル情報バイナリ保存
        File.WriteAllBytes(
            Path.Combine(verDirPath, "infoList.dat".GetHashString()),
            infoList.ToBinary().Cryption()
        );

        //アセットバンドルを暗号化して保存
        foreach (var assetBundleName in manifest.GetAllAssetBundles())
        {
            var path = Path.Combine(verDirPath, assetBundleName.GetHashString());
            var bytes = File.ReadAllBytes(Path.Combine(directoryPath, assetBundleName)).Cryption();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, bytes);
        }

        Debug.LogFormat("########## End BuildAssetbundles {0} ##########", args.buildTarget);
    }

    /// <summary>
    /// コマンドライン引数解析
    /// </summary>
    private static CommandLineArgs GetCommandLineArgs()
    {
        var result = new CommandLineArgs();
        string[] args = Environment.GetCommandLineArgs();

        for (int i = 0, imax = args.Length; i < imax; i++)
        {
            switch (args[i])
            {
                case "-platform":
                result.buildTargetGroup = (BuildTargetGroup)Enum.Parse(typeof(BuildTargetGroup), args[i + 1]);
                result.buildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), args[i + 1]);
                break;

                case "-buildMode":
                result.buildMode = (BuildMode)Enum.Parse(typeof(BuildMode), args[i + 1]);
                break;

                case "-language":
                result.language = (Language)Enum.Parse(typeof(Language), args[i + 1]);
                break;

                case "-isProduction":
                result.isProduction = bool.Parse(args[i + 1]);
                break;

                case "-isOffline":
                result.isOffline = bool.Parse(args[i + 1]);
                break;

                case "-useAssetbundle":
                result.useAssetbundle = bool.Parse(args[i + 1]);
                break;

                case "-locationPathName":
                result.locationPathName = args[i + 1];
                break;

                case "-buildNumber":
                result.buildNumber = int.Parse(args[i + 1]);
                break;

                case "-bundleVersion":
                result.bundleVersion = args[i + 1];
                break;

                case "-resourceVersion":
                result.resourceVersion = args[i + 1];
                break;
            }
        }

        if (result.buildMode == BuildMode.Debug)
        {
            result.isProduction = false;
        }

        return result;
    }

    /// <summary>
    /// アプリ名取得
    /// </summary>
    private static string GetProductName(Language language, BuildMode buildMode)
    {
        string productName = (language == Language.Ja) ? "SunFishing"
                           : (language == Language.Zh) ? "阳阳捕鱼"
                           : (language == Language.Tw) ? "陽陽捕魚"
                           : (language == Language.En) ? "SunFishing"
                           : "SHARK";

        if (buildMode == BuildMode.Debug)
        {
            productName += "-Debug";
        }

        return productName;
    }

    /// <summary>
    /// AndroidのKeystore設定
    /// </summary>
    private static void SetAndroidKeyStore(BuildMode buildMode)
    {
        if (buildMode == BuildMode.Debug)
        {
            PlayerSettings.Android.keystoreName = null;
            PlayerSettings.Android.keystorePass = null;
            PlayerSettings.Android.keyaliasName = null;
            PlayerSettings.Android.keyaliasPass = null;
        }
        else
        {
            PlayerSettings.Android.keystoreName = "/Users/compile/Keystore/shark.keystore";
            PlayerSettings.Android.keystorePass = "Sunchoi4259";
            PlayerSettings.Android.keyaliasName = "shark";
            PlayerSettings.Android.keyaliasPass = "Sunchoi4259";
        }
    }

    /// <summary>
    /// Defineシンボル取得
    /// </summary>
    private static string GetDefineSymbols(
        Language language,
        bool isProduction,
        bool isOffline,
        bool useAssetbundle)
    {
        string defineSymbols = null;

        //言語
        defineSymbols += string.Format("LANGUAGE_{0}", language.ToString().ToUpper());

        //製品版
        if (isProduction)
        {
            defineSymbols += ";SHARK_PRODUCTION";
        }

        //オフラインモード
        if (isOffline)
        {
            defineSymbols += ";SHARK_OFFLINE";
        }

        if (useAssetbundle)
        {
            defineSymbols += ";USE_ASSETBUNDLE";
        }

        return defineSymbols;
    }

    /// <summary>
    /// ビルドオプション値取得
    /// </summary>
    private static BuildOptions GetBuildOptions(BuildTarget buildTarget, BuildMode buildMode)
    {
        var opt = BuildOptions.None;
        if (buildTarget == BuildTarget.iOS)
        {
            opt |= BuildOptions.SymlinkLibraries;
        }
        if (buildMode == BuildMode.Debug)
        {
            opt |= BuildOptions.Development
                |  BuildOptions.ConnectWithProfiler
                |  BuildOptions.AllowDebugging;
        }
        return opt;
    }

    int IOrderedCallback.callbackOrder
    {
        get { return 0; }
    }

    void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
    {
        //TODO:アセットバンドルリソースの削除（プロジェクト外に移動するだけでも良いかも）
    }

    void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report)
    {

    }
}
