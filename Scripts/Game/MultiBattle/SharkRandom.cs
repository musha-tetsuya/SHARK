using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ランダム
/// </summary>
public class SharkRandom : IBinary
{
    /// <summary>
    /// Seed値
    /// </summary>
    private int seed = 0;
    /// <summary>
    /// 次のSeed値
    /// </summary>
    private int nextSeed = 0;

    /// <summary>
    /// construct
    /// </summary>
    public SharkRandom()
    {
        this.SetSeed(Random.Range(int.MinValue, int.MaxValue));
    }

    /// <summary>
    /// シード値セット
    /// </summary>
    private void SetSeed(int seed)
    {
        this.seed = seed;
        Random.InitState(this.seed);
        this.nextSeed = Random.Range(int.MinValue, int.MaxValue);
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset()
    {
        this.SetSeed(this.nextSeed);
    }

    /// <summary>
    /// 値取得
    /// </summary>
    private T GetValue<T>(System.Func<T> func)
    {
        Random.InitState(this.seed);
        this.seed = Random.Range(int.MinValue, int.MaxValue);
        return func();
    }

    public int Range(int min, int max)
    {
        return this.GetValue(() => Random.Range(min, max));
    }

    public float Range(float min, float max)
    {
        return this.GetValue(() => Random.Range(min, max));
    }

    public float value
    {
        get => this.GetValue(() => Random.value);
    }

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        writer.Write(this.seed);
        writer.Write(this.nextSeed);
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        this.seed = reader.ReadInt32();
        this.nextSeed = reader.ReadInt32();
    }
}
