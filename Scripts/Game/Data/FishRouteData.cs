using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using PathCreation.Utility;

/// <summary>
/// 魚回遊ルートデータ
/// </summary>
public class FishRouteData : MonoBehaviour
{
    [SerializeField, Header("回遊ルートパス")]
    public PathCreator pathCreator = null;

    [SerializeField, Header("魚ID")]
    public uint fishId = 1;

    [SerializeField, Header("生成数")]
    public uint amount = 1;

    [SerializeField, Header("生成間隔(秒)")]
    public float interval = 1f;

    [SerializeField, Header("ユニット数1～5")]
    public uint unitSize = 1;

    [SerializeField, Header("ユニットの広がり")]
    public float unitSpread = 1f;

    [SerializeField, Header("移動速度")]
    public float moveSpeed = 1f;

    [SerializeField, Header("回転時間(360度回転するのに何秒かかるか)")]
    public float rotationTime = 1f;

    [SerializeField, Header("ルートを何回折り返しループするか")]
    public uint loopCount = 0;

    [SerializeField, Header("ルートのスタートとゴールを逆転させるかどうか")]
    public bool isReverse = false;

    /// <summary>
    /// 回遊ポイント取得
    /// </summary>
    public Vector3 GetPoint(int index)
    {
        var path = this.pathCreator.path;
        var p = path.GetPoint(index);
        return MathUtility.TransformPoint(p, this.transform, path.space);
    }

#if UNITY_EDITOR
    /// <summary>
    /// OnDrawGizmos
    /// </summary>
    public void OnDrawGizmos()
    {
        if (this.pathCreator != null)
        {
            var path = this.pathCreator.path;

            Gizmos.color = Color.green;

            for (int i = 0; i < path.NumPoints; i++)
            {
                int nextI = i + 1;

                if (nextI >= path.NumPoints)
                {
                    if (path.isClosedLoop)
                    {
                        nextI %= path.NumPoints;
                    }
                    else
                    {
                        break;
                    }
                }

                Gizmos.DrawLine(this.GetPoint(i), this.GetPoint(nextI));
            }
        }
    }
#endif

    /// <summary>
    /// Delay付き回遊ルートデータ
    /// </summary>
    [Serializable]
    public class WithDelay
    {
        /// <summary>
        /// 回遊ルートデータ
        /// </summary>
        [SerializeField]
        public FishRouteData data = null;

        /// <summary>
        /// Delay
        /// </summary>
        [SerializeField]
        public float delay = 0f;
    }

    /// <summary>
    /// 確率付き回遊ルートデータ
    /// </summary>
    [Serializable]
    public class WithProbability
    {
        /// <summary>
        /// 回遊ルートデータ
        /// </summary>
        [SerializeField]
        public FishRouteData data = null;

        /// <summary>
        /// 抽選確率
        /// </summary>
        [SerializeField]
        public uint probability = 50;
    }
}