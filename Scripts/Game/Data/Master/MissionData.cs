using System;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// ミッションデータ
    /// </summary>
    public class MissionData : ModelBase
    {
        /// <summary>
        /// ミッションのメインタイプ
        /// </summary>
        public enum MainType
        {
            SingleGetSter = 1,          //シングルモードで星を〇〇個入手する
            SingleTotalStageClear = 2,  //シングルモードを〇〇回クリアする
            SingleSelectStageClear = 3, //シングルモードステージ△△を〇〇回クリアする
            MultiGetCoin = 4,           //マルチモードでコインを〇〇枚入手する
            MultiConsumCoin = 5,        //マルチモードでコインを〇〇枚消費する
            MultiConsumTotalItem = 6,   //マルチモードでバトルアイテムを〇〇個使用する
            MultiConsumSelectItem = 7,  //マルチモードでバトルアイテム△△を〇〇個使用する
            MultiCatchTotalFish = 8,    //マルチモードで魚を〇〇匹捕まえる
            MultiCatchSelectFish = 9,   //マルチモードで特定の魚△△を〇〇匹捕まえる
            ReachLevel = 10,            //〇〇レベルになる
            MultiGetRyugyoku = 11,      //マルチモードで龍(?)玉を合計で〇〇個入手する
            MultiPlaySlot = 12,         //マルチモードでドラゴン(?)スロットを〇〇回回す
            MultiGetJP = 13,            //マルチモードでJPを〇〇回獲得する
            MultiPlayFVAttack = 14,     //マルチモードでFVアタックを〇〇回発動する
            GetTotalGear = 15,          //ギアを〇〇個入手する
            GetBatteryGear = 16,        //台座用ギアを〇〇個入手する
            GetBarrelGear = 17,         //砲身用ギアを〇〇個入手する
            GetBulletGear = 18,         //砲弾用ギアを〇〇個入手する
            DecompositionGear = 19,     //ギアを〇〇個分解する
            LoginTotal = 20,            //〇〇日ログインする
            GetTypesBattery = 21,       //〇〇種類砲台を入手する
            ReachVIPRank = 22,          //VIPランク〇〇になる
            LoginSelect = 23,           //〇〇時にログインする
            LinkedAccount = 24          //△△連携する(Wechat連携とか)
        }


        [JsonProperty("missionGroup")]
        public uint missionGroup { get; set; }

        [JsonProperty("missionTypeId")]
        public uint missionTypeId { get; set; }

        [JsonProperty("missionTypeSubId")]
        public uint? missionTypeSubId { get; set; }

        [JsonProperty("missionDepth")]
        public uint missionDepth { get; set; }

        [JsonProperty("missionCount")]
        public ulong missionCount { get; set; }

        [JsonProperty("missionRewardId")]
        public uint missionRewardId { get; set; }

        [JsonProperty("startDate")]
        public DateTime? startDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime? endDate { get; set; }

        [JsonProperty("day")]
        public uint? day { get; set; }
    }
}
