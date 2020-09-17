using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステート基礎
/// </summary>
public abstract class StateBase
{
    public StateManager manager = null;
    public Action onPop = null;
    public virtual void Start(){}
    public virtual void End(){}
}

/// <summary>
/// ステート管理
/// </summary>
public class StateManager
{
    /// <summary>
    /// ステートリスト
    /// </summary>
    private List<StateBase> stateList = new List<StateBase>();
    /// <summary>
    /// スタック中ステート
    /// </summary>
    private Stack<StateBase> stateStack = new Stack<StateBase>();
    /// <summary>
    /// 現在のステート
    /// </summary>
    public StateBase currentState { get; private set; }
    /// <summary>
    /// 前のステート
    /// </summary>
    public StateBase prevState { get; private set; }

    /// <summary>
    /// ステート追加
    /// </summary>
    public void AddState(StateBase state)
    {
        state.manager = this;
        this.stateList.Add(state);
    }

    /// <summary>
    /// 現在のステートを変更
    /// </summary>
    private void SetCurrentState(StateBase state)
    {
        this.prevState = this.currentState;
        this.currentState = state;
    }

    /// <summary>
    /// ステート切替
    /// </summary>
    public void ChangeState(Type type)
    {
        this.currentState?.End();
        this.SetCurrentState(this.GetState(type));
        this.currentState.Start();
    }

    /// <summary>
    /// ステート切替
    /// </summary>
    public void ChangeState<T>() where T : StateBase
    {
        this.ChangeState(typeof(T));
    }

    /// <summary>
    /// 現在のステートをスタックしてステート切替
    /// </summary>
    public void PushState(Type type, Action onPop)
    {
        this.currentState.onPop = onPop;
        this.stateStack.Push(this.currentState);
        this.SetCurrentState(this.GetState(type));
        this.currentState.Start();
    }

    /// <summary>
    /// 現在のステートをスタックしてステート切替
    /// </summary>
    public void PushState<T>(Action onPop) where T : StateBase
    {
        this.PushState(typeof(T), onPop);
    }

    /// <summary>
    /// スタックしてるステートに戻る
    /// </summary>
    public void PopState()
    {
        this.currentState.End();
        this.SetCurrentState(this.stateStack.Pop());
        this.currentState.onPop?.Invoke();
    }

    /// <summary>
    /// タイプに一致するステートを取得
    /// </summary>
    public StateBase GetState(Type type)
    {
        return this.stateList.Find(x => x.GetType() == type);
    }

    /// <summary>
    /// タイプに一致するステートを取得
    /// </summary>
    public T GetState<T>() where T : StateBase
    {
        return this.GetState(typeof(T)) as T;
    }
}

public interface ITimeStampUpdate
{
    void Update(float deltaTime, double timeStamp);
}