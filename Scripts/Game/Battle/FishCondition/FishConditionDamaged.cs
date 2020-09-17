using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// 状態異常：非ダメ
/// </summary>
public sealed class FishConditionDamaged : FishCondition
{
    /// <summary>
    /// 効果時間
    /// </summary>
    private float time = 0.1f;

    /// <summary>
    /// construct
    /// </summary>
    public FishConditionDamaged()
        : base(FishConditionType.Damaged)
    {
        this.canMove = true;
    }

    /// <summary>
    /// construct
    /// </summary>
    public FishConditionDamaged(System.IO.BinaryReader reader)
        : base(reader)
    {
    }

    /// <summary>
    /// Start
    /// </summary>
    public override void Start()
    {
        base.Start();
        this.manager.fish.RefleshForm();

        if (this.manager.fish.master.isBoss == 0)
        {
            //ボスじゃなければ被弾時アニメ再生
            this.manager.fish.animator.Play("hirumi", 0, 0f);
        }
    }

    /// <summary>
    /// End
    /// </summary>
    public override void End()
    {
        base.End();
        this.manager.fish.RefleshForm();
    }

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

    /// <summary>
    /// バイナリデータ書き出し
    /// </summary>
    public override void Write(System.IO.BinaryWriter writer)
    {
        base.Write(writer);
        writer.Write(this.time);
    }

    /// <summary>
    /// バイナリデータ読み込み
    /// </summary>
    public override void Read(System.IO.BinaryReader reader)
    {
        base.Read(reader);
        this.time = reader.ReadSingle();
    }
}

}