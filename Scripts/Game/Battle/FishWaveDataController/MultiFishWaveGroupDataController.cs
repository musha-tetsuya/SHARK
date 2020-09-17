using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle;

/// <summary>
/// マルチ用WAVEグループデータ制御
/// </summary>
public class MultiFishWaveGroupDataController : IBinary
{
    /// <summary>
    /// ステート
    /// </summary>
    private enum State
    {
        None,
        Random,
        Wave,
        Afterglow,
        Length
    }

    /// <summary>
    /// マスター
    /// </summary>
    private MultiFishWaveGroupData master = null;
    /// <summary>
    /// ステート
    /// </summary>
    private State state = State.None;
    /// <summary>
    /// 現在のアクティブなWAVE番号
    /// </summary>
    private int activeWaveNo = 0;
    /// <summary>
    /// WAVE開始までのDelay
    /// </summary>
    private float waveDelay = 0f;
    /// <summary>
    /// ステート別処理
    /// </summary>
    private System.Action<float>[] stateActions = new System.Action<float>[(int)State.Length];
    /// <summary>
    /// WAVE制御リスト
    /// </summary>
    private List<FishWaveDataController> fishWaveDataControllers = new List<FishWaveDataController>();
    /// <summary>
    /// ランダムルート制御：Lowレート
    /// </summary>
    private RandomFishRouteDataController lowRouteDataController = null;
    /// <summary>
    /// ランダムルート制御：Midレート
    /// </summary>
    private RandomFishRouteDataController midRouteDataController = null;
    /// <summary>
    /// ランダムルート制御：Highレート
    /// </summary>
    private RandomFishRouteDataController highRouteDataController = null;
    /// <summary>
    /// ランダムルート制御：SPレート
    /// </summary>
    private RandomFishRouteDataController spRouteDataController = null;
    /// <summary>
    /// ローダー
    /// </summary>
    public AssetListLoader loader = new AssetListLoader();

    /// <summary>
    /// construct
    /// </summary>
    public MultiFishWaveGroupDataController(MultiFishWaveGroupData master)
    {
        this.master = master;
        this.activeWaveNo = Random.Range(0, this.master.waveDatas.Count);

        this.stateActions[(int)State.None] = null;
        this.stateActions[(int)State.Random] = this.RandomStateUpdate;
        this.stateActions[(int)State.Wave] = this.WaveStateUpdate;
        this.stateActions[(int)State.Afterglow] = this.AfterglowStateUpdate;

        for (int i = 0; i < this.master.waveDatas.Count; i++)
        {
            var controller = new FishWaveDataController(
                id: new Fish.ID{ waveId = (byte)i },
                master: this.master.waveDatas[i].data,
                delay: 0f
            );
            controller.onFinished = this.OnFinishedWave;

            this.fishWaveDataControllers.Add(controller);
            this.loader.AddRange(controller.loader);
        }

        this.lowRouteDataController = new RandomFishRouteDataController(
            new Fish.ID{ routeId = 0 },
            this.master.randomLowRoute
        );
        this.midRouteDataController = new RandomFishRouteDataController(
            new Fish.ID{ routeId = 1 },
            this.master.randomMidRoute
        );
        this.highRouteDataController = new RandomFishRouteDataController(
            new Fish.ID{ routeId = 2 },
            this.master.randomHighRoute
        );
        this.spRouteDataController = new RandomFishRouteDataController(
            new Fish.ID{ routeId = 3 },
            this.master.randomSpRoute
        );

        this.loader.AddRange(this.lowRouteDataController.loader);
        this.loader.AddRange(this.midRouteDataController.loader);
        this.loader.AddRange(this.highRouteDataController.loader);
        this.loader.AddRange(this.spRouteDataController.loader);
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup()
    {
        this.lowRouteDataController.Setup();
        this.midRouteDataController.Setup();
        this.highRouteDataController.Setup();
        this.spRouteDataController.Setup();

        //ランダムステート開始
        this.waveDelay = this.master.waveDatas[this.activeWaveNo].delay;
        this.state = State.Random;
    }

    /// <summary>
    /// Update
    /// </summary>
    public void Update(float deltaTime)
    {
        this.stateActions[(int)this.state]?.Invoke(deltaTime);
    }

    /// <summary>
    /// ランダムステート
    /// </summary>
    private void RandomStateUpdate(float deltaTime)
    {
        this.lowRouteDataController.Update(deltaTime);
        this.midRouteDataController.Update(deltaTime);
        this.highRouteDataController.Update(deltaTime);
        this.spRouteDataController.Update(deltaTime);

        //時間カウント
        this.waveDelay -= deltaTime;

        //ランダムステート終了
        if (this.waveDelay <= 0f)
        {
            //LowとMidの魚ははける
            this.lowRouteDataController.OffStage();
            this.midRouteDataController.OffStage();

            //WAVE開始
            this.fishWaveDataControllers[this.activeWaveNo].Reset(0f);
            this.fishWaveDataControllers[this.activeWaveNo].Setup();
            this.state = State.Wave;
        }
    }

    /// <summary>
    /// WAVEステート
    /// </summary>
    private void WaveStateUpdate(float deltaTime)
    {
        this.fishWaveDataControllers[this.activeWaveNo].Update(deltaTime);
    }

    /// <summary>
    /// WAVE終了時
    /// </summary>
    private void OnFinishedWave()
    {
        //WAVE余韻ステートへ
        this.state = State.Afterglow;
    }

    /// <summary>
    /// WAVE余韻ステート
    /// </summary>
    private void AfterglowStateUpdate(float deltaTime)
    {
        this.fishWaveDataControllers[this.activeWaveNo].Update(deltaTime);

        //画面内にHigh, SP以外の魚がいなくなったら
        if (!this.fishWaveDataControllers[this.activeWaveNo].HasAlivedFish()
        &&  !this.lowRouteDataController.HasAlivedFish()
        &&  !this.midRouteDataController.HasAlivedFish())
        {
            //次のWAVE番号へ
            this.activeWaveNo++;
            this.activeWaveNo %= this.fishWaveDataControllers.Count;

            //ランダム生成のインターバルリセット
            this.lowRouteDataController.Reset();
            this.midRouteDataController.Reset();
            this.highRouteDataController.Reset();
            this.spRouteDataController.Reset();

            //ランダムステート開始
            this.waveDelay = this.master.waveDatas[this.activeWaveNo].delay;
            this.state = State.Random;
        }
    }

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        writer.Write((byte)this.state);
        writer.Write(this.activeWaveNo);
        writer.Write(this.waveDelay);
        (this.fishWaveDataControllers[this.activeWaveNo] as IBinary).Write(writer);
        (this.lowRouteDataController as IBinary).Write(writer);
        (this.midRouteDataController as IBinary).Write(writer);
        (this.highRouteDataController as IBinary).Write(writer);
        (this.spRouteDataController as IBinary).Write(writer);
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        this.state = (State)reader.ReadByte();
        this.activeWaveNo = reader.ReadInt32();
        this.waveDelay = reader.ReadSingle();
        this.fishWaveDataControllers[this.activeWaveNo].Setup();
        (this.fishWaveDataControllers[this.activeWaveNo] as IBinary).Read(reader);
        (this.lowRouteDataController as IBinary).Read(reader);
        (this.midRouteDataController as IBinary).Read(reader);
        (this.highRouteDataController as IBinary).Read(reader);
        (this.spRouteDataController as IBinary).Read(reader);
    }
}
