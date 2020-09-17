#if UNITY_EDITOR
//#define FISHCOLLIDER2D_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battle {

/// <summary>
/// 魚当たり判定用コライダ
/// </summary>
public class FishCollider2D : Graphic
{
    /// <summary>
    /// 頂点調整用Vector3値
    /// </summary>
    private static Vector3[] adjustVerts =
    {
        new Vector3(1f, 1f, 1f),    //右上
        new Vector3(-1f, 1f, 1f),   //左上
        new Vector3(1f, -1f, 1f),   //右下
        new Vector3(-1f, -1f, 1f),  //左下
    };

    /// <summary>
    /// コライダ
    /// </summary>
    [SerializeField]
    public PolygonCollider2D polygonCollider = null;
    /// <summary>
    /// BoxColliderの８頂点プレハブ
    /// </summary>
    [SerializeField]
    private BoxColliderPoints boxColliderPointsPrefab = null;

    /// <summary>
    /// どの魚のコライダなのか
    /// </summary>
    public Fish fish { get; private set; }
    /// <summary>
    /// モデルのコライダ
    /// </summary>
    public BoxCollider boxCollider { get; private set; }
    /// <summary>
    /// BoxColliderの８頂点
    /// </summary>
    private BoxColliderPoints boxColliderPoints = null;
    /// <summary>
    /// 親のRectTransform
    /// </summary>
    private RectTransform parentRect = null;
    /// <summary>
    /// BoxColliderの8頂点のキャンバス座標
    /// </summary>
    private Vector2[] canvasPoints = new Vector2[8];
    /// <summary>
    /// PolygonCollider2Dの頂点インデックス
    /// </sumamry>
    private List<int> pointIndexList = new List<int>();

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup(Fish fish, FishColliderData colliderData)
    {
        this.fish = fish;
        this.parentRect = (RectTransform)this.rectTransform.parent;

        //コライダ設置先
        Transform colliderPlacement = null;

        if (!string.IsNullOrEmpty(colliderData.placementName))
        {
            //コライダ設置先の検索
            colliderPlacement = this.fish.model
                .GetComponentsInChildren<Transform>()
                .FirstOrDefault(child => child.name == colliderData.placementName);
        }

        if (colliderPlacement == null)
        {
            //基本はFBXのルートがコライダ設置先
            colliderPlacement = this.fish.cachedModelTransform;
        }

        //コライダ設置
        this.boxCollider = colliderPlacement.gameObject.AddComponent<BoxCollider>();
        this.boxCollider.center = colliderData.center;
        this.boxCollider.size = colliderData.size;
        this.boxColliderPoints = Instantiate(this.boxColliderPointsPrefab, colliderPlacement, false);
    }

    /// <summary>
    /// スクリーン内にいるかどうか
    /// </summary>
    public bool IsInScreen()
    {
        if (this.polygonCollider.enabled)
        {
            for (int i = 0; i < this.canvasPoints.Length; i++)
            {
                //画面内にコライダの頂点が一つでも入っていればtrue
                if (this.parentRect.rect.Contains(this.canvasPoints[i] + this.rectTransform.anchoredPosition))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 頂点更新
    /// </sumamry>
    public void UpdateVertices()
    {
        //BoxColliderの8頂点をキャンバス内での座標に変換
        for (int i = 0; i < 8; i++)
        {
            //UIキャンバス内での座標に変換
            this.canvasPoints[i] = BattleGlobal.instance.WorldToCanvasPoint(this.boxColliderPoints[i].position - this.boxColliderPoints.cachedTransform.position);
        }

        //xが最大、最小の頂点インデックスを求める
        int minXIdx = 0;
        int maxXIdx = 0;
        for (int i = 0; i < 8; i++)
        {
            if (this.canvasPoints[i].x < this.canvasPoints[minXIdx].x)
            {
                minXIdx = i;
            }
            if (this.canvasPoints[i].x > this.canvasPoints[maxXIdx].x)
            {
                maxXIdx = i;
            }
        }

        //最初は一番左の点から
        this.pointIndexList.Clear();
        this.pointIndexList.Add(minXIdx);

        //ぐるっと一周するように外周の点をリストに詰める
        while (this.pointIndexList.Count < 8)
        {
            int i = this.pointIndexList.Last();
            int maxAngleIndex = 0;
            float maxAngle = 0f;
            bool isReverse = this.pointIndexList.Contains(maxXIdx);//一番右の点まで到達したかどうか

            for (int j = 0; j < 8; j++)
            {
                //半周終える前
                if (!isReverse)
                {
                    //自分より左にある点はスキップ
                    if (this.canvasPoints[j].x < this.canvasPoints[i].x)
                    {
                        continue;
                    }
                    //すでにリストに含まれている点はスキップ
                    if (this.pointIndexList.Contains(j))
                    {
                        continue;
                    }
                }
                //半周終えた後
                else
                {
                    //自分より右にある点はスキップ
                    if (this.canvasPoints[j].x > this.canvasPoints[i].x)
                    {
                        continue;
                    }
                    //ゴール地点(=スタート地点)以外ですでにリストに含まれている点はスキップ
                    if (j != this.pointIndexList[0] && this.pointIndexList.Contains(j))
                    {
                        continue;
                    }
                }

                //角度が最大になる点が次の外周の点となる
                Vector2 v1 = isReverse ? Vector2.right : Vector2.left;
                Vector2 v2 = this.canvasPoints[j] - this.canvasPoints[i];
                float angle = Vector2.SignedAngle(v1, v2);
                if (angle < 0f)
                {
                    angle += 360f;
                }

                if (angle > maxAngle)
                {
                    maxAngle = angle;
                    maxAngleIndex = j;
                }
            }

            if (maxAngleIndex == this.pointIndexList[0])
            {
                //一周したので終了
                break;
            }
            else
            {
                this.pointIndexList.Add(maxAngleIndex);
            }
        }

        //ポリゴンコライダの頂点を決定
        this.polygonCollider.points = this.pointIndexList.Select(i => this.canvasPoints[i]).ToArray();

#if FISHCOLLIDER2D_DEBUG
        //OnPopulateMeshを呼んでコライダイメージ更新する
        this.SetVerticesDirty();
#endif
    }

    /// <summary>
    /// 座標更新
    /// </summary>
    public void UpdatePosition()
    {
        //BoxColliderのCenterをキャンバス位置に変換し、自身の座標とする
        this.rectTransform.anchoredPosition = BattleGlobal.instance.WorldToCanvasPoint(this.boxColliderPoints.cachedTransform.position);
    }

    /// <summary>
    /// コライダイメージのデバッグ表示
    /// </summary>
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
#if FISHCOLLIDER2D_DEBUG
        //三角形の数は頂点数-2
        int triangleCount = this.polygonCollider.points.Length - 2;

        //三角形を構成する頂点インデックスを設定
        for (int i = 1; i <= triangleCount; i++)
        {
            vh.AddTriangle(i, i + 1, 0);
        }

        //メッシュの頂点を設定
        for (int i = 0; i < this.polygonCollider.points.Length; i++)
        {
            var v = UIVertex.simpleVert;
            v.position = this.polygonCollider.points[i];
            v.color = this.color;
            vh.AddVert(v);
        }
#endif
    }

}//class FishCollider2D

}//namespace Battle