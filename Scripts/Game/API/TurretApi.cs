using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 砲台系API
/// </summary>
public class TurretApi
{
    /// <summary>
    /// パーツロックResponseDataのlockFlg
    /// </summary>
    public class TCannon
    {
        public uint lockFlg;
        public uint id;
    }

    /// <summary>
    /// cannon/cannonLockのレスポンスデータ
    /// </summary>
    public class CannonLockResponseData
    {
        public TCannon tCannon;
    }

    /// <summary>
    /// cannon/cannonLockListのレスポンスデータ
    /// </summary>
    public class CannonCollectLockResponseData
    {
        public TCannon[] tCannon;
    }

    /// <summary>
    /// cannon/gearLockのレスポンスデータ
    /// </summary>
    public class GearLockResponseData
    {
        public UserGearData tGear;
    }

    /// <summary>
    /// cannon/gearLockListのレスポンスデータ
    /// </summary>
    public class GearCollectLockResponseData
    {
        public UserGearData[] tGear;
    }

    /// <summary>
    /// cannon/setのレスポンスデータ
    /// </summary>
    public class CannonSetResponseData
    {
        public UserTurretData tCannonSetting;
    }

    /// <summary>
    /// cannon/useのレスポンスデータ
    /// </summary>
    public class CannonUseResponseData
    {
        public UserTurretData tCannonSetting;
    }

    /// <summary>
    /// cannon/gearSetのレスポンスデータ
    /// </summary>
    public class CannonGearSetResponseData
    {
        public UserGearData tGear;
    }

    /// <summary>
    /// cannon/gearUnsetのレスポンスデータ
    /// </summary>
    public class CannonGearUnsetResponseData
    {
        public TUsers tUsers;
        public UserGearData tGear;
        public uint freeRemoveCount;
    }

    /// <summary>
    /// TODO
    /// cannon/gearUnsetのレスポンスデータ
    /// </summary>
    public class CannonaddResponseData
    {
        public UserTurretData tCannonSetting;
    }

    /// <summary>
    /// cannon/addSlotのレスポンスデータ
    /// </summary>
    public class GearAddSlotResponseData
    {
        public UserTurretData userCannonData;
    }

    /// <summary>
    /// cannon/sellCannonのResponseData
    /// </summary>
    public class CannonDecompositionResponseData
    {
        public TUsers tUsers;
        public AddItem[] mItemSell;
        public UserGearData[] tGear;
        public TCannon tCannon;
    }

    /// <summary>
    /// cannon/sellCannonListのResponseData
    /// </summary>
    public class CannonDecompositionListResponseData
    {
        public TUsers tUsers;
        public AddItem[] mItemSell;
        public TCannon[] tCannon;
        public UserGearData[] tGear;
    }

    /// <summary>
    /// cannon/sellGearのResponseData
    /// </summary>
    public class GearDecompositionResponseData
    {
        public TUsers tUsers;
        public AddItem[] mItemSell;
        public UserGearData tGear;
    }

    /// <summary>
    /// cannon/sellGearListのResponseData
    /// </summary>
    public class GearDecompositionListResponseData
    {
        public TUsers tUsers;
        public AddItem[] mItemSell;
        public UserGearData[] tGear;
    }

    /// <summary>
    /// 所持砲台、パーツ情報を更新する必要があるかどうか
    /// </summary>
    public static bool isNeedRefleshCannonUser = false;

    /// <summary>
    /// 所持砲台、パーツ情報の取得通信
    /// </summary>
    public static void CallUserApi(Action onCompleted)
    {
        if (!isNeedRefleshCannonUser)
        {
            onCompleted?.Invoke();
            return;
        }
#if SHARK_OFFLINE
        var request = new SharkWebRequest<FirstApi.FirstUserResponseData>("first/user");
#else
        var request = new SharkWebRequest<FirstApi.FirstUserResponseData>("cannon/user");
#endif

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.onSuccess = (response) =>
        {
            UserData.Get().SetTurretData(
                response.tCannonSetting,
                response.tCannonBattery,
                response.tCannonBarrel,
                response.tCannonBullet,
                response.tCannonAccessories,
                response.tGear
            );

            isNeedRefleshCannonUser = false;

            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// 砲台のセット通信
    /// </summary>
    public static void CallSetApi(
        uint settingNumber,
        uint batteryServerId,
        uint barrelServerId,
        uint bulletServerId,
        uint accessoryServerId,
        Action onCompleted)
    {
#if SHARK_OFFLINE
        {
            onCompleted?.Invoke();
            return;
        }
#endif
        var request = new SharkWebRequest<CannonSetResponseData>("cannon/set");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "settingNumber", settingNumber },
            { "batteryCannonId", batteryServerId },
            { "barrelCannonId", barrelServerId },
            { "bulletCannonId", bulletServerId },
            { "accessoriesCannonId", accessoryServerId },
        });

        request.onSuccess = (response) =>
        {
            var userData = UserData.Get();
            for (int i = 0; i < userData.turretData.Length; i++)
            {
                if (userData.turretData[i].settingNumber == settingNumber)
                {
                    userData.turretData[i] = response.tCannonSetting;
                    break;
                }
            }
            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// 使用する砲台の変更通信
    /// </summary>
    public static void CallUseApi(uint settingNumber, Action onCompleted)
    {
#if SHARK_OFFLINE
        {
            var userData = UserData.Get();
            var selectedTurretData = userData.GetSelectedTurretData();
            if (selectedTurretData != null)
            {
                selectedTurretData.useFlg = 0;
            }
            userData.turretData.First(x => x.settingNumber == settingNumber).useFlg = 1;
            onCompleted?.Invoke();
            return;
        }
#endif
        var request = new SharkWebRequest<CannonUseResponseData>("cannon/use");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "settingNumber",  settingNumber }
        });

        request.onSuccess = (response) =>
        {
            var userData = UserData.Get();
            for (int i = 0; i < userData.turretData.Length; i++)
            {
                if (userData.turretData[i].settingNumber == settingNumber)
                {
                    //現在の選択状態を解除
                    var selectedTurretData = userData.GetSelectedTurretData();
                    if (selectedTurretData != null)
                    {
                        selectedTurretData.useFlg = 0;
                    }
                    //レスポンスで更新
                    userData.turretData[i] = response.tCannonSetting;
                    break;
                }
            }
            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// パーツへのギア装着通信
    /// </summary>
    public static void CallGearSetApi(uint gearServerId, uint partsServerId, Action onCompleted)
    {
        //対象ギアの取得
        UserGearData userGear = UserData.Get().gearData.First(gear => gear.serverId == gearServerId);

#if SHARK_OFFLINE
        userGear.SetPartsServerId(partsServerId, DateTime.Now);
        onCompleted?.Invoke();
        return;
#endif
        var request = new SharkWebRequest<CannonGearSetResponseData>("cannon/gearSet");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "gearId", gearServerId },
            { "cannonId", partsServerId },
        });

        request.onSuccess = (response) =>
        {
            //ギア装着の反映
            userGear.SetPartsServerId(partsServerId, response.tGear.setDateTime);

            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// パーツからギアの取り外し通信
    /// </summary>
    public static void CallGearUnsetApi(uint gearServerId, Action onCompleted)
    {
        //対象ギアの取得
        UserGearData userGear = UserData.Get().gearData.First(gear => gear.serverId == gearServerId);

#if SHARK_OFFLINE
        userGear.SetPartsServerId(null, DateTime.Now);
        onCompleted?.Invoke();
        return;
#endif
        var request = new SharkWebRequest<CannonGearUnsetResponseData>("cannon/gearUnset");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "gearId", gearServerId },
        });

        request.onSuccess = (response) =>
        {
            //コイン消費の反映
            UserData.Get().Set(response.tUsers);
            SharedUI.Instance.header.SetInfo(UserData.Get());

            //ギア取り外しの反映
            userGear.SetPartsServerId(null, response.tGear.setDateTime);
            // 無理、ギア外すカウンター更新
            CustomGearConfirmDialogContent.freeGearRemoveCount = response.freeRemoveCount;

            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// 砲台追加 TODO. サーバ側で、APIを準備するまで
    /// </summary>
    public static void CallAddApi(
        uint settingNumber,
        uint batteryServerId,
        uint barrelServerId,
        uint bulletServerId,
        uint accessoryServerId,
        Action onCompleted)
    {
#if SHARK_OFFLINE
        {
            onCompleted?.Invoke();
            return;
        }
#endif
        var request = new SharkWebRequest<CannonSetResponseData>("cannon/set");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "settingNumber", settingNumber },
            { "batteryCannonId", batteryServerId },
            { "barrelCannonId", barrelServerId },
            { "bulletCannonId", bulletServerId },
            { "accessoriesCannonId", accessoryServerId },
        });

        request.onSuccess = (response) =>
        {
            var userData = UserData.Get();
            List<UserTurretData> turretDataList = userData.turretData.ToList();

            turretDataList.Add(response.tCannonSetting);

            userData.turretData = turretDataList.ToArray();
            
            onCompleted?.Invoke();
        };

        request.Send();
    }
    
    /// <summary>
    /// ギアスロット解除
    /// </summary>
    public static void CallGearAddSlotApi(
        uint partsServerId,
        uint partsType,
        Action onCompleted)
    {
        var request = new SharkWebRequest<GearAddSlotResponseData>("cannon/addSlot");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "cannonId", partsServerId },
            { "cannonType", partsType },
        });

        request.onSuccess = (response) =>
        {
            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// パーツロック情報の通信
    /// </summary>
    public static void CallCannonLock(UserPartsData partsData, Action onCompleted)
    {
        var request = new SharkWebRequest<CannonLockResponseData>("cannon/cannonLock");
        request.SetRequestHeader("AccessToken", UserData.Get().hash);
        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "cannonId", partsData.serverId },
            { "cannonType", partsData.itemType }
        });
        
        request.onSuccess = (response) =>
        {
            partsData.lockFlg = response.tCannon.lockFlg;
            onCompleted?.Invoke();
        };
        
        request.Send();
    }

    /// <summary>
    /// パーツまとめて、ロック情報の通信
    /// </summary>
    public static void CallCannonLockList(uint cannonType, List<uint> cannonId, Action onCompleted)
    {
        var request = new SharkWebRequest<CannonCollectLockResponseData>("cannon/cannonLockList");
        request.SetRequestHeader("AccessToken", UserData.Get().hash);
        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "cannonType", cannonType },
            { "cannonId", cannonId }
        });
        
        request.onSuccess = (response) =>
        {
            var userData = UserData.Get();

            if (cannonType == (uint)ItemType.Battery)
            {
                for (int i = 0; i < userData.batteryData.Length; i++)
                {
                    var id = response.tCannon
                    .Where(x => x.id == userData.batteryData[i].serverId)
                    .Select(x => x.id)
                    .SingleOrDefault();

                    if (id > 0)
                    {
                        var lockFlgResponse = response.tCannon
                        .Where(x => x.id == id)
                        .Select(x => x.lockFlg)
                        .SingleOrDefault();

                        userData.batteryData[i].lockFlg = lockFlgResponse;
                    }
                }
            }
            else if (cannonType == (uint)ItemType.Barrel)
            {
                for (int i = 0; i < userData.barrelData.Length; i++)
                {
                    var id = response.tCannon
                    .Where(x => x.id == userData.barrelData[i].serverId)
                    .Select(x => x.id)
                    .SingleOrDefault();

                    if (id > 0)
                    {
                        var lockFlgResponse = response.tCannon
                        .Where(x => x.id == id)
                        .Select(x => x.lockFlg)
                        .SingleOrDefault();

                        userData.barrelData[i].lockFlg = lockFlgResponse;
                    }
                }
            }
            else if (cannonType == (uint)ItemType.Bullet)
            {
                for (int i = 0; i < userData.bulletData.Length; i++)
                {
                    var id = response.tCannon
                    .Where(x => x.id == userData.bulletData[i].serverId)
                    .Select(x => x.id)
                    .SingleOrDefault();

                    if (id > 0)
                    {
                        var lockFlgResponse = response.tCannon
                        .Where(x => x.id == id)
                        .Select(x => x.lockFlg)
                        .SingleOrDefault();

                        userData.bulletData[i].lockFlg = lockFlgResponse;
                    }
                }
            }
            else if (cannonType == (uint)ItemType.Accessory)
            {
                for (int i = 0; i < userData.accessoriesData.Length; i++)
                {
                    var id = response.tCannon
                    .Where(x => x.id == userData.accessoriesData[i].serverId)
                    .Select(x => x.id)
                    .SingleOrDefault();

                    if (id > 0)
                    {
                        var lockFlgResponse = response.tCannon
                        .Where(x => x.id == id)
                        .Select(x => x.lockFlg)
                        .SingleOrDefault();

                        userData.accessoriesData[i].lockFlg = lockFlgResponse;
                    }
                }
            }

            onCompleted?.Invoke();
        };
        
        request.Send();
    }

    /// <summary>
    /// ギアロック情報の通信
    /// </summary>
    public static void CallGearLock(UserGearData geardata, Action onCompleted)
    {
        var request = new SharkWebRequest<GearLockResponseData>("cannon/gearLock");
        request.SetRequestHeader("AccessToken", UserData.Get().hash);
        request.SetRequestParameter(new Dictionary<string, object>
        {
            {"gearId", geardata.serverId}
        });

        request.onSuccess = (response) =>
        {
            geardata.lockFlg = response.tGear.lockFlg;
            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// ギアまとめて、ロック情報の通信
    /// </summary>
    public static void CallGearLockList(List<uint> gearId, Action onCompleted)
    {
        var request = new SharkWebRequest<GearCollectLockResponseData>("cannon/gearLockList");
        request.SetRequestHeader("AccessToken", UserData.Get().hash);
        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "gearId", gearId }
        });
        
        request.onSuccess = (response) =>
        {
            var gearData = UserData.Get().gearData;

            for (int i = 0; i < gearData.Length; i++)
                {
                    var id = response.tGear
                    .Where(x => x.serverId == gearData[i].serverId)
                    .Select(x => x.serverId)
                    .SingleOrDefault();

                    if (id > 0)
                    {
                        var lockFlgResponse = response.tGear
                        .Where(x => x.serverId == id)
                        .Select(x => x.lockFlg)
                        .SingleOrDefault();

                        gearData[i].lockFlg = lockFlgResponse;
                    }
                }

            onCompleted?.Invoke();
        };
        
        request.Send();
    }

    /// <summary>
    /// パーツ分解情報の通信
    /// </summary>
    public static void CallSellCannon(uint cannonType, uint cannonId, Action onCompleted)
    {
        var request = new SharkWebRequest<CannonDecompositionResponseData>("cannon/sellCannon");
        request.SetRequestHeader("AccessToken", UserData.Get().hash);
        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "cannonType", cannonType },
            { "cannonId", cannonId }
        });

        /// 成功時
        request.onSuccess = (response) =>
        {
            //コイン消費の反映
            UserData.Get().Set(response.tUsers);
            SharedUI.Instance.header.SetInfo(UserData.Get());

            // addItem responseデータ、itemIdとstockCountの順にソート
            var addItemSort = response.mItemSell
            .OrderBy(x => x.itemId)
            .ThenBy(c => c.itemNum)
            .ToArray();

            // アイテム追加
            foreach (var item in addItemSort)
            {
                // BattleItemの場合
                if ((ItemType)item.itemType == ItemType.BattleItem)
                {
                    uint userStockCount = UserData.Get().itemData
                    .Where(x => x.itemId == item.itemId)
                    .Select(x => x.stockCount)
                    .SingleOrDefault();

                    uint addNum = item.itemNum - userStockCount;

                    UserData.Get().AddItem((ItemType)item.itemType, item.itemId, addNum);
                }
            }

            // 分解した、パーツ削除
            if (cannonType == (uint)ItemType.Battery)
            {
                var list = UserData.Get().batteryData.ToList();
                var index = list.FindIndex(x => x.serverId == response.tCannon.id);
                list.RemoveAt(index);
                UserData.Get().batteryData = list.ToArray();
            }
            else if (cannonType == (uint)ItemType.Barrel)
            {
                var list = UserData.Get().barrelData.ToList();
                var index = list.FindIndex(x => x.serverId == response.tCannon.id);
                list.RemoveAt(index);
                UserData.Get().barrelData = list.ToArray();
            }
            else if (cannonType == (uint)ItemType.Bullet)
            {
                var list = UserData.Get().bulletData.ToList();
                var index = list.FindIndex(x => x.serverId == response.tCannon.id);
                list.RemoveAt(index);
                UserData.Get().bulletData = list.ToArray();
            }
            else if (cannonType == (uint)ItemType.Accessory)
            {
                var list = UserData.Get().accessoriesData.ToList();
                var index = list.FindIndex(x => x.serverId == response.tCannon.id);
                list.RemoveAt(index);
                UserData.Get().accessoriesData = list.ToArray();
            }

            // 装着中のギアを削除
            foreach (var item in response.tGear)
            {
                var list = UserData.Get().gearData.ToList();
                var index = list.FindIndex(x => x.serverId == item.serverId);
                list.RemoveAt(index);
                UserData.Get().gearData = list.ToArray();
                Debug.Log("index : " + index);
            }

            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// まとめてパーツ分解情報の通信
    /// </summary>
    public static void CallSellCannonList(uint partType, List<uint> partsList, Action onCompleted)
    {
        var request = new SharkWebRequest<CannonDecompositionListResponseData>("cannon/sellCannonList");
        request.SetRequestHeader("AccessToken", UserData.Get().hash);
        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "cannonType", partType },
            { "cannonId", partsList }
        });

        /// 成功時
        request.onSuccess = (response) =>
        {
            // コイン消費の反映
            UserData.Get().Set(response.tUsers);
            SharedUI.Instance.header.SetInfo(UserData.Get());

            // addItem responseデータ、itemIdとstockCountの順にソート
            var addItemSort = response.mItemSell
            .OrderBy(x => x.itemId)
            .ThenBy(c => c.itemNum)
            .ToArray();

            // アイテム追加
            foreach (var item in addItemSort)
            {
                // BattleItemの場合
                if((ItemType)item.itemType == ItemType.BattleItem)
                {
                    // 現在所有するアイテム数
                    uint userStockCount = UserData.Get().itemData
                    .Where(x => x.itemId == item.itemId)
                    .Select(x => x.stockCount)
                    .SingleOrDefault();

                    // 追加するアイテム数
                    uint addNum = item.itemNum - userStockCount;
                    // アイテム追加
                    UserData.Get().AddItem((ItemType)item.itemType, item.itemId, addNum);
                }
            }

            // 分解した、パーツ削除
            if (partType == (uint)ItemType.Battery)
            {
                foreach (var item in response.tCannon)
                {
                    var list = UserData.Get().batteryData.ToList();
                    var index = list.FindIndex(x => x.serverId == item.id);
                    list.RemoveAt(index);
                    UserData.Get().batteryData = list.ToArray();
                }
            }
            else if (partType == (uint)ItemType.Barrel)
            {
                foreach (var item in response.tCannon)
                {
                    var list = UserData.Get().barrelData.ToList();
                    var index = list.FindIndex(x => x.serverId == item.id);
                    list.RemoveAt(index);
                    UserData.Get().barrelData = list.ToArray();
                }
            }
            else if (partType == (uint)ItemType.Bullet)
            {
                foreach (var item in response.tCannon)
                {
                    var list = UserData.Get().bulletData.ToList();
                    var index = list.FindIndex(x => x.serverId == item.id);
                    list.RemoveAt(index);
                    UserData.Get().bulletData = list.ToArray();
                }
            }
            else if (partType == (uint)ItemType.Accessory)
            {
                foreach (var item in response.tCannon)
                {
                    var list = UserData.Get().accessoriesData.ToList();
                    var index = list.FindIndex(x => x.serverId == item.id);
                    list.RemoveAt(index);
                    UserData.Get().accessoriesData = list.ToArray();
                }
            }

            // 装着中のギアを削除
            foreach (var item in response.tGear)
            {
                var list = UserData.Get().gearData.ToList();
                var index = list.FindIndex(x => x.serverId == item.serverId);
                list.RemoveAt(index);
                UserData.Get().gearData = list.ToArray();
                Debug.Log("index : " + index);
            }
            onCompleted?.Invoke();
        };
        request.Send();
    }

    /// <summary>
    /// ギア分解情報の通信
    /// </summary>
    public static void CallSellGear(uint gearId, Action onCompleted)
    {
        var request = new SharkWebRequest<GearDecompositionResponseData>("cannon/gearSell");
        request.SetRequestHeader("AccessToken", UserData.Get().hash);
        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "gearId", gearId }
        });

        // 成功時
        request.onSuccess = (response) =>
        {
            //コイン消費の反映
            UserData.Get().Set(response.tUsers);
            SharedUI.Instance.header.SetInfo(UserData.Get());

            // addItem responseデータ、itemIdとstockCountの順にソート
            var addItemSort = response.mItemSell
            .OrderBy(x => x.itemId)
            .ThenBy(c => c.itemNum)
            .ToArray();

            // アイテム追加
            foreach (var item in addItemSort)
            {
                // BattleItemの場合
                if((ItemType)item.itemType == ItemType.BattleItem)
                {
                    uint userStockCount = UserData.Get().itemData
                    .Where(x => x.itemId == item.itemId)
                    .Select(x => x.stockCount)
                    .SingleOrDefault();

                    uint addNum = item.itemNum - userStockCount;

                    UserData.Get().AddItem((ItemType)item.itemType, item.itemId, addNum);
                }
            }

            // 分解した、ギア削除
            var list = UserData.Get().gearData.ToList();
            var index = list.FindIndex(x => x.serverId == response.tGear.serverId);
            list.RemoveAt(index);
            UserData.Get().gearData = list.ToArray();

            onCompleted?.Invoke();
        };
        request.Send();
    }

    /// <summary>
    /// まとめてギア分解情報の通信
    /// </summary>
    public static void CallSellGearList(List<uint> gearList, Action onCompleted)
    {
        var request = new SharkWebRequest<GearDecompositionListResponseData>("cannon/gearSellList");
        request.SetRequestHeader("AccessToken", UserData.Get().hash);
        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "gearIdList", gearList }
        });

        /// 成功時
        request.onSuccess = (response) =>
        {
            //コイン消費の反映
            UserData.Get().Set(response.tUsers);
            SharedUI.Instance.header.SetInfo(UserData.Get());

            // addItem responseデータ、itemIdとstockCountの順にソート
            var addItemSort = response.mItemSell
            .OrderBy(x => x.itemId)
            .ThenBy(c => c.itemNum)
            .ToArray();

            // アイテム追加
            foreach (var item in addItemSort)
            {
                // BattleItemの場合
                if ((ItemType)item.itemType == ItemType.BattleItem)
                {
                    // 現在所有するアイテム数
                    uint userStockCount = UserData.Get().itemData
                    .Where(x => x.itemId == item.itemId)
                    .Select(x => x.stockCount)
                    .SingleOrDefault();

                    // 追加するアイテム数
                    uint addNum = item.itemNum - userStockCount;
                    // アイテム追加
                    UserData.Get().AddItem((ItemType)item.itemType, item.itemId, addNum);
                }
            }

            // 分解した、ギア削除
            foreach (var item in response.tGear)
            {
                var list = UserData.Get().gearData.ToList();
                var index = list.FindIndex(x => x.serverId == item.serverId);
                list.RemoveAt(index);
                UserData.Get().gearData = list.ToArray();
            }
            onCompleted?.Invoke();
        };
        request.Send();
    }
}
