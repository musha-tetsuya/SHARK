using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Battle;

/// <summary>
/// 召喚魚回遊ルートデータ制御クラス
/// </summary>
public class SummonFishRouteDataController : IBinary
{
    /// <summary>
    /// 召喚データ
    /// </summary>
    public class SummonData
    {
        /// <summary>
        /// 召喚データキー
        /// </summary>
        [JsonProperty("key")]
        public string key = null;
        /// <summary>
        /// ランダム回遊ルートデータ
        /// </summary>
        public RandomFishRouteData master = null;
        /// <summary>
        /// 魚マスターリスト
        /// </summary>
        public Master.FishData[] fishMasters = null;
        /// <summary>
        /// 魚ローダーリスト
        /// </summary>
        public Fish.Loader[] fishLoaders = null;
    }

    /// <summary>
    /// ID
    /// </summary>
    private Fish.ID id = new Fish.ID{ type = (byte)Fish.ID.Type.Summon };
    /// <summary>
    /// マスター
    /// </summary>
    private List<SummonData> dataList = new List<SummonData>();
    /// <summary>
    /// 魚リスト
    /// </summary>
    private List<(Fish fish, SummonDto dto)> fishList = new List<(Fish fish, SummonDto dto)>();
    /// <summary>
    /// 回遊ルートリスト
    /// </summary>
    private Dictionary<FishRouteData, Vector3[]> migrationRouteDataList = new Dictionary<FishRouteData, Vector3[]>();
    /// <summary>
    /// ローダー
    /// </summary>
    public AssetListLoader loader = new AssetListLoader();

    /// <summary>
    /// アセットローダー取得
    /// </summary>
    public static IEnumerable<IAssetLoader> GetAssetLoaders()
    {
        return Masters.SkillDB
            .GetList()
            .Where(x => x.skillType == (uint)SkillType.Summon)
            .SelectMany(x => Masters.SkillGroupDB.GetList().Where(y => y.skillId == x.id))
            .Select(x => JsonConvert.DeserializeObject<SummonData>(x.effectValue).key)
            .Select(x => new AssetLoader<RandomFishRouteData>(SharkDefine.GetRandomFishRouteDataPath(x)));
    }

    /// <summary>
    /// construct
    /// </summary>
    public SummonFishRouteDataController(RandomFishRouteData[] masters)
    {
        for (int i = 0; i < masters.Length; i++)
        {
            //データ
            var data = new SummonData();
            data.key = masters[i].name;
            data.master = masters[i];
            data.fishMasters = masters[i].fishDatas
                .Select(x => Masters.FishDB.FindById(x.fishId))
                .ToArray();
            data.fishLoaders = data.fishMasters
                .Select(x => new Fish.Loader(x.id, x.key))
                .ToArray();

            //リストに追加
            this.dataList.Add(data);

            //ローダーに追加
            this.loader.AddRange(data.fishLoaders.SelectMany(x => x));
        }
    }

    /// <summary>
    /// ActorNumberセット
    /// </summary>
    public void SetActorNo(int actorNo)
    {
        this.id.summonId = actorNo;
    }

    /// <summary>
    /// 魚生成
    /// </summary>
    public (Fish fish, SummonDto dto) CreateFish(string key)
    {
        Fish fish = null;
        SummonDto dto = null;

        for (int i = 0; i < this.dataList.Count; i++)
        {
            if (this.dataList[i].key.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                var summonData = this.dataList[i];

                //召喚データ
                dto = new SummonDto();
                dto.timeStamp = BattleGlobal.GetTimeStamp();
                dto.keyNo = (byte)i;
                dto.fishNo = (byte)UnityEngine.Random.Range(0, summonData.fishMasters.Length);
                dto.routeNo = (byte)UnityEngine.Random.Range(0, summonData.master.routeDatas.Count);
                dto.id = this.id;

                //生成回数増加
                this.id.createId++;

                //魚生成
                fish = this.AddFish(dto);

                break;
            }
        }
        return (fish, dto);
    }

    /// <summary>
    /// リストに魚を追加する
    /// </summary>
    public Fish AddFish(SummonDto dto)
    {
        var fish = this.CreateFish(dto);
        this.fishList.Add((fish, dto));
        return fish;
    }

    /// <summary>
    /// 魚生成
    /// </summary>
    private Fish CreateFish(SummonDto dto)
    {
        var summonData = this.dataList[dto.keyNo];
        var fishMaster = summonData.fishMasters[dto.fishNo];
        var fishLoader = summonData.fishLoaders[dto.fishNo];
        var routeData = summonData.master.routeDatas[dto.routeNo].data;
        var migrationRoute = this.GetMigrationRoute(routeData);

        //魚生成
        var fish = BattleGlobal.instance.CreateFish(fishLoader, fishMaster, dto.id);
        //回遊ルート設定
        fish.SetMigrationRoute(migrationRoute);
        //移動、回転速度設定
        fish.SetMoveValue(routeData.moveSpeed, routeData.rotationTime);
        //破棄時コールバック登録
        fish.onDestroy += this.OnDestroyFish;

        return fish;
    }

    /// <summary>
    /// 魚破棄時
    /// </summary>
    private void OnDestroyFish(Fish fish)
    {
        for (int i = 0; i < this.fishList.Count; i++)
        {
            if (this.fishList[i].fish == fish)
            {
                this.fishList.RemoveAt(i);
                break;
            }
        }
    }

    /// <summary>
    /// 回遊ルート取得
    /// </summary>
    private Vector3[] GetMigrationRoute(FishRouteData key)
    {
        Vector3[] migrationRoute = null;

        if (!this.migrationRouteDataList.TryGetValue(key, out migrationRoute))
        {
            migrationRoute = BattleGlobal.instance.GetBaseMigrationRoute(key);
            this.migrationRouteDataList.Add(key, migrationRoute);
        }

        return migrationRoute;
    }

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        writer.Write(this.fishList.Count);

        for (int i = 0; i < this.fishList.Count; i++)
        {
            (this.fishList[i].fish.GetResumeDto() as IBinary).Write(writer);

            (this.fishList[i].dto as IBinary).Write(writer);
        }
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            var resumeDto = new FishResumeDto();
            (resumeDto as IBinary).Read(reader);

            var summonDto = new SummonDto();
            (summonDto as IBinary).Read(reader);

            var fish = this.AddFish(summonDto);
            fish.SetResumeDto(resumeDto);
        }
    }
}
