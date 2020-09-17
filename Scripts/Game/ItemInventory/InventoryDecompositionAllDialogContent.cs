using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDecompositionAllDialogContent : MonoBehaviour
{
    /// <summary>
    /// 分解確認テキスト
    /// </summary>
    [SerializeField]
    private Text disassemblyQuestionText = null;
    /// <summary>
    /// パーツ前スクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView partsNameScrollView = null;
    /// <summary>
    /// パーツ前スクロールビューアイテム
    /// </summary>
    [SerializeField]
    private InventoryDecompositionNameScrollViewItem partsNameItemPrefab = null;

    [Header("分解スクロールビュー")]
    /// <summary>
    /// 分解リストスクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView decompositionScrollView = null;
    /// <summary>
    /// 分解時、修得アイテムアイコン
    /// </summary>
    [SerializeField]
    private ItemInventoryDecompositionScrollViewItem decompositionItemPrefab = null;

    /// <summary>
    /// パーツやギア名リスト
    /// </summary>
    private List<string> partsName = new List<string>();
    /// <summary>
    /// パーツデータ
    /// </summary>
    private UserPartsData partsData = null;
    private UserGearData gearData = null;

    private Master.ItemSellData[] getItem = null;

    /// <summary>
    /// まとめて分解確認ダイアログのデータロード
    /// </summary>
    public void Set(uint itemType, List<uint>partsServerIdList)
    {
        // 分解確認テキスト
        this.disassemblyQuestionText.text = Masters.LocalizeTextDB.GetFormat("DisassemblyQuestion", partsServerIdList.Count.ToString());
        List<Master.ItemSellData> getItemList = new List<Master.ItemSellData>();

        // パーツ名検索してリストに追加
        if (itemType == (uint)ItemType.Battery)
        {
            for (int i = 0; i < partsServerIdList.Count; i++)
            {
                // パーツデータ取得
                this.partsData = UserData.Get().batteryData
                .Where(x => x.serverId == partsServerIdList[i])
                .First();

                // パーツ名リストに追加
                this.partsName.Add(Masters.BatteryDB.FindById(this.partsData.itemId).name);
                // パーツ販売マスターID取得
                var sellItemId = Masters.BatteryDB.FindById(partsData.itemId).itemSellId;
                // パーツ販売データ取得
                this.getItem = Masters.ItemSellDB.GetList().FindAll(x => x.itemSellId == sellItemId).ToArray();

                // パーツ販売データリストに追加
                for(int y = 0; y < this.getItem.Length; y++)
                {
                    getItemList.Add(getItem[y]);
                }

                // パーツのギアマスターID
                var gearId = partsData.gearMasterIds;

                // ギアを装着している場合、ギア名をも検索してリストに追加
                for(int y = 0; y < gearId.Length; y++)
                {
                    // パーツ名前
                    this.partsName.Add(Masters.GearDB.FindById(gearId[y]).name);
                    // ギア販売マスターID取得
                    var sellGearId = Masters.GearDB.FindById(gearId[y]).itemSellId;

                    // ギアの分解時アイテム取得
                    Master.ItemSellData[] getGearItem = Masters.ItemSellDB.GetList().FindAll(x => x.itemSellId == sellGearId).ToArray();

                    // リストに追加
                    for (int z = 0; z < getGearItem.Length; z++)
                    {
                        getItemList.Add(getGearItem[z]);
                    }
                }
            }
        }
        else if (itemType == (uint)ItemType.Barrel)
        {
            for (int i = 0; i < partsServerIdList.Count; i++)
            {
                // パーツデータ取得
                this.partsData = UserData.Get().barrelData
                .Where(x => x.serverId == partsServerIdList[i])
                .First();

                // パーツ名リストに追加
                this.partsName.Add(Masters.BarrelDB.FindById(this.partsData.itemId).name);
                // パーツ販売マスターID取得
                var sellItemId = Masters.BatteryDB.FindById(partsData.itemId).itemSellId;
                // パーツ販売データ取得
                this.getItem = Masters.ItemSellDB.GetList().FindAll(x => x.itemSellId == sellItemId).ToArray();

                // パーツ販売データリストに追加
                for(int y = 0; y < this.getItem.Length; y++)
                {
                    getItemList.Add(getItem[y]);
                }

                // パーツのギアマスターID
                var gearId = partsData.gearMasterIds;

                // ギアを装着している場合、ギア名をも検索してリストに追加
                for(int y = 0; y < gearId.Length; y++)
                {
                    // パーツ名前
                    this.partsName.Add(Masters.GearDB.FindById(gearId[y]).name);
                    // ギア販売マスターID取得
                    var sellGearId = Masters.GearDB.FindById(gearId[y]).itemSellId;

                    // ギアの分解時アイテム取得
                    Master.ItemSellData[] getGearItem = Masters.ItemSellDB.GetList().FindAll(x => x.itemSellId == sellGearId).ToArray();

                    // リストに追加
                    for (int z = 0; z < getGearItem.Length; z++)
                    {
                        getItemList.Add(getGearItem[z]);
                    }
                }
            }
        }
        else if (itemType == (uint)ItemType.Bullet)
        {
            for (int i = 0; i < partsServerIdList.Count; i++)
            {
                // パーツデータ取得
                this.partsData = UserData.Get().bulletData
                .Where(x => x.serverId == partsServerIdList[i])
                .First();

                // パーツ名リストに追加
                this.partsName.Add(Masters.BarrelDB.FindById(this.partsData.itemId).name);
                // パーツ販売マスターID取得
                var sellItemId = Masters.BatteryDB.FindById(partsData.itemId).itemSellId;
                // パーツ販売データ取得
                this.getItem = Masters.ItemSellDB.GetList().FindAll(x => x.itemSellId == sellItemId).ToArray();

                // パーツ販売データリストに追加
                for(int y = 0; y < this.getItem.Length; y++)
                {
                    getItemList.Add(getItem[y]);
                }

                // パーツのギアマスターID
                var gearId = partsData.gearMasterIds;

                // ギアを装着している場合、ギア名をも検索してリストに追加
                for(int y = 0; y < gearId.Length; y++)
                {
                    // パーツ名前
                    this.partsName.Add(Masters.GearDB.FindById(gearId[y]).name);
                    // ギア販売マスターID取得
                    var sellGearId = Masters.GearDB.FindById(gearId[y]).itemSellId;

                    // ギアの分解時アイテム取得
                    Master.ItemSellData[] getGearItem = Masters.ItemSellDB.GetList().FindAll(x => x.itemSellId == sellGearId).ToArray();

                    // リストに追加
                    for (int z = 0; z < getGearItem.Length; z++)
                    {
                        getItemList.Add(getGearItem[z]);
                    }
                }
            }
        }
        else if (itemType == (uint)ItemType.Accessory)
        {
            for (int i = 0; i < partsServerIdList.Count; i++)
            {
                // パーツデータ取得
                this.partsData = UserData.Get().accessoriesData
                .Where(x => x.serverId == partsServerIdList[i])
                .First();

                // パーツ名リストに追加
                this.partsName.Add(Masters.BarrelDB.FindById(this.partsData.itemId).name);
                // パーツ販売マスターID取得
                var sellItemId = Masters.BatteryDB.FindById(partsData.itemId).itemSellId;
                // パーツ販売データ取得
                this.getItem = Masters.ItemSellDB.GetList().FindAll(x => x.itemSellId == sellItemId).ToArray();

                // パーツ販売データリストに追加
                for(int y = 0; y < this.getItem.Length; y++)
                {
                    getItemList.Add(getItem[y]);
                }
            }
        }
        else if (itemType == (uint)ItemType.Gear)
        {
            for (int i = 0; i < partsServerIdList.Count; i++)
            {
                // パーツデータ取得
                this.gearData = UserData.Get().gearData
                .Where(x => x.serverId == partsServerIdList[i])
                .First();

                // パーツ名リストに追加
                this.partsName.Add(Masters.GearDB.FindById(this.gearData.gearId).name);
                // パーツ販売マスターID取得
                var sellItemId = Masters.GearDB.FindById(this.gearData.gearId).itemSellId;
                // パーツ販売データ取得
                this.getItem = Masters.ItemSellDB.GetList().FindAll(x => x.itemSellId == sellItemId).ToArray();

                // パーツ販売データリストに追加
                for(int y = 0; y < this.getItem.Length; y++)
                {
                    getItemList.Add(getItem[y]);
                }
            }
        }
        
        this.getItem = getItemList.ToArray();

        // パーツ名アイテムセット
        this.partsNameScrollView.Initialize(
            this.partsNameItemPrefab.gameObject,
            this.partsName.Count,
            this.OnUpdatePartsScrollViewItem
        );

        this.decompositionScrollView.Initialize(
            this.decompositionItemPrefab.gameObject,
            this.getItem.Length,
            this.OnUpdateDecompositionScrollViewItem
            
        );
    }

    // パーツ名アイテムデータロード
    private void OnUpdatePartsScrollViewItem(GameObject gobj, int elementId)
    {
        var item = gobj.GetComponent<InventoryDecompositionNameScrollViewItem>();
        var partsName = this.partsName[elementId];

        item.Set(
            partsName: partsName
        );
    }

    private void OnUpdateDecompositionScrollViewItem(GameObject gobj, int elementId)
    {
        var item = gobj.GetComponent<ItemInventoryDecompositionScrollViewItem>();
        item.Set(getItem[elementId].itemType, getItem[elementId].itemId, getItem[elementId].itemNum);
    }
}
