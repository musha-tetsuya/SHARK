using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

namespace Battle {

/// <summary>
/// 編隊イベント制御クラス
/// </summary>
public class FormationEventDataController : IBinary
{
    /// <summary>
    /// イベントデータリスト
    /// </summary>
    private List<FishFormationEventData> eventDatas = null;
    /// <summary>
    /// イベント発火フラグ
    /// </summary>
    private BitVector32[] isFired = null;
    /// <summary>
    /// 時間カウント
    /// </summary>
    private float time = 0f;
    /// <summary>
    /// イベントレシーバー
    /// </summary>
    private IFormationEventReceiver eventReceiver = null;

    /// <summary>
    /// construct
    /// </summary>
    public FormationEventDataController(List<FishFormationEventData> eventDatas, IFormationEventReceiver eventReceiver)
    {
        this.eventDatas = eventDatas;
        this.isFired = new BitVector32[eventDatas.Count / 32 + 1];
        this.eventReceiver = eventReceiver;
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset()
    {
        this.isFired = new BitVector32[this.eventDatas.Count / 32 + 1];
        this.time = 0f;
    }

    /// <summary>
    /// Update
    /// </summary>
    public void Update(float deltaTime)
    {
        for (int i = 0, imax = this.eventDatas.Count; i < imax; i++)
        {
            //発火済みイベントなら何もしない
            if (this.isFired.Get(i))
            {
                continue;
            }

            //発火タイミングを迎えたら
            if (this.time >= this.eventDatas[i].time)
            {
                //発火フラグON
                this.isFired.Set(i, true);

                switch (this.eventDatas[i].eventType)
                {
                    //再生イベント
                    case FishFormationEventData.EventType.Play:
                    {
                        this.eventReceiver.OnPlay();
                    }
                    break;

                    //一時停止イベント
                    case FishFormationEventData.EventType.Pause:
                    {
                        this.eventReceiver.OnPause();
                    }
                    break;

                    case FishFormationEventData.EventType.Quit:
                    {
                        this.eventReceiver.OnQuit();
                    }
                    break;
                }
            }
        }

        //時間カウント
        this.time += deltaTime;
    }

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        for (int i = 0; i < this.isFired.Length; i++)
        {
            writer.Write(this.isFired[i].Data);
        }
        writer.Write(this.time);
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        for (int i = 0; i < this.isFired.Length; i++)
        {
            this.isFired[i] = new BitVector32(reader.ReadInt32());
        }
        this.time = reader.ReadSingle();
    }

}//class FormationEventDataController

}//namespace Battle