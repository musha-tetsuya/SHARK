using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInventoryOtherDialogContent : MonoBehaviour
{
    /// <summary>
    /// その他CommonIcon
    /// </summary>
    [SerializeField]
    private CommonIcon otherItemCommonIcon = null;
    /// <summary>
    /// アイテム名テキスト
    /// </summary>
    [SerializeField]
    private Text otherItemNameText = null;
    /// <summary>
    /// アイテム説明テキスト
    /// </summary>
    [SerializeField]
    private Text otherItemDescriptionText = null;

    public UserItemData itemData { get; private set; }

    public void SetOtherItemData(UserItemData data)
    {   
        this.itemData = data;
        
        string key = CommonIconUtility.GetSpriteKey((uint)data.itemType, data.itemId);
        string path = CommonIconUtility.GetSpritePath((uint)data.itemType, key);
        
        var handle = AssetManager.FindHandle<Sprite>(path);
        this.otherItemCommonIcon.SetIconSprite(handle.asset as Sprite);

        this.otherItemCommonIcon.SetCountText(this.itemData.stockCount);
        this.otherItemCommonIcon.countText.text = null;

        this.otherItemNameText.text = CommonIconUtility.GetName((uint)data.itemType, data.itemId);
        this.otherItemDescriptionText.text = CommonIconUtility.GetDescription((uint)data.itemType, data.itemId);
    }
    
}
