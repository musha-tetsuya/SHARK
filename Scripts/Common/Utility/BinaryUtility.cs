using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

/// <summary>
/// バイナリインターフェース
/// </summary>
public interface IBinary
{
    void Write(BinaryWriter writer);
    void Read(BinaryReader reader);
}

/// <summary>
/// バイナリユーティリティ
/// </summary>
public static class BinaryUtility
{
    /// <summary>
    /// バイナリデータ取得
    /// </summary>
    public static byte[] GetBinary(this IBinary bin)
    {
        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream))
        {
            bin.Write(writer);
            return stream.GetBuffer();
        }
    }

    /// <summary>
    /// バイナリデータセット
    /// </summary>
    public static void SetBinary(this IBinary bin, byte[] bytes)
    {
        using (var stream = new MemoryStream(bytes))
        using (var reader = new BinaryReader(stream))
        {
            bin.Read(reader);
        }
    }

    /// <summary>
    /// バイナリデータ取得
    /// </summary>
    public static byte[] ToBinary<T>(this IEnumerable<T> bins) where T : IBinary
    {
        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(bins.Count());
            foreach (var bin in bins)
            {
                bin.Write(writer);
            }
            return stream.GetBuffer();
        }
    }

    /// <summary>
    /// バイナリデータから配列作成
    /// </summary>
    public static T[] CreateArray<T>(byte[] bytes) where T : IBinary
    {
        T[] bins = null;
        using (var stream = new MemoryStream(bytes))
        using (var reader = new BinaryReader(stream))
        {
            int count = reader.ReadInt32();
            bins = new T[count];
            for (int i = 0; i < count; i++)
            {
                bins[i] = (T)Activator.CreateInstance(typeof(T));
                bins[i].Read(reader);
            }
        }
        return bins;
    }

    /// <summary>
    /// Vector2をバイナリ書き出し
    /// </summary>
    public static void Write(this BinaryWriter writer, Vector2 v)
    {
        writer.Write(v.x);
        writer.Write(v.y);
    }

    /// <summary>
    /// バイナリからVector2読み込み
    /// </summary>
    public static Vector2 ReadVector2(this BinaryReader reader)
    {
        Vector2 v;
        v.x = reader.ReadSingle();
        v.y = reader.ReadSingle();
        return v;
    }

    /// <summary>
    /// Vector3をバイナリ書き出し
    /// </summary>
    public static void Write(this BinaryWriter writer, Vector3 v)
    {
        writer.Write(v.x);
        writer.Write(v.y);
        writer.Write(v.z);
    }

    /// <summary>
    /// バイナリからVector3読み込み
    /// </summary>
    public static Vector3 ReadVector3(this BinaryReader reader)
    {
        Vector3 v;
        v.x = reader.ReadSingle();
        v.y = reader.ReadSingle();
        v.z = reader.ReadSingle();
        return v;
    }
}