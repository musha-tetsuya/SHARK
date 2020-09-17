using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

/// <summary>
/// シングルモードステージパネル
/// </summary>
public class SingleStagePanel : MonoBehaviour
{
    /// <summary>
    /// ボタン
    /// </summary>
    [SerializeField]
    private Button button = null;
    /// <summary>
    /// バトルベース
    /// </summary>
    [SerializeField]
    private Image battleBase = null;
    /// <summary>
    /// ストーリーベース
    /// </summary>
    [SerializeField]
    private Image storyBase = null;
    /// <summary>
    /// ステージ番号テキスト
    /// </summary>
    [SerializeField]
    private Text stageNoText = null;
    /// <summary>
    /// 魚イメージ
    /// </summary>
    [SerializeField]
    private Image fishImage = null;
    /// <summary>
    /// ロックイメージ
    /// </summary>
    [SerializeField]
    private Image lockImage = null;
    /// <summary>
    /// 星マークグループ
    /// </summary>
    [SerializeField]
    private GameObject starMarkGroup = null;
    /// <summary>
    /// 星マーク 
    /// </summary>
    [SerializeField]
    private Image[] starMarks = null;
    /// <summary>
    /// ライン
    /// </summary>
    [SerializeField]
    private GameObject line = null;
    [SerializeField]
    private Graphic[] grayScaleTargets = null;

    /// <summary>
    /// ステージマスターデータ
    /// </summary>
    public Master.SingleStageData master { get; private set; }
    /// <summary>
    /// ステージサーバーデータ
    /// </summary>
    public SinglePlayApi.TSingleStage server { get; private set; }
    /// <summary>
    /// ステージが解放済みかどうか
    /// </summary>
    public bool isOpen => this.server.IsOpen();
    /// <summary>
    /// バトルステージかどうか
    /// </summary>
    public bool isBattle => (this.master.type == (int)Master.SingleStageData.StageType.Battle);
    /// <summary>
    /// ストーリーステージかどうか
    /// </summary>
    public bool isStory => (this.master.type == (int)Master.SingleStageData.StageType.Story);
    /// <summary>
    /// クリック時コールバック
    /// </summary>
    private Action<SingleStagePanel> onClick = null;

    /// <summary>
    /// 内容構築
    /// </summary>
    public void Set(
        Master.SingleStageData master,
        SinglePlayApi.TSingleStage server,
        int battleStageIndex,
        bool isLastStage,
        Action<SingleStagePanel> onClick)
    {
        this.master = master;
        this.server = server;
        this.onClick = onClick;

        //解放済みかどうかでボタンとして押せるか押せないか切り替え
        this.button.interactable = this.isOpen;

        //状態による表示切替
        this.battleBase.gameObject.SetActive(this.isBattle);
        this.storyBase.gameObject.SetActive(this.isStory);
        this.lockImage.enabled = !this.isOpen;
        this.fishImage.enabled = this.isOpen;
        this.starMarkGroup.SetActive(this.isOpen);

        //クリアランクでの星数表示
        if (this.starMarkGroup.activeInHierarchy)
        {
            for (int i = 0; i < this.starMarks.Length; i++)
            {
                this.starMarks[i].enabled = i < (int)this.server.clearRank - 2;
            }
        }

        //ロックの場合グレースケール表示
        foreach (var graphic in this.grayScaleTargets)
        {
            graphic.material = this.isOpen ? null : SharedUI.Instance.grayScaleMaterial;
        }

        //最終ステージ以外はラインを表示
        this.line.SetActive(!isLastStage);

        //バトルステージ番号
        this.stageNoText.text = (battleStageIndex < 0) ? null : (battleStageIndex + 1).ToString();

        if (this.isBattle && this.isOpen)
        {
            //バトルステージの魚アイコン切り替え
            this.fishImage.sprite = AssetManager.FindHandle<Sprite>(SharkDefine.GetSingleStageIconSpritePath(this.master.key)).asset as Sprite;
        }
    }

    /// <summary>
    /// クリック時
    /// </summary>
    public void OnClick()
    {
        this.onClick?.Invoke(this);
    }
}
