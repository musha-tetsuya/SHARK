using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VipApi
{
    /// <summary>
    /// tVipRewardデータ
    /// </summary>
    public class TVipReward
    {
        public uint vipLevel;
        public uint receiveFlg;
    }
    /// <summary>
    /// vip/vipLevelのレスポンスデータ
    /// </summary>
    public class VipLevelCheckResponseData
    {
        public TUsers tUsers;
        public TVipReward[] tVipReward;
    }
    /// <summary>
    /// vip/rewardGetのレスポンスデータ
    /// </summary>
    public class VipRewardGetResponseData
    {
        public TUsers tUsers;
        public UserGemData tGem;
        public AddItem[] mVipReward;
    }

    /// <summary>
    /// VIPレベル、補償チェック取得通信
    /// </summary>
    public static void CallVipLevelApi(Action<VipLevelCheckResponseData> onCompleted)
    {
        var request = new SharkWebRequest<VipLevelCheckResponseData>("vip/vipLevel");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.onSuccess = (response) =>
        {
            onCompleted?.Invoke(response);
        };

        request.Send();
    }

    /// <summary>
    /// 補償取得通信
    /// </summary>
    public static void CallVipRewardGetApi(uint vipLevel, Action onCompleted)
    {
        var request = new SharkWebRequest<VipRewardGetResponseData>("vip/rewardGet");
        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "vipLevel", vipLevel}
        });

        request.onSuccess = (response) =>
        {
            UserData userData = UserData.Get();
            
            // アイテム更新
            foreach (var add in response.mVipReward)
            {
                userData.AddItem((ItemType)add.itemType, add.itemId, add.itemNum);
            }

            //ジェムとコインを反映
            if (response.tGem != null)
            {
                userData.Set(response.tGem);
            }
            if (response.tUsers != null)
            {
                userData.Set(response.tUsers);
                SharedUI.Instance.header.SetInfo(userData);
            }

            onCompleted?.Invoke();
        };
        request.Send();
    }
}