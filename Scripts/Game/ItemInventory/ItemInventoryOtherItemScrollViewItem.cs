using UnityEngine;
using System;

public class ItemInventoryOtherItemScrollViewItem : MonoBehaviour
{
    /// <summary>
    /// CommonIcon
    /// </summary>
    [SerializeField]
    private CommonIcon commonIcon = null;

    /// <summary>
    /// ユーザーギアデータ
    /// </summary>
    public UserItemData itemData { get; private set; }

    /// <summary>
    /// 表示構築
    /// </summary>
    public void Set(UserItemData data, Action<ItemInventoryOtherItemScrollViewItem> onClick)
    {
        this.itemData = data;
        this.commonIcon.rankImage.gameObject.SetActive(false);
        this.commonIcon.rankBgImage.gameObject.SetActive(false);

        // アイコンスプライト切替
        string key = CommonIconUtility.GetSpriteKey((uint)ItemType.BattleItem, this.itemData.itemId);
        string path = CommonIconUtility.GetSpritePath((uint)ItemType.BattleItem, key);
        var handle = AssetManager.FindHandle<Sprite>(path);
        this.commonIcon.SetIconSprite(handle.asset as Sprite);

        this.commonIcon.SetCountText(data.stockCount);
        
        // クリック時処理登録
        this.commonIcon.onClick = () => onClick(this);
    }
}
