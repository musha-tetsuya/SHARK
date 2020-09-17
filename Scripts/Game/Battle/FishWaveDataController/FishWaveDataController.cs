using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle {

/// <summary>
/// バトルWAVE管理クラス（適当）
/// </summary>
public class FishWaveDataController : IBinary
{
    /// <summary>
    /// ID
    /// </summary>
    private Fish.ID id;
    /// <summary>
    /// マスター
    /// </summary>
    public FishWaveData master { get; private set; }
    /// <summary>
    /// Delay
    /// </summary>
    private float delay = 0f;
    /// <summary>
    /// 終了した編隊数
    /// </summary>
    private int finishedFormationCount = 0;
    /// <summary>
    /// 編隊データ制御リスト
    /// </summary>
    private List<FormationDataController> formationDataControllers = new List<FormationDataController>();
    /// <summary>
    /// ローダー
    /// </summary>
    public AssetListLoader loader = new AssetListLoader();
    /// <summary>
    /// Update処理
    /// </summary>
    private Action<float> onUpdate = null;
    /// <summary>
    /// 全編隊終了時コールバック
    /// </summary>
    public Action onFinished = null;

    /// <summary>
    /// construct
    /// </summary>
    public FishWaveDataController(Fish.ID id, FishWaveData master, float delay)
    {
        this.id = id;
        this.master = master;
        this.delay = delay;

        for (int i = 0; i < master.formationDatas.Count; i++)
        {
            this.id.formationId = (byte)i;

            var controller = new FormationDataController(
                id: this.id,
                master: this.master.formationDatas[i].data,
                delay: this.master.formationDatas[i].delay
            );
            controller.onFinished = this.OnFinishedFormation;

            this.formationDataControllers.Add(controller);
            this.loader.AddRange(controller.loader);
        }
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset(float delay)
    {
        this.delay = delay;
        this.finishedFormationCount = 0;

        for (int i = 0; i < this.formationDataControllers.Count; i++)
        {
            this.formationDataControllers[i].Reset(this.master.formationDatas[i].delay);
        }
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup()
    {
        for (int i = 0; i < this.formationDataControllers.Count; i++)
        {
            this.formationDataControllers[i].Setup();
        }

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
        
        for (int i = 0, imax = this.formationDataControllers.Count; i < imax; i++)
        {
            this.formationDataControllers[i].Update(deltaTime);
        }
    }

    /// <summary>
    /// 編隊終了時
    /// </summary>
    private void OnFinishedFormation()
    {
        //終了した編隊数をカウント
        this.finishedFormationCount++;

        //全編隊が終了したら
        if (this.finishedFormationCount >= this.formationDataControllers.Count)
        {
            //終了を通知
            this.onFinished?.Invoke();
        }
    }

    /// <summary>
    /// 生きている魚がいるかどうか
    /// </summary>
    public bool HasAlivedFish()
    {
        return this.formationDataControllers.Exists(x => x.HasAlivedFish());
    }

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        writer.Write(this.delay);
        writer.Write(this.finishedFormationCount);

        for (int i = 0; i < this.formationDataControllers.Count; i++)
        {
            (this.formationDataControllers[i] as IBinary).Write(writer);
        }
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        this.delay = reader.ReadSingle();
        this.finishedFormationCount = reader.ReadInt32();

        for (int i = 0; i < this.formationDataControllers.Count; i++)
        {
            (this.formationDataControllers[i] as IBinary).Read(reader);
        }
    }

}//class FishWaveManager

}//namespace Battle