using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battle {

/// <summary>
/// バトルアイテムアイコンUI
/// </summary>
public class UIBattleItemIcon : TimePauseBehaviour
{
    /// <summary>
    /// アイコン
    /// </summary>
    [SerializeField]
    private CommonIcon icon = null;
    /// <summary>
    /// グレーアウトイメージ
    /// </summary>
    [SerializeField]
    private Image grayoutImage = null;
    /// <summary>
    /// 禁止マークイメージ
    /// </summary>
    [SerializeField]
    private Image banImage = null;

    /// <summary>
    /// ユーザーアイテムデータ
    /// </summary>
    public UserItemData userItemData { get; private set; }
    /// <summary>
    /// アイテムマスターデータ
    /// </summary>
    private Master.BattleItemData itemMaster = null;
    /// <summary>
    /// スキルグループ管理
    /// </summary>
    private SkillGroupManager skillGroupManager = new SkillGroupManager();
    /// <summary>
    /// クールタイム時間
    /// </summary>
    private float coolTime = 0f;
    /// <summary>
    /// クールタイム時間最大値
    /// </summary>
    private float maxCoolTime = 0f;
    /// <summary>
    /// ローダー
    /// </summary>
    public AssetListLoader loader { get; private set; } = new AssetListLoader();
    /// <summary>
    /// ステート処理
    /// </summary>
    private Action<float> stateAction = null;
    /// <summary>
    /// クリック時コールバック
    /// </summary>
    private Action<UIBattleItemIcon> onClick = null;

    /// <summary>
    /// OnDestroy
    /// </summary>
    protected override void OnDestroy()
    {
        this.loader.Unload();
        this.loader.Clear();
        base.OnDestroy();
    }

    /// <summary>
    /// Awake
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        this.enabled = false;
        this.icon.iconImage.enabled = false;
        this.icon.countText.text = null;
        this.icon.SetButtonInteractable(false);
        this.grayoutImage.enabled = false;
        this.SetVisibleBanMark(false);
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(UserItemData userItemData, Action<UIBattleItemIcon> onClick)
    {
        this.onClick = onClick;
        this.userItemData = userItemData;
        this.itemMaster = Masters.BattleItemDB.FindById(this.userItemData.itemId);
        this.skillGroupManager.AddSkillGroup(this.itemMaster.skillGroupId);

        this.maxCoolTime = this.skillGroupManager
            .Where(x => x is SkillBattleItemBase)
            .Select(x => (x as SkillBattleItemBase).coolTime)
            .Max() * Masters.MilliSecToSecond;
    }

    /// <summary>
    /// ロード
    /// </summary>
    public void Load(Action onLoaded)
    {
        //アイコンスプライト
        string spritePath = SharkDefine.GetBattleItemIconSpritePath(this.itemMaster.key);
        this.loader.Add<Sprite>(spritePath);

        //アイテム使用時SE
        var item = this.skillGroupManager.Find(x => x is SkillBattleItemBase) as SkillBattleItemBase;
        if (item != null)
        {
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(item.seName));
        }

        //ロード
        this.loader.Load(onLoaded);
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup()
    {
        this.enabled = true;
    }

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        //アイコンスプライトセット
        string spritePath = SharkDefine.GetBattleItemIconSpritePath(this.itemMaster.key);
        this.icon.iconImage.enabled = true;
        this.icon.SetIconSprite(this.loader[spritePath].handle.asset as Sprite);

        //所持数テキスト更新
        this.RefleshStockCountText();

        //ボタンの有効無効更新
        this.RefleshButtonInteractable();

        //スキルによるアイテム再使用間隔の短縮
        float shortenCoolTime = BattleGlobal.instance.userData.skill.ShortenCoolTime();
        //Debug.LogFormat("アイテムID{0}:{1}　再使用間隔{2}秒。スキル効果で{3}秒短縮します。", this.itemMaster.id, this.itemMaster.name, this.maxCoolTime, shortenCoolTime);
        this.maxCoolTime = Mathf.Max(0f, this.maxCoolTime - shortenCoolTime);
    }

    /// <summary>
    /// Run
    /// </summary>
    public void Run(float deltaTime)
    {
        if (this.enabled)
        {
            this.stateAction?.Invoke(deltaTime);
        }
    }

    /// <summary>
    /// クールタイム中ステート
    /// </summary>
    private void CoolTimeState(float deltaTime)
    {
        this.RefleshCoolTimeGaugeValue();

        if (this.coolTime <= 0f)
        {
            this.RefleshButtonInteractable();
            this.stateAction = null;
        }

        this.coolTime -= deltaTime;
    }

    /// <summary>
    /// クリック時
    /// </summary>
    public void OnClick()
    {
        this.onClick?.Invoke(this);
    }

    /// <summary>
    /// 使用
    /// </summary>
    public void OnUse()
    {
        //所持数テキスト更新
        this.RefleshStockCountText();

        //クールタイム開始
        this.coolTime = this.maxCoolTime;
        this.stateAction = this.CoolTimeState;

        //クールタイム開始 or アイテム残数が無くなったらボタン押せなくなる
        this.RefleshButtonInteractable();

        //アイテム効果発動
        this.skillGroupManager.OnUseBattleItem();

        //SE再生
        for (int i = 0; i < this.skillGroupManager.Count; i++)
        {
            var skill = this.skillGroupManager[i] as SkillBattleItemBase;
            if (skill != null)
            {
                SoundManager.Instance.PlaySe(skill.seName);
            }
        }
    }

    /// <summary>
    /// アイテム所持数テキスト更新
    /// </summary>
    public void RefleshStockCountText()
    {
        this.icon.SetCountText(this.userItemData.stockCount);
        this.icon.countText.color = this.userItemData.stockCount > 0 ? Color.black : Color.red;
    }

    /// <summary>
    /// クールタイムゲージ値更新
    /// </summary>
    private void RefleshCoolTimeGaugeValue()
    {
        float fillAmount = (this.maxCoolTime > 0f) ? Mathf.Clamp01(this.coolTime / this.maxCoolTime) : 0f;
        this.grayoutImage.fillAmount = fillAmount;
        this.grayoutImage.enabled = fillAmount > 0f;
    }

    /// <summary>
    /// ボタンの有効無効更新
    /// </summary>
    public void RefleshButtonInteractable()
    {
        bool interactable = this.enabled
                         && this.userItemData.stockCount > 0    //ストックがある
                         && this.coolTime <= 0f                 //クールタイム中じゃない
                         && !this.banImage.isActiveAndEnabled;  //使用禁止マークが非表示

        this.icon.SetButtonInteractable(interactable);
    }

    /// <summary>
    /// 禁止マーク表示切替
    /// </summary>
    public void SetVisibleBanMark(bool visible)
    {
        this.banImage.gameObject.SetActive(visible);
    }

    /// <summary>
    /// 禁止マークが表示中かどうか
    /// </summary>
    public bool IsVisibleBanMark()
    {
        return this.banImage.gameObject.activeSelf;
    }

    /// <summary>
    /// 砲台制御するかどうか
    /// </summary>
    public bool IsTurretController()
    {
        if (this.skillGroupManager != null)
        {
            for (int i = 0, imax = this.skillGroupManager.Count; i < imax; i++)
            {
                var skill = this.skillGroupManager[i] as SkillBattleItemBase;
                if (skill != null && skill.isTurretController)
                {
                    return true;
                }
            }
        }
        return false;
    }
}

}