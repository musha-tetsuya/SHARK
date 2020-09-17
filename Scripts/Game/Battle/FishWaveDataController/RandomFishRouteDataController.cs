using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Battle;

/// <summary>
/// ランダム回遊ルートデータ制御クラス
/// </summary>
public class RandomFishRouteDataController : IBinary
{
    /// <summary>
    /// 回遊ルートデータ
    /// </summary>
    private class MigrationRouteData
    {
        /// <summary>
        /// 回遊ルートキー
        /// </summary>
        public FishRouteData key = null;
        /// <summary>
        /// 回遊ルート基礎
        /// </summary>
        public Vector3[] baseRoute = null;
        /// <summary>
        /// ユニットサイズをキーとした回遊ルート
        /// </summary>
        public Dictionary<int, Vector3[][]> unitSizeToRoutes = new Dictionary<int, Vector3[][]>();
    }

    /// <summary>
    /// 回遊ルート情報
    /// </summary>
    private struct MigrationRouteInfo
    {
        /// <summary>
        /// 回遊ルート番号
        /// </summary>
        public byte routeNo;
        /// <summary>
        /// 魚番号
        /// </summary>
        public byte fishNo;
        /// <summary>
        /// ユニットサイズ
        /// </summary>
        public byte unitSize;
        /// <summary>
        /// ユニット番号
        /// </summary>
        public byte unitNo;
    }

    /// <summary>
    /// ID
    /// </summary>
    private Fish.ID id;
    /// <summary>
    /// マスター
    /// </summary>
    private RandomFishRouteData master = null;
    /// <summary>
    /// ランダム
    /// </summary>
    private SharkRandom random = new SharkRandom();
    /// <summary>
    /// 生成開始までの遅延
    /// </summary>
    private float delay = 0f;
    /// <summary>
    /// 時間
    /// </summary>
    private float time = 0f;
    /// <summary>
    /// 現在のイベント番号
    /// </summary>
    private int eventNo = 0;
    /// <summary>
    /// 生成回数
    /// </summary>
    private int createCount = 0;
    /// <summary>
    /// 回遊ルート番号リスト
    /// </summary>
    private int[] fishRouteNums = null;
    /// <summary>
    /// 魚マスターリスト
    /// </summary>
    private Master.FishData[] fishMasters = null;
    /// <summary>
    /// 魚ローダーリスト
    /// </summary>
    private Fish.Loader[] fishLoaders = null;
    /// <summary>
    /// 魚リスト
    /// </summary>
    private List<(Fish fish, MigrationRouteInfo info)> fishList = new List<(Fish fish, MigrationRouteInfo info)>();
    /// <summary>
    /// 回遊ルートデータリスト
    /// </summary>
    private List<MigrationRouteData> migrationRouteDatas = new List<MigrationRouteData>();
    /// <summary>
    /// ローダー
    /// </summary>
    public AssetListLoader loader = new AssetListLoader();
    /// <summary>
    /// Update処理
    /// </summary>
    private System.Action<float> onUpdate = null;

