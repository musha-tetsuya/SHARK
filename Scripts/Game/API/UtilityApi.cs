using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Utility系API
public class UtilityApi
{
    /// <summary>
    /// パーツインベントリ拡張ResponseDataのlockFlg
    /// </summary>
    public class ExpansionCannonPossessionResponseData
    {
        public TUsers tUsers;
        public TutilityData tUtility;
        public UserGemData tGem;
    }

    /// <summary>
    /// ギアインベントリ拡張ResponseDataのlockFlg
    /// </summary>
    public class ExpansionGearPossessionResponseData
    {
        public TUsers tUsers;
        public TutilityData tUtility;
        public UserGemData tGem;
    }

    /// <summary>
    /// 砲台枠拡張ResponseData
    /// </summary>
    public class ExpansionCannonResponseData
    {
        public TUsers tUsers;
        public TutilityData tUtility;
        public UserTurretData tCannonSetting;
        public UserGemData tGem;
    }

    /// <summary>
    /// パーツインベントリから、所持制限拡張通信
    /// </summary>
    public static void CallExpansionCannonApi(Action onCompleted)
    {
        var request = new SharkWebRequest<ExpansionCannonPossessionResponseData>("utility/maxCannonPossession");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.onSuccess = (response) =>
        {
            UserData userData = UserData.Get();
            userData.Set(response.tUsers);
            userData.Set(response.tGem);
            userData.Set(response.tUtility);
            SharedUI.Instance.header.SetInfo(userData);

            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// ギアインベントリから、所持制限拡張通信
    /// </summary>
    public static void CallExpansionGearApi(Action onCompleted)
    {
        var request = new SharkWebRequest<ExpansionGearPossessionResponseData>("utility/maxGearPossession");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.onSuccess = (response) =>
        {  
            UserData userData = UserData.Get();
            userData.Set(response.tUsers);
            userData.Set(response.tGem);
            userData.Set(response.tUtility);
            SharedUI.Instance.header.SetInfo(userData);

            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// 砲台枠、所持制限拡張通信
    /// </summary>
    public static void CallExpansionCannonSetApi(Action onCompleted)
    {
        var request = new SharkWebRequest<ExpansionCannonResponseData>("utility/maxCannonSetting");
        
        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.onSuccess = (response) =>
        {
            UserData userData = UserData.Get();
            userData.Set(response.tUsers);
            userData.Set(response.tGem);
            userData.Set(response.tUtility);
            SharedUI.Instance.header.SetInfo(userData);
            
            // 追加された砲台を砲台リストに適用
            List<UserTurretData> turretDataList = userData.turretData.ToList();
            turretDataList.Add(response.tCannonSetting);
            userData.turretData = turretDataList.ToArray();
            onCompleted?.Invoke();
        };

        request.Send();
    }
}
