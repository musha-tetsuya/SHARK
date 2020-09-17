using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 時間停止管理
/// </summary>
public static class TimePauseManager
{
    /// <summary>
    /// 時間停止対象と復帰データのセット
    /// </summary>
    private class TimePauseData
    {
        /// <summary>
        /// 対象
        /// </summary>
        public ITimePause target = null;
        /// <summary>
        /// 復帰データ
        /// </summary>
        public byte[] resumeData = null;
    }

    /// <summary>
    /// 停止中かどうか
    /// </summary>
    public static bool isPause { get; private set; }
    /// <summary>
    /// 管理対象リスト
    /// </summary>
    private static List<TimePauseData> targetList = new List<TimePauseData>();

    /// <summary>
    /// 管理リストへの追加
    /// </summary>
    public static void Add(ITimePause item)
    {
        if (!targetList.Exists(x => x.target == item))
        {
            targetList.Add(new TimePauseData{ target = item });
        }
    }

    /// <summary>
    /// 管理リストからの除去
    /// </summary>
    public static void Remove(ITimePause item)
    {
        targetList.RemoveAll(x => x.target == item);
    }

    /// <summary>
    /// 停止
    /// </summary>
    public static void Pause()
    {
        if (!isPause)
        {
            isPause = true;

            for (int i = 0; i < targetList.Count; i++)
            {
                using (var stream = new MemoryStream())
                using (var writer = new BinaryWriter(stream))
                {
                    targetList[i].target.Pause(writer);
                    targetList[i].resumeData = stream.GetBuffer();
                }
            }
        }
    }

    /// <summary>
    /// 再開
    /// </summary>
    public static void Play()
    {
        if (isPause)
        {
            for (int i = 0; i < targetList.Count; i++)
            {
                using (var stream = new MemoryStream(targetList[i].resumeData))
                using (var reader = new BinaryReader(stream))
                {
                    targetList[i].target.Play(reader);
                    targetList[i].resumeData = null;
                }
            }

            isPause = false;
        }
    }
}