    /// <summary>
    /// construct
    /// </summary>
    public RandomFishRouteDataController(
        Fish.ID id,
        RandomFishRouteData master)
    {
        this.id = id;
        this.id.type = (byte)Fish.ID.Type.Random;
        this.master = master;
        this.delay = this.master.delay;

        this.fishRouteNums = new int[this.master.routeDatas.Count];
        for (int i = 0; i < this.fishRouteNums.Length; i++)
        {
            this.fishRouteNums[i] = i;
        }

        this.fishMasters = this.master.fishDatas
            .Select(x => Masters.FishDB.FindById(x.fishId))
            .ToArray();

        this.fishLoaders = this.fishMasters
            .Select(x => new Fish.Loader(x.id, x.key))
            .ToArray();

        this.loader.AddRange(this.fishLoaders.SelectMany(x => x));
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup()
    {
#if DEBUG
        if (this.master.createEvent == null)
        {
            Debug.LogErrorFormat("{0}: 生成イベントデータがnull", this.master.name);
            return;
        }
#endif
        this.onUpdate = this.MainStateUpdate;
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset()
    {
        this.delay = this.master.delay;
        this.time = 0f;
        this.eventNo = 0;
        this.random.Reset();
    }

    /// <summary>
    /// Update
    /// </summary>
    public void Update(float deltaTime)
    {
        this.onUpdate?.Invoke(deltaTime);
    }

    /// <summary>
    /// メインステートUpdate
    /// </summary>
    private void MainStateUpdate(float deltaTime)
    {
        if (this.delay > 0f)
        {
            this.delay -= deltaTime;
            return;
        }

        //現在の生成イベント
        var currentEvent = this.master.createEvent.events[this.eventNo];

        //イベントタイミングを迎えたら
        while (this.time >= currentEvent.time)
        {
            if (currentEvent.functionName == "End")
            {
                //ループ
                this.time -= currentEvent.time;
            }
            else
            {
                //回遊ルート番号をランダムソート
                int[] randomRouteNums = this.fishRouteNums.OrderBy(x => this.random.Range(0, 100)).ToArray();

                MigrationRouteInfo info;

                for (int i = 0; i < randomRouteNums.Length; i++)
                {
                    //回遊ルート番号決定
                    info.routeNo = (byte)randomRouteNums[i];
                    var routeData = this.master.routeDatas[info.routeNo];

                    //このルートで生成するかどうか確率判定
                    bool isWin = this.random.Range(0, 100) < routeData.probability;
                    if (!isWin)
                    {
                        continue;
                    }

                    //ランダム魚番号決定
                    info.fishNo = (byte)this.random.Range(0, this.master.fishDatas.Count);
                    var randomFishData = this.master.fishDatas[info.fishNo];

                    //ユニットサイズ決定
                    info.unitSize = (byte)Mathf.Clamp(1 + this.random.Range(0, (int)randomFishData.unitSizeMax), 1, 5);

                    //回遊ルート決定
                    var migrationRoute = this.GetMigrationRoute(routeData.data, info.unitSize, randomFishData.unitSpread);

                    for (info.unitNo = 0; info.unitNo < info.unitSize; info.unitNo++)
                    {
                        this.id.createId = (uint)this.createCount;
                        this.createCount++;

                        //魚生成
                        var fish = this.CreateFish(info, this.id, migrationRoute);
                        this.fishList.Add((fish, info));
                    }
                }
            }

            //次のイベントへ
            this.eventNo++;
            this.eventNo %= this.master.createEvent.events.Length;
            currentEvent = this.master.createEvent.events[this.eventNo];
        }

        //時間カウント
        this.time += deltaTime;
    }

    /// <summary>
    /// 魚生成
    /// </summary>
    private Fish CreateFish(MigrationRouteInfo info, Fish.ID fishId, Vector3[][] migrationRoute)
    {
        var fishLoader = this.fishLoaders[info.fishNo];
        var fishMaster = this.fishMasters[info.fishNo];
        var routeData = this.master.routeDatas[info.routeNo].data;

        //魚生成
        var fish = BattleGlobal.instance.CreateFish(fishLoader, fishMaster, fishId);
        //回遊ルート設定
        fish.SetMigrationRoute(migrationRoute[info.unitNo]);
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
    private Vector3[][] GetMigrationRoute(FishRouteData key, int unitSize, float unitSpread)
    {
        Vector3[][] migrationRoute = null;

        var data = this.migrationRouteDatas.Find(x => x.key == key);
        if (data == null)
        {
            data = new MigrationRouteData();
            data.key = key;
            data.baseRoute = BattleGlobal.instance.GetBaseMigrationRoute(key);
            this.migrationRouteDatas.Add(data);
        }

        if (!data.unitSizeToRoutes.TryGetValue(unitSize, out migrationRoute))
        {
            migrationRoute = BattleGlobal.instance.GetMigrationRoute(data.baseRoute, unitSize, unitSpread);
            data.unitSizeToRoutes.Add(unitSize, migrationRoute);
        }

        return migrationRoute;
    }

    /// <summary>
    /// 画面からはける
    /// </summary>
    public void OffStage()
    {
        for (int i = 0; i < this.fishList.Count; i++)
        {
            this.fishList[i].fish.OffStage();
        }

        this.fishList.Clear();
    }

    /// <summary>
    /// 生きている魚がいるかどうか
    /// </summary>
    public bool HasAlivedFish()
    {
        return this.fishList.Exists(x => x.fish.HasTargetPosition());
    }

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        (this.random as IBinary).Write(writer);
        writer.Write(this.delay);
        writer.Write(this.time);
        writer.Write(this.eventNo);
        writer.Write(this.createCount);
        writer.Write(this.fishList.Count);

        for (int i = 0; i < this.fishList.Count; i++)
        {
            (this.fishList[i].fish.GetResumeDto() as IBinary).Write(writer);

            this.fishList[i].fish.id.Write(writer);

            writer.Write(this.fishList[i].info.routeNo);
            writer.Write(this.fishList[i].info.fishNo);
            writer.Write(this.fishList[i].info.unitSize);
            writer.Write(this.fishList[i].info.unitNo);
        }
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        (this.random as IBinary).Read(reader);
        this.delay = reader.ReadSingle();
        this.time = reader.ReadSingle();
        this.eventNo = reader.ReadInt32();
        this.createCount = reader.ReadInt32();
        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            var dto = new FishResumeDto();
            (dto as IBinary).Read(reader);

            var fishId = new Fish.ID();
            fishId.Read(reader);

            MigrationRouteInfo info;
            info.routeNo = reader.ReadByte();
            info.fishNo = reader.ReadByte();
            info.unitSize = reader.ReadByte();
            info.unitNo = reader.ReadByte();

            var randomFishData = this.master.fishDatas[info.fishNo];
            var routeData = this.master.routeDatas[info.routeNo];
            var migrationRoute = this.GetMigrationRoute(routeData.data, info.unitSize, randomFishData.unitSpread);

            var fish = this.CreateFish(info, fishId, migrationRoute);
            fish.SetResumeDto(dto);
            this.fishList.Add((fish, info));
        }
    }
}

