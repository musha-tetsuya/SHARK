using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// パス系ユーティリティ
/// </summary>
public static class PathUtility
{
    /// <summary>
    /// 拡張子除去
    /// </summary>
    public static string RemoveExtension(this string path)
    {
        string extension = Path.GetExtension(path);
        if (!string.IsNullOrEmpty(extension))
        {
            path = path.Replace(extension, null);
        }
        return path;
    }
}
