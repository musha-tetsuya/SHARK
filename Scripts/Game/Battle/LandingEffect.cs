using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 着弾エフェクト
/// </summary>
public class LandingEffect : TimePauseBehaviour
{
    /// <summary>
    /// アニメーター
    /// </summary>
    [SerializeField]
    private Animator animator = null;

    /// <summary>
    /// ステート
    /// </summary>
    private MyState state = null;

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        if (this.animator == null)
        {
            //アニメーターが無いなら時間で消滅するステート
            this.state = new TimeUpdateState{ landingEffect = this };
        }
        else
        {
            //アニメーターがあるなら、アニメ終了で消滅するステート
            this.state = new AnimatorUpdateState{ landingEffect = this };
        }
    }

    /// <summary>
    /// Update
    /// </summary>
    private void Update()
    {
        this.state.Update(Time.deltaTime);
    }

    /// <summary>
    /// 停止
    /// </summary>
    public override void Pause(System.IO.BinaryWriter writer)
    {
        base.Pause(writer);
        (this.state as ITimePause)?.Pause(writer);
    }

    /// <summary>
    /// 再開
    /// </summary>
    public override void Play(System.IO.BinaryReader reader)
    {
        base.Play(reader);
        (this.state as ITimePause)?.Play(reader);
    }

    /// <summary>
    /// ステート基底
    /// </summary>
    private abstract class MyState : StateBase
    {
        /// <summary>
        /// 着弾エフェクト
        /// </summary>
        public LandingEffect landingEffect = null;

        /// <summary>
        /// Update
        /// </summary>
        public virtual void Update(float deltaTime){}

        /// <summary>
        /// 終了
        /// </summary>
        public override void End()
        {
            //着弾エフェクト消滅
            Destroy(this.landingEffect.gameObject);
        }
    }

    /// <summary>
    /// 時間で消滅するステート
    /// </summary>
    private class TimeUpdateState : MyState
    {
        /// <summary>
        /// 消滅までの時間
        /// </summary>
        private float time = 0.4f;

        /// <summary>
        /// Update
        /// </summary>
        public override void Update(float deltaTime)
        {
            if (this.time <= 0f)
            {
                this.End();
            }
            this.time -= deltaTime;
        }
    }

    /// <summary>
    /// アニメ終了で消滅するステート
    /// </summary>
    private class AnimatorUpdateState : MyState, ITimePause
    {
        /// <summary>
        /// Update
        /// </summary>
        public override void Update(float deltaTime)
        {
            var state = this.landingEffect.animator.GetCurrentAnimatorStateInfo(0);
            if (state.normalizedTime >= 1f)
            {
                this.End();
            }
        }

        /// <summary>
        /// 一時停止時
        /// </summary>
        void ITimePause.Pause(BinaryWriter writer)
        {
            writer.Write(this.landingEffect.animator.enabled);
            this.landingEffect.animator.enabled = false;
        }

        /// <summary>
        /// 一時停止再開時
        /// </summary>
        void ITimePause.Play(BinaryReader reader)
        {
            this.landingEffect.animator.enabled = reader.ReadBoolean();
        }
    }
}