using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// マルチワールドパネル
/// </summary>
public class MultiWorldPanel : MonoBehaviour
{
    /// <summary>
    /// メインイメージ
    /// </summary>
    [SerializeField]
    private Image mainImage = null;
    /// <summary>
    /// ComingSoonイメージ
    /// </summary>
    [SerializeField]
    private Image comingSoonImage = null;
    /// <summary>
    /// グレーアウト対象のグラフィック
    /// </summary>
    [SerializeField]
    private Graphic[] grayoutTarget = null;

    [SerializeField]
    private Text jackpotText = null;

    [SerializeField]
    private Image jackpotFrame = null;

    /// <summary>
    /// ワールドデータ
    /// TODO. シングルのデータを使おうか？
    /// </summary>
    public MultiStageSelectScene.WorldData worldData { get; private set; }

    /// <summary>
    /// セット
    /// </summary>
    public void Set(MultiStageSelectScene.WorldData worldData)
    {
        this.worldData = worldData;
        
        // isComingSoonの場合
        if (this.worldData.worldMasterData.isComingSoon > 0)
        {
            this.SetGrayout(true);

            // メインイメージ
            this.mainImage.gameObject.SetActive(false);
            // comingSoonイメージ
            this.comingSoonImage.gameObject.SetActive(true);
            return;
        }

        // isComingSoonで、ない場合
        // メインイメージ
        this.mainImage.gameObject.SetActive(true);
        // comingSoonイメージ
        this.comingSoonImage.gameObject.SetActive(false);
        // メインイメージ切り替え
        this.mainImage.sprite = this.worldData.bgAssetLoader.handle.asset as Sprite;
        // ジャックポッドCoinパンネル
        if (this.worldData.worldMasterData.id == this.worldData.worldJackpotInfoData.worldId)
        {
            long jackpotPoint = this.worldData.worldJackpotInfoData.jackpotPoint;
            string frameSpriteName = jackpotPoint < 100000000  ? "SsMulti_010_0001"
                                   : jackpotPoint < 1000000000 ? "SsMulti_010_0002"
                                   :                             "SsMulti_010_0003";
            this.jackpotText.text = jackpotPoint.ToString("#,0");
            this.jackpotFrame.sprite = SceneChanger.currentScene.sceneAtlas.GetSprite(frameSpriteName);
        }

        // ワールド、ON/OFF
        if (this.worldData.worldServerData.IsOpen())
        {
            this.SetGrayout(false);
        }
        else
        {
            this.SetGrayout(true);
        }
    }

    /// <summary>
    /// グレーアウトON/OFF
    /// </summary>
    private void SetGrayout(bool isGrayout)
    {
        foreach (var graphic in this.grayoutTarget)
        {
            graphic.material = isGrayout ? SharedUI.Instance.grayScaleMaterial : null;
        }
    }
}