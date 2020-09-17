using System;

/// <summary>
/// Home系API
/// </summary>
public class HomeApi
{
    /// <summary>
    /// 普通ログインボーナス取得のid
    /// </summary>
    public class TLoginBonus
    {
        public uint loginBonusId;
    }
    /// <summary>
    /// スペシャルログインボーナス取得のid
    /// </summary>
    public class TLoginBonusSpecial
    {
        public uint loginBonusId;
    }

    /// <summary>
    /// home/homeDataのレスポンス
    /// </summary>
    public class HomeDataResponse
    {
        public bool loginBonusChk;
        public bool specialLoginBonusChk;
        public TLoginBonus tLoginBonus;
        public TLoginBonusSpecial tLoginBonusSpecial;
        public uint tPresentBoxCount;//無期限品の数
        public uint tPresentBoxLimitedCount;//有期限品の数
        public bool isMaxPossession;
        public uint freeRemoveCount;
    }

    /// <summary>
    /// Home情報取得通信
    /// </summary>
    public static void CallHomeDataApi(Action<HomeDataResponse> onCompleted)
    {
        var request = new SharkWebRequest<HomeDataResponse>("home/homeData");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.onSuccess = (response) =>
        {
            //無期限BOXの上限超過してるかどうか
            HomeScene.isMaxPossession = response.isMaxPossession;

            if (response.loginBonusChk)
            {
                //通常ログボのアイテム付与
                var master = Masters.LoginBonusDB.FindById(response.tLoginBonus.loginBonusId);
                UserData.Get().AddItem((ItemType)master.itemType, master.itemId, master.itemNum);
            }

            if (response.specialLoginBonusChk)
            {
                //スペシャルログボのアイテム付与
                var master = Masters.LoginBonusSpecialDB.FindById(response.tLoginBonusSpecial.loginBonusId);
                UserData.Get().AddItem((ItemType)master.itemType, master.itemId, master.itemNum);
            }

            //ヘッダー更新
            SharedUI.Instance.header.SetInfo(UserData.Get());
            // 無理、ギア外すカウンター取得
            CustomGearConfirmDialogContent.freeGearRemoveCount = response.freeRemoveCount;

            onCompleted?.Invoke(response);
        };

        request.Send();
    }
}