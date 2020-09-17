using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 変更していないSpriteAtlasがGitで変更状態になる現象を防ぐやつ
/// </summary>
public class SpriteAtlasPostprocessor : AssetPostprocessor
{
    private const string WINDOWS = "\r\n";
    private const string UNIX    = "\n";
    private const string MAC     = "\r";

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        var list = importedAssets.Where(x => x.EndsWith(".spriteatlas")).ToArray();

        if (list == null || list.Length <= 0) return;

        foreach (var path in list)
        {
            var text = File.ReadAllText(path);
            text = text.Replace(WINDOWS, UNIX);
            text = text.Replace(MAC, UNIX);
            text = text.Replace(UNIX, WINDOWS);
            File.WriteAllText(path, text);
        }

        AssetDatabase.SaveAssets();
    }
}
