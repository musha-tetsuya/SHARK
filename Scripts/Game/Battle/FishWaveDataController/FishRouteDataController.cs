using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// 回遊ルートデータ制御クラス
/// </summary>
public class FishRouteDataController : IFormationEventReceiver, IBinary
{
    /// <summary>
    /// ID
    /// </summary>
    private Fish.ID id;
    /// <summary>
    /// マスターデータ
    /// </summary>
    private FishRouteData master = null;
    /// <summary>
    /// Delay
    /// </summary>
    private float delay = 0f;
    /// <summary>
    /// 生成間隔
    /// </summary>
    private float interval = 0f;
    /// <summary>
    /// 生成回数
    /// </summary>
    private int createCount = 0;
    /// <summary>
    /// 魚リスト
    /// </summary>
    private List<(Fish fish, byte unitNo)> fishList = new List<(Fish fish, byte unitNo)>();
    /// <summary>
    /// 魚マスターデータ
    /// </summary>
    private Master.FishData fishMaster = null;
    /// <summary>
    /// 魚FBXロードハンドル
    /// </summary>
    private Fish.Loader fishLoader = null;
    /// <summary>
    /// 回遊ルート
    /// </summary>
    private Vector3[][] migrationRoutes = null;
    /// <summary>
    /// ローダー
    /// </summary>
    public AssetListLoader loader = new AssetListLoader();
    /// <summary>
    /// Update処理
    /// </summary>
    private Action<float> onUpdate = null;
    /// <summary>
    /// 全魚生成完了時コールバック
    /// </summary>
    public Action onFinished = null;

    /// <summary>
    /// construct
    /// </summary>
    public FishRouteDataController(Fish.ID id, FishRouteData master, float delay)
    {
        Debug.Assert(1 <= master.unitSize && master.unitSize <= 5);

        this.id = id;
        this.master = master;
        this.delay = delay;
        this.fishMaster = Masters.FishDB.FindById(master.fishId);
        this.fishLoader = new Fish.Loader(this.fishMaster.id, this.fishMaster.key);
        this.loader.AddRange(this.fishLoader);
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset(float delay)
    {
        this.delay = delay;
        this.interval = 0f;
        this.createCount = 0;
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup()
    {
        if (this.migrationRoutes == null)
        {
            //回遊ルート生成
            this.migrationRoutes = BattleGlobal.instance.GetMigrationRoute(this.master);
        }

        //メインステートへ
        this.onUpdate = this.MainStateUpdate;
    }

    /// <summary>
    /// Update
    /// </summary>
    public void Update(float deltaTime)
    {
        this.onUpdate?.Invoke(deltaTime);
    }

    /// <summary>
    /// メインステート
    /// </summary>
    private void MainStateUpdate(float deltaTime)
    {
        //Delay中
        if (this.delay > 0f)
        {
            this.delay -= deltaTime;
            return;
        }

        if (this.interval <= 0f)
        {
            for (int i = 0; i < this.master.unitSize; i++)
            {
                this.id.createId = (uint)this.createCount;
                this.createCount++;

                //魚生成
                var fish = this.CreateFish(i, this.id);
                this.fishList.Add((fish, (byte)i));

                if (this.createCount >= this.master.amount)
                {
                    //全匹生成したので終了
                    this.onFinished?.Invoke();
                    this.onUpdate = null;
                    return;
                }
            }

            this.interval += this.master.interval;
        }

        //時間カウント
        this.interval -= deltaTime;
    }

    /// <summary>
    /// 魚生成
    /// </summary>
    private Fish CreateFish(int unitNo, Fish.ID fishId)
    {
        //魚生成
        var fish = BattleGlobal.instance.CreateFish(this.fishLoader, this.fishMaster, fishId);
        //回遊ルート設定
        fish.SetMigrationRoute(this.migrationRoutes[unitNo]); 
        //移動、回転速度設定
        fish.SetMoveValue(this.master.moveSpeed, this.master.rotationTime);
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
    /// 生きている魚がいるかどうか
    /// </summary>
    public bool HasAlivedFish()
    {
        return this.fishList.Exists(x => x.fish.HasTargetPosition());
    }

    /// <summary>
    /// 一時停止時
    /// </summary>
    void IFormationEventReceiver.OnPause()
    {
        for (int i = 0; i < this.fishList.Count; i++)
        {
            (this.fishList[i].fish as IFormationEventReceiver)?.OnPause();
        }
    }

    /// <summary>
    /// 再開時
    /// </summary>
    void IFormationEventReceiver.OnPlay()
    {
        for (int i = 0; i < this.fishList.Count; i++)
        {
            (this.fishList[i].fish as IFormationEventReceiver)?.OnPlay();
        }
    }

    /// <summary>
    /// 強制終了時
    /// </summary>
    void IFormationEventReceiver.OnQuit()
    {
        for (int i = 0; i < this.fishList.Count; i++)
        {
            (this.fishList[i].fish as IFormationEventReceiver)?.OnQuit();
        }
    }

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        writer.Write(this.delay);
        writer.Write(this.interval);
        writer.Write(this.createCount);
        writer.Write(this.fishList.Count);

        for (int i = 0; i < this.fishList.Count; i++)
        {
            (this.fishList[i].fish.GetResumeDto() as IBinary).Write(writer);

            this.fishList[i].fish.id.Write(writer);

            writer.Write(this.fishList[i].unitNo);
        }
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        this.delay = reader.ReadSingle();
        this.interval = reader.ReadSingle();
        this.createCount = reader.ReadInt32();
        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            var dto = new FishResumeDto();
            (dto as IBinary).Read(reader);

            var fishId = new Fish.ID();
            fishId.Read(reader);

            var unitNo = reader.ReadByte();

            var fish = this.CreateFish((int)unitNo, fishId);
            fish.SetResumeDto(dto);
            this.fishList.Add((fish, unitNo));
        }
    }

}//class FormationRouteDataController

}//namespace Battle
