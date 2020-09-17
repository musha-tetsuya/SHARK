using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// トップAPI
/// </summary>
public class TopApi
{
    /// <summary>
    /// リソースバージョン取得通信
    /// </summary>
    public static void CallTopApi(Action<string> onCompleted)
    {
        var request = new SharkWebRequest<object>("top");

        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "deviceType", SharkDefine.DEVICE_TYPE },
        });

        request.onSuccess = (response) =>
        {
            onCompleted?.Invoke(request.resourceVersion);
        };

        request.Send();
    }
}
