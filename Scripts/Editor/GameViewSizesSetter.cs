using UnityEditor;
using UnityEngine;
using Kyusyukeigo.Helper;

public static class GameViewSizesSetter
{
    static readonly GameViewSizeGroupType[] TargetGroupTypes = new []
    {
        GameViewSizeGroupType.Standalone,
        GameViewSizeGroupType.Android,
        GameViewSizeGroupType.iOS
    };

    [MenuItem("Tools/Add GameViewSizes")]
    private static void AddGameViewSizesMenu()
    {
        AddGameViewSizes();
    }

    static void AddGameViewSizes()
    {
        //AddGameViewSize("iPhone", 320, 480);
        //AddGameViewSize("iPhone 3G", 320, 480);
        //AddGameViewSize("iPhone 3GS", 320, 480);
        //AddGameViewSize("iPhone 4", 640, 960);
        //AddGameViewSize("iPhone 4S", 640, 960);
        //AddGameViewSize("iPhone 5", 640, 1136);
        //AddGameViewSize("iPhone 5s", 640, 1136);
        //AddGameViewSize("iPhone 5c", 640, 1136);

        // 拡大モード　ピクセルサイズ
        //AddGameViewSize("iPhone 6 Plus", 1125, 2001);
        //AddGameViewSize("iPhone 6s Plus", 1125, 2001);

        //AddGameViewSize("iPad Mini", 768, 1024);
        //AddGameViewSize("iPad Mini 2", 1536, 2048);
        //AddGameViewSize("iPad Mini 3", 1536, 2048);
        //AddGameViewSize("iPad Mini 4", 1536, 2048);
        //AddGameViewSize("iPad", 768, 1024);
        //AddGameViewSize("iPad 2", 768, 1024);
        //AddGameViewSize("iPad 3", 1536, 2048);
        //AddGameViewSize("iPad 4", 1536, 2048);
        //AddGameViewSize("iPad Air", 1536, 2048);
        //AddGameViewSize("iPad Air 2", 1536, 2048);

        AddGameViewSize("iPhone SE", 640, 1136);

        AddGameViewSize("iPhone 6,6s,7,8", 1334, 750);

        AddGameViewSize("iPhone 6,6s,7,8 Plus(pixel)", 2208, 1242);

        AddGameViewSize("iPhone X,Xs", 2436, 1125);
        AddGameViewSize("iPhone Xs Max", 2688, 1242);
        AddGameViewSize("iPhone Xr", 1792, 828);

        AddGameViewSize("iPhone 11", 1792, 828);
        AddGameViewSize("iPhone 11 Pro", 2436, 1125);
        AddGameViewSize("iPhone 11 Pro Max", 2688, 1242);

        AddGameViewSize("iPad Pro 9.7", 2048, 1536);
        AddGameViewSize("iPad Pro 12.9", 2732, 2048);

        AddGameViewSize("Android-suzuki", 2340, 1080);

        AddGameViewSize("Game Standard", 1920, 1080);


        /*
         * エディタスクリプト上からの変更だけでは設定保存が行われず、Unityを終了すると元に戻ってしまう。
         * どれか一つでもよいので登録した解像度設定を消すことで、設定を保存しているファイルに更新がかかるようなので、
         * そのための【削除(10x10)】を追加して各々手入力で消してもらうようにする
         */
        AddGameViewSize("削除", 0, 0);
        Debug.LogWarning("設定が保存されていません。設定保存のため、Gameビューの解像度変更で【削除(10x10)】を削除してください。");
    }

    static void AddGameViewSize(string baseText, int width, int height)
    {
        //Debug.Log("AddGameViewSize : " + baseText + ", width = " + width + ", height = " + height);

        foreach (var groupType in TargetGroupTypes)
        {
            Kyusyukeigo.Helper.GameViewSizeHelper.AddCustomSize(
                groupType,
                new GameViewSizeHelper.GameViewSize
                {
                    baseText = baseText,
                    type = GameViewSizeHelper.GameViewSizeType.FixedResolution,
                    width = width,
                    height = height
                });
        }
    }
}
 