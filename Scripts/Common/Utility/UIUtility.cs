using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class UIUtility
{
    public static GUIStyle CreateButtonStyle(float width, int fontSize)
    {
        float w = width / SharkDefine.SCREEN_WIDTH;
        float h = (float)fontSize / SharkDefine.SCREEN_HEIGHT;
#if UNITY_EDITOR
        var res = UnityEditor.UnityStats.screenRes.Split('x');
        width = int.Parse(res[0]) * w;
        fontSize = (int)(int.Parse(res[1]) * h);
#else
        width = Screen.width * w;
        fontSize = (int)(Screen.height * h);
#endif
        var style = new GUIStyle(GUI.skin.button);
        style.fixedWidth = width;
        style.stretchWidth = false;
        style.fontSize = fontSize;

        return style;
    }

    public static readonly Color32 increaseColor = new Color32(0, 255, 0, 255);
    public static readonly Color32 decreaseColor = new Color32(255, 0, 0, 255);

    public static int ToColorCode(this Color32 color)
    {
        return (color.r << 24)
             | (color.g << 16)
             | (color.b << 8)
             | (color.a << 0);
    }

    /// <summary>
    /// 指定した文字列の色を変更する
    /// </summary>
    public static string GetColorText(TextColorType colorType, string text)
    {
        string str = "";
        switch (colorType)
        {
            case TextColorType.None:
                return text;
            case TextColorType.IncreaseParam:
                str = string.Format("<color=#{0:x8}>{1}</color>", increaseColor.ToColorCode(), text);
                break;
            case TextColorType.DecreaseParam:
                str = string.Format("<color=#{0:x8}>{1}</color>", decreaseColor.ToColorCode(), text);
                break;
            case TextColorType.Alert:
                str = string.Format("<color=#{0:x8}>{1}</color>", decreaseColor.ToColorCode(), text);
                break;
        }
        return str;
    }
}
