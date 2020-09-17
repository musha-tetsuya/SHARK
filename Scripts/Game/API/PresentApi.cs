using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// プレゼントBox関連API.
/// </summary>
public class PresentApi
{
    /// <summary>
    /// プレゼントBoxの種類
    /// </summary>
    public enum PresentBoxType
    {
        NonLimited = 1, //無期限
        Limited = 2     //期限有り
    }

    /// <summary>
    /// present/listのレスポンスデータ
    /// </summary>
    public class ListResponseData
    {
        public uint tPresentBoxCount;
        public uint tPresentBoxLimitedCount;
        public List<TPresentBox> tPresentBox;
        public List<TPresentBoxLimited> tPresentBoxLimited;
        public List<TPresentBoxReceived> tPresentBoxReceived;
    }

    /// <summary>
    /// present/receiveListのレスポンスデータ
    /// </summary>
    public class ReceiveListResponseData
    {
        public uint[] presentBoxNotFound;
        public uint[] presentBoxClosed;
        public TPresentBoxLimited[] wakuFull;
        public TPresentBoxReceived[] tPresentBoxReceived;
        public TUsers tUsers;
        public UserGemData tGem;
        public bool isMaxPossession;
    }

    /// <summary>
    /// プレゼントBox内のアイテムと受け取り履歴を取得する通信
    /// </summary>
    public static void CallListApi(Action<ListResponseData> onCompleted)
    {
        //リクエスト作成
        var request = new SharkWebRequest<ListResponseData>("present/list");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        //通信完了時コールバック登録
        request.onSuccess = (response) =>
        {
            HomeScene.isMaxPossession |= response.tPresentBoxCount + response.tPresentBoxLimitedCount > Masters.ConfigDB.Get().maxPresentBox;

            onCompleted?.Invoke(response);
        };

        //通信開始
        request.Send();
    }

    /// <summary>
    /// 受け取り通信
    /// </summary>
    public static void CallReceiveApi(TPresentBox[] dataList, Action<ReceiveListResponseData> onCompleted, Action<int> onError)
    {
        var presentBoxType = (dataList[0] is TPresentBoxLimited) ? PresentBoxType.Limited : PresentBoxType.NonLimited;
        var presentBoxIdList = dataList.Select(x => x.id).ToArray();

        //リクエスト作成
        var request = new SharkWebRequest<ReceiveListResponseData>("present/receiveList");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        //リクエストパラメータセット
        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "presentBoxType", presentBoxType },
            { "presentBoxIdList",   presentBoxIdList },
        });

        //通信完了時コールバック登録
        request.onSuccess = (response) =>
        {
            if (response.tPresentBoxReceived != null)
            {
                //アイテム付与
                foreach (var item in response.tPresentBoxReceived)
                {
                    UserData.Get().AddItem((ItemType)item.itemType, item.itemId, item.itemNum);
                }
            }

            //ユーザー情報同期
            if (response.tUsers != null)
            {
                UserData.Get().Set(response.tUsers);
            }

            //ジェム同期
            if (response.tGem != null)
            {
                UserData.Get().Set(response.tGem);
            }

            //ヘッダー更新
            SharedUI.Instance.header.SetInfo(UserData.Get());

            HomeScene.isMaxPossession = response.isMaxPossession;

            //通信完了
            onCompleted?.Invoke(response);
        };

        //エラー時の対応を追加
        request.onError = onError;

        //通信開始
        request.Send();
    }
}
