using System;
using System.Collections.Generic;
using System.Linq;
using Master;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// ショップ関連API.
/// </summary>
public class ShopApi
{
    /// <summary>
    /// shop/buyのレスポンスデータ
    /// </summary>
    public class BuyResponseData
    {
        public UserBatteryPartsData[] tCannonBattery;
        public UserBarrelPartsData[] tCannonBarrel;
        public UserBulletPartsData[] tCannonBullet;
        public UserGearData[] tGear;
        public UserItemData[] tItem;
        public UserShopData tShop;
        public TUsers tUsers;
        public UserGemData tGem;
        public bool isMaxPossession;
    }

    /// <summary>
    /// shop/buyStatusのレスポンスデータ
    /// </summary>
    public class BuyStatusResponseData
    {
        public List<UserShopData> tShop;
    }

    /// <summary>
    /// shop/nowShopのレスポンスデータ
    /// </summary>
    public class NowShopResponseData
    {
        public object mShop;
        public object mShopGroup;
        public object[][] mShopItem;
        public List<UserShopData> tShop;
    }

    /// <summary>
    /// 商品購入通信
    /// </summary>
    public static void CallBuyApi(uint shopId, uint buyNum, Action<UserShopData> onCompleted)
    {
#if SHARK_OFFLINE
        //オフライン時の仮対応
        UserData user = UserData.Get();
        if (user != null)
        {
            //購入による消費関連の設定
            ShopData shopData = Masters.ShopDB.FindById(shopId);
            ulong totalCost = 0;
            if (shopData.needChargeGem > 0)
            {
                totalCost = shopData.needChargeGem * buyNum;
                user.chargeGem -= totalCost;
            }
            else if(shopData.needFreeGem > 0)
            {
                totalCost = shopData.needFreeGem * buyNum;
                if (user.freeGem < totalCost)
                {
                    user.chargeGem -= totalCost - user.freeGem;
                    user.freeGem = 0;
                }
                else
                {
                    user.freeGem -= totalCost;
                }
            }
            else if (shopData.needCoin > 0)
            {
                totalCost = shopData.needCoin * buyNum;
                user.coin -= totalCost;
            }

            //購入によるアイテム付与
            List<ShopItemData> shopItemDatas = Masters.ShopItemDB.GetList().FindAll(x => x.shopItemId == shopData.shopItemId);
            foreach (ShopItemData shopItem in shopItemDatas)
            {
                //ひとまずはコインとジェムのみ
                switch ((ItemType)shopItem.itemType)
                {
                    case ItemType.ChargeGem:
                        user.chargeGem += shopItem.itemNum * buyNum;
                        break;
                    case ItemType.FreeGem:
                        user.freeGem += shopItem.itemNum * buyNum;
                        break;
                    case ItemType.Coin:
                        user.coin += shopItem.itemNum * buyNum;
                        break;
                }
            }

            //購入による履歴の設定
            UserShopData userShopData = new UserShopData();
            userShopData.shopId = shopData.id;
            userShopData.buyNum = buyNum;

            onCompleted?.Invoke(userShopData);
            return;
        }
#endif

        //リクエスト作成
        var request = new SharkWebRequest<BuyResponseData>("shop/buy");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        //リクエストパラメータセット
        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "shopId", shopId },
            { "buyNum", buyNum },
        });

        //通信完了時コールバック登録
        request.onSuccess = (response) =>
        {
            OnSuccessBuyApi(response, onCompleted);
        };

        //通信開始
        request.Send();
    }

    /// <summary>
    /// 購入通信成功時
    /// </summary>
    public static void OnSuccessBuyApi(BuyResponseData response, Action<UserShopData> onCompleted)
    {
        //購入後のユーザーデータの更新
        UserData userData = UserData.Get();
        userData.Set(response.tUsers);
        userData.Set(response.tGem);

        //入手したアイテムの情報を更新
        if (response.tCannonBattery != null)
        {
            for (int i = 0; i < response.tCannonBattery.Length; ++i)
            {
                userData.AddItem(ItemType.Battery, response.tCannonBattery[i].itemId, 1);
            }
        }
        if (response.tCannonBarrel != null)
        {
            for (int i = 0; i < response.tCannonBarrel.Length; ++i)
            {
                userData.AddItem(ItemType.Barrel, response.tCannonBarrel[i].itemId, 1);
            }
        }
        if (response.tCannonBullet != null)
        {
            for (int i = 0; i < response.tCannonBullet.Length; ++i)
            {
                userData.AddItem(ItemType.Bullet, response.tCannonBullet[i].itemId, 1);
            }
        }
        if (response.tGear != null)
        {
            for (int i = 0; i < response.tGear.Length; ++i)
            {
                userData.AddItem(ItemType.Gear, response.tGear[i].gearId, 1);
            }
        }
        if (response.tItem != null)
        {
            for (int i = 0; i < response.tItem.Length; i++)
            {
                userData.SetItem(response.tItem[i]);
            }
        }

        HomeScene.isMaxPossession = response.isMaxPossession;

        //通信完了
        onCompleted?.Invoke(response.tShop);
    }

    /// <summary>
    /// 購入履歴の取得
    /// </summary>
    public static void CallBuyStatusApi(uint shopId, Action<List<UserShopData>> onCompleted)
    {
        //リクエスト作成
        var request = new SharkWebRequest<BuyStatusResponseData>("shop/buyStatus");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        //リクエストパラメータセット
        if (shopId > 0) {
            //shopIdの指定がない場合は全件取得
            request.SetRequestParameter(new Dictionary<string, object>
            {
                { "shopId", shopId }
            });
        }

        //通信完了時コールバック登録
        request.onSuccess = (response) =>
        {
            //通信完了
            onCompleted?.Invoke(response.tShop);
        };

        //通信開始
        request.Send();
    }

    /// <summary>
    /// ショップ関連のデータ取得通信
    /// </summary>
    public static void CallNowShopApi(Action<List<UserShopData>> onCompleted)
    {
#if SHARK_OFFLINE
        CoroutineUpdator.Create(null, () => onCompleted?.Invoke(new List<UserShopData>()));
        return;
#endif
        //リクエスト作成
        var request = new SharkWebRequest<NowShopResponseData>("shop/nowShop");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        //通信完了時コールバック登録
        request.onSuccess = (response) =>
        {
            var jArray = new JArray(response.mShopItem.SelectMany(x => x).ToArray());
            Masters.ShopDB.SetList(response.mShop.ToString());
            Masters.ShopGroupDB.SetList(response.mShopGroup.ToString());
            Masters.ShopItemDB.SetList(jArray.ToString());

            //通信完了
            onCompleted?.Invoke(response.tShop);
        };

        //通信開始
        request.Send();
    }
}
