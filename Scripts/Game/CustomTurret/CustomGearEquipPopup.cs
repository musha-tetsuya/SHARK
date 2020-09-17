using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CustomGearEquipPopup : MonoBehaviour
{
    /// <summary>
    /// ギア装着ポップアップスクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView gearScrollView = null;
    /// <summary>
    /// ギア装着ポップアップスクロールビューアイテム
    /// </summary>
    [SerializeField]
    private CustomGearScrollViewItem gearScrollViewItemPrefab = null;
    [SerializeField]
    private CustomGearScrollViewItem fastenGearViewItemPrefab = null;
    /// <summary>
    /// 着脱確認ダイアログ内容プレハブ
    /// </summary>
    [SerializeField]
    private CustomGearConfirmDialogContent confirmDialogContentPrefab = null;

    /// <summary>
    /// ギア外すボタン
    /// </summary>
    [SerializeField]
    private Button unEquipGearButton = null;

    /// <summary>
    /// 自身のダイアログ
    /// </summary>
    private SimpleDialog dialog = null;
    /// <summary>
    /// パーツ別に分類した、ギアデータの臨時保存
    /// </summary>
    private UserGearData[] freeGears;
    /// <summary>
    /// ギア装着対象のパーツ
    /// </summary>
    private UserPartsData partsData = null;
    /// <summary>
    /// 変更前ギア
    /// </summary>
    private UserGearData beforeGear = null;
    /// <summary>
    /// 更新通知
    /// </summary>
    private Action onReflesh = null;

    /// <summary>
    /// 装着する、ギア選択ポップアップをセット
    /// </summary>
    public void Setup(
        SimpleDialog dialog,
        UserPartsData partsData,
        uint gearId,
        Action onReflesh)
    {
        this.dialog = dialog;
        this.partsData = partsData;
        this.beforeGear = UserData.Get().gearData.FirstOrDefault(x => x.gearId == gearId && x.partsServerId == this.partsData.serverId);
        this.onReflesh = onReflesh;

        //パーツに装着出来るギアのタイプ
        var gearType = partsData.itemType == (uint)ItemType.Battery ? GearType.Battery
                     : partsData.itemType == (uint)ItemType.Barrel  ? GearType.Barrel
                     : partsData.itemType == (uint)ItemType.Bullet  ? GearType.Bullet
                     : 0;

        //装着可能なギア一覧（パーツタイプが一致し、どのパーツにも装着されていないもの）
        this.freeGears = UserData.Get().gearData
            .Where(x => x.gearType == (uint)gearType && !x.partsServerId.HasValue)
            .ToArray();

        // 所持するギアがない場合(Nullエラーため)
        if(this.beforeGear == null && this.freeGears.Length == 0)
        {
            // 固定されている装着ギアパネルが未装着の場合(ギア未所持)
            this.fastenGearViewItemPrefab.SetNotEquippedPanel();
        }
        // 所持するギアがある場合
        else
        {
            // ギア未装着
            if (gearId == 0)
            {
                // 固定されている装着ギアパネルが未装着の場合(ギア所持)
                this.fastenGearViewItemPrefab.SetNotEquippedPanel();
            }
            // ギアを装着してある場合
            else
            {
                // 固定されている装着ギアパネルにデータロッド
                this.fastenGearViewItemPrefab.SetGearData(beforeGear, null);
                this.unEquipGearButton.gameObject.SetActive(true);

                //装着したギアを外すボタンをロードするため、null空間を生成
                //this.freeGears = new UserGearData[] { null }.Concat(this.freeGears).ToArray();
            }
        }

        // ScrollView生成
        this.gearScrollView.Initialize(
            this.gearScrollViewItemPrefab.gameObject,
            this.freeGears.Length,
            this.LoadGearPrefabData
        );
    }

    /// <summary>
    /// ScrollViewのアイテムのデータセット・クリックした時
    /// </summary>
    private void LoadGearPrefabData(GameObject gobj, int elementId)
    {
        var item = gobj.GetComponent<CustomGearScrollViewItem>();
        item.SetGearData(this.freeGears[elementId], this.OnClickGearScrollViewItem);
    }

    /// <summary>
    /// ギア装着決定ダイアログ
    /// </summary>
    private void OnClickGearScrollViewItem(CustomGearScrollViewItem item)
    {
        SoundManager.Instance.PlaySe(SeName.YES);

        if (this.dialog.isClose) return;

        var dialog = SharedUI.Instance.ShowSimpleDialog();
        var content = dialog.AddContent(this.confirmDialogContentPrefab);
        content.Setup(
            dialog: dialog,
            partsData: this.partsData,
            beforeGear: this.beforeGear,
            afterGear: item.gearData,
            onReflesh: () =>
            {
                //ギアに変更があったのでリフレッシュ通知
                this.onReflesh?.Invoke();
                this.dialog.Close();
            },
            onCancel: () =>
            {
                //キャンセルで戻ってきた。現状特に何もしない。
            });
    }

    /// <summary>
    /// ギア外すボタン
    /// </summary>
    public void OnClickUnEquipGearButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);

        if (this.dialog.isClose) return;

        var dialog = SharedUI.Instance.ShowSimpleDialog();
        var content = dialog.AddContent(this.confirmDialogContentPrefab);
        content.Setup(
            dialog: dialog,
            partsData: this.partsData,
            beforeGear: this.beforeGear,
            afterGear: null,
            onReflesh: () =>
            {
                //ギアに変更があったのでリフレッシュ通知
                this.onReflesh?.Invoke();
                this.dialog.Close();
            },
            onCancel: () =>
            {
                //キャンセルで戻ってきた。現状特に何もしない。
            });
    }
}
