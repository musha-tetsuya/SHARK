using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

/// <summary>
/// シングルワールドパネル
/// </summary>
public class SingleWorldPanel : MonoBehaviour
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

    /// <summary>
    /// ワールドデータ
    /// </summary>
    public SingleStageSelectScene.WorldData worldData { get; private set; }

    /// <summary>
    /// セット
    /// </summary>
    public void Set(SingleStageSelectScene.WorldData worldData)
    {
        this.worldData = worldData;

        if (this.worldData.worldMasterData.isComingSoon > 0)
        {
            this.SetGrayout(true);

            //メインイメージ切り替え
            this.mainImage.gameObject.SetActive(false);
            this.comingSoonImage.gameObject.SetActive(true);

            return;
        }

        //メインイメージ切り替え
        this.mainImage.gameObject.SetActive(true);
        this.comingSoonImage.gameObject.SetActive(false);
        this.mainImage.sprite = this.worldData.bgAssetLoader.handle.asset as Sprite;

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
