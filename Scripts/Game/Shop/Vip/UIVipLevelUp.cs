using UnityEngine;
using UnityEngine.UI;

public class UIVipLevelUp : DialogBase
{
    /// <summary>
    /// 現在のVipLevel
    /// </summary>
    [SerializeField]
    private Text currentVipLevel = null;
    /// <summary>
    /// ジェム購入前 VipLevel
    /// </summary>
    public static uint beforeVipLevel = 0;

    /// <summary>
    /// ポップアップ判断
    /// </summary>
    public static void OpenIfNeed(UIVipLevelUp prefab)
    {
        if (beforeVipLevel != UserData.Get().vipLevel)
        {
            beforeVipLevel = UserData.Get().vipLevel;

            var popup = SharedUI.Instance.ShowPopup(prefab);
            popup.Set(UserData.Get().vipLevel);
        }
    }

    /// <summary>
    /// Set
    /// </summary>
    private void Set(uint currentVipLevel)
    {
        // vipレベル表示
        this.currentVipLevel.text = currentVipLevel.ToString();
    }

    protected override void Start()
    {
        //なにもしない
    }

    public override void Close()
    {
        Destroy(this.gameObject);
    }
}
