using System;
using System.Collections.Generic;
using System.Linq;

public class MissionApi
{
    /// <summary>
    /// ミッションカテゴリ
    /// </summary>
    public enum Category
    {
        None      = 0,//なし
        Total     = 1,//通算
        Daily     = 2,//デイリー
        Event     = 3,//イベント
        StartDash = 4,//スタートダッシュ
    }

    /// <summary>
    /// ミッション状態
    /// </summary>
    public enum Status
    {
        ClearNotReceived,//クリア済み未受け取り
        NotClear,        //未クリア
        ClearReceived,   //受け取り済み
    }

    /// <summary>
    /// ミッション進捗
    /// </summary>
    public class MissionProgress
    {
        public Category category;
        public Status status;
        public uint tMissionId;
        public uint missionTypeId;
        public uint? missionTypeSubId;
        public uint missionRewardId;
        public uint clearCondition;
        public uint missionCount;
        public uint? endTime;
    }

    /// <summary>
    /// ミッション進捗グループ
    /// </summary>
    public class MissionProgressGroup
    {
        public MissionProgress[] clearNotReceived;
        public MissionProgress[] notClear;
        public MissionProgress[] clearReceived;

        public void Setup(Category category)
        {
            foreach (var x in this.clearNotReceived)
            {
                x.category = category;
                x.status = Status.ClearNotReceived;
            }
            foreach (var x in this.notClear)
            {
                x.category = category;
                x.status = Status.NotClear;
            }
            foreach (var x in this.clearReceived)
            {
                x.category = category;
                x.status = Status.ClearReceived;
            }
        }
    }

    /// <summary>
    /// mission/missionProgressのレスポンスデータ
    /// </summary>
    public class MissionProgressResponseData
    {
        public MissionProgressGroup totalMission;
        public MissionProgressGroup dailyMission;
        public MissionProgressGroup startDashMissionProgress;
        public MissionProgressGroup eventMissionProgress;

        public void Setup()
        {
            this.totalMission?.Setup(Category.Total);
            this.dailyMission?.Setup(Category.Daily);
            this.eventMissionProgress?.Setup(Category.Event);
            this.startDashMissionProgress?.Setup(Category.StartDash);
        }
    }

    /// <summary>
    /// mission/getMissionRewardのレスポンスデータ
    /// </summary>
    public class MissionRewardResponseData : MissionProgressResponseData
    {
        public AddItem mMissionReward;
        public TUsers tUsers;
        public UserGemData tGem;
        public bool isMaxPossession;
    }

    /// <summary>
    /// ミッションリスト取得通信
    /// </summary>
    public static void CallMissionProgressApi(Action<MissionProgressResponseData> onCompleted)
    {
        var request = new SharkWebRequest<MissionProgressResponseData>("mission/missionProgress");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.onSuccess = (response) =>
        {
            response.Setup();
            onCompleted?.Invoke(response);
        };

        request.Send();
    }

    /// <summary>
    /// ミッションの報酬受け取り通信
    /// </summary>
    public static void CallGetMissionRewardApi(MissionProgress progress, Action<MissionRewardResponseData> onCompleted)
    {
        //リクエスト作成
        var request = new SharkWebRequest<MissionRewardResponseData>("mission/getMissionReward");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        //リクエストパラメータセット
        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "tMissionId", progress.tMissionId },
            { "missionGroupId", (uint)progress.category }
        });

        //通信完了時コールバック登録
        request.onSuccess = (response) =>
        {
            response.Setup();

            //アイテムの付与
            UserData.Get().AddItem((ItemType)response.mMissionReward.itemType, response.mMissionReward.itemId, response.mMissionReward.itemNum);

            //ジェムの同期
            if (response.tGem != null)
            {
                UserData.Get().Set(response.tGem);
            }

            //ユーザー情報の同期
            if (response.tUsers != null)
            {
                UserData.Get().Set(response.tUsers);
            }
            
            //ヘッダ更新
            SharedUI.Instance.header.SetInfo(UserData.Get());

            //無期限のプレゼントBoxの上限を超えているかのフラグ
            HomeScene.isMaxPossession = response.isMaxPossession;

            //通信完了
            onCompleted?.Invoke(response);
        };

        //通信開始
        request.Send();
    }
}
