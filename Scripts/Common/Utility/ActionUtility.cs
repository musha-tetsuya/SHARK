using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Action系ユーティリティ
/// </summary>
public static class ActionUtility
{
    /// <summary>
    /// EventTriggerへのアクション登録
    /// </summary>
    public static void AddAction(this EventTrigger eventTrigger, EventTriggerType eventTriggerType, UnityAction<BaseEventData> action)
    {
        var entry = new EventTrigger.Entry();
        entry.eventID = eventTriggerType;
        entry.callback = new EventTrigger.TriggerEvent();
        entry.callback.AddListener(action);
        eventTrigger.triggers.Add(entry);
    }

    /// <summary>
    /// WaitForSeconds後にコールバック処理する
    /// </summary>
    public static IEnumerator AddCallback(this WaitForSeconds obj, Action callback)
    {
        yield return obj;
        callback?.Invoke();
    }

    /// <summary>
    /// WaitWhile後にコールバック処理する
    /// </summary>
    public static IEnumerator AddCallback(this WaitWhile obj, Action callback)
    {
        yield return obj;
        callback?.Invoke();
    }

    /// <summary>
    /// WaitUntil後にコールバック処理する
    /// </summary>
    public static IEnumerator AddCallback(this WaitUntil obj, Action callback)
    {
        yield return obj;
        callback?.Invoke();
    }

    /// <summary>
    /// EndOfFrame後にコールバック処理する
    /// </summary>
    public static IEnumerator AddCallback(this WaitForEndOfFrame obj, Action callback)
    {
        yield return obj;
        callback?.Invoke();
    }

    /// <summary>
    /// アニメーション後にコールバック処理する
    /// </summary>
    public static IEnumerator AddCallback(this WaitAnimation obj, Action callback)
    {
        yield return obj;
        callback?.Invoke();
    }
}
