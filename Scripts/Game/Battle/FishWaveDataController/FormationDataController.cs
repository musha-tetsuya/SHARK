using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle {

/// <summary>
/// 編隊データ制御クラス
/// </summary>
public class FormationDataController : IFormationEventReceiver, IBinary
{
    /// <summary>
    /// ID
    /// </summary>
    private Fish.ID id;
    /// <summary>
    /// マスターデータ
    /// </summary>
    private FishFormationData master = null;
    /// <summary>
    /// Delay
    /// </summary>
    private float delay = 0f;
    /// <summary>
    /// 一時停止フラグ
    /// </summary>
    private bool isPause = false;
    /// <summary>
    /// 終了した回遊ルート数
    /// </summary>
    private int finishedRouteCount = 0;
    /// <summary>
    /// 回遊ルートデータ制御リスト
    /// </summary>
    private List<FishRouteDataController> routeDataControllers = new List<FishRouteDataController>();
    /// <summary>
    /// イベントデータ制御
    /// </summary>
    private FormationEventDataController eventDataController = null;
    /// <summary>
    /// ローダー
    /// </summary>
    public AssetListLoader loader = new AssetListLoader();
    /// <summary>
    /// Update処理
    /// </summary>
    private Action<float> onUpdate = null;
    /// <summary>
    /// 全回遊ルート終了時コールバック
    /// </summary>
    public Action onFinished = null;

    /// <summary>
    /// construct
    /// </summary>
    public FormationDataController(Fish.ID id, FishFormationData master, float delay)
    {
        this.id = id;
        this.master = master;
        this.delay = delay;
        this.eventDataController = new FormationEventDataController(this.master.eventDatas, this);

        for (int i = 0; i < this.master.routeDatas.Count; i++)
        {
            this.id.routeId = (byte)i;

            var controller = new FishRouteDataController(
                id: this.id,
                master: this.master.routeDatas[i].data,
                delay: this.master.routeDatas[i].delay
            );
            controller.onFinished = this.OnFinishedRoute;

            this.routeDataControllers.Add(controller);
            this.loader.AddRange(controller.loader);
        }
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset(float delay)
    {
        this.delay = delay;
        this.finishedRouteCount = 0;

        this.eventDataController.Reset();

        for (int i = 0; i < this.routeDataControllers.Count; i++)
        {
            this.routeDataControllers[i].Reset(this.master.routeDatas[i].delay);
        }
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup()
    {
        for (int i = 0; i < this.routeDataControllers.Count; i++)
        {
            this.routeDataControllers[i].Setup();
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

        //イベント更新
        this.eventDataController.Update(deltaTime);

        //一時停止状態ならreturn
        if (this.isPause)
        {
            return;
        }

        //回遊ルート更新
        for (int i = 0, imax = this.routeDataControllers.Count; i < imax; i++)
        {
            this.routeDataControllers[i].Update(deltaTime);
        }
    }

    /// <summary>
    /// 回遊ルート終了時
    /// </summary>
    private void OnFinishedRoute()
    {
        //終了した回遊ルート数をカウント
        this.finishedRouteCount++;

        //全回遊ルートが終了したら
        if (this.finishedRouteCount >= this.routeDataControllers.Count)
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
        return this.routeDataControllers.Exists(x => x.HasAlivedFish());
    }

    /// <summary>
    /// 編隊一時停止時
    /// </summary>
    void IFormationEventReceiver.OnPause()
    {
        this.isPause = true;

        for (int i = 0; i < this.routeDataControllers.Count; i++)
        {
            (this.routeDataControllers[i] as IFormationEventReceiver).OnPause();
        }
    }

    /// <summary>
    /// 編隊再開時
    /// </summary>
    void IFormationEventReceiver.OnPlay()
    {
        this.isPause = false;

        for (int i = 0; i < this.routeDataControllers.Count; i++)
        {
            (this.routeDataControllers[i] as IFormationEventReceiver).OnPlay();
        }
    }

    /// <summary>
    /// 編隊強制終了時
    /// </summary>
    void IFormationEventReceiver.OnQuit()
    {
        for (int i = 0; i < this.routeDataControllers.Count; i++)
        {
            (this.routeDataControllers[i] as IFormationEventReceiver).OnQuit();
        }
    }

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        writer.Write(this.delay);
        writer.Write(this.isPause);
        writer.Write(this.finishedRouteCount);

        for (int i = 0; i < this.routeDataControllers.Count; i++)
        {
            (this.routeDataControllers[i] as IBinary).Write(writer);
        }

        (this.eventDataController as IBinary).Write(writer);
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        this.delay = reader.ReadSingle();
        this.isPause = reader.ReadBoolean();
        this.finishedRouteCount = reader.ReadInt32();

        for (int i = 0; i < this.routeDataControllers.Count; i++)
        {
            (this.routeDataControllers[i] as IBinary).Read(reader);
        }

        (this.eventDataController as IBinary).Read(reader);
    }

}//class FormationDataController

}//namespace Battle