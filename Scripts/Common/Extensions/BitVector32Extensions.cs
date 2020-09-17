using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

/// <summary>
/// BitVector32拡張
/// </summary>
public static class BitVector32Extentions
{
    /// <summary>
    /// Get
    /// </summary>
    public static bool Get(this BitVector32[] bits, int index)
    {
        return bits[index / 32][1 << (index % 32)];
    }

    /// <summary>
    /// Set
    /// </summary>
    public static void Set(this BitVector32[] bits, int index, bool value)
    {
        bits[index / 32][1 << (index % 32)] = value;
    }
}
