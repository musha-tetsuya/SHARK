using UnityEngine;
using UnityEngine.UI;

public class ToggleManager : MonoBehaviour
{
    /// <summary>
    /// トグルオブジェクト
    /// </summary>
    [SerializeField]
    private Toggle toggle = null;
    /// <summary>
    /// トグル有効状態
    /// </summary>
    [SerializeField]
    private GameObject isOn = null;
    /// <summary>
    /// トグル無効状態
    /// </summary>
    [SerializeField]
    private GameObject isOff = null;

    /// <summary>
    /// 最初一回実行
    /// </summary>
    private void Awake()
    {
        IsOnCheck();
    }

    /// <summary>
    /// トグルのステータスが変更時に実行
    /// </summary>
    public void IsOnCheck()
    {
        // トグル状態がOnの場合
        if(this.toggle.isOn)
        {
            this.isOn.SetActive(true);
            this.isOff.SetActive(false);
        }
        // トグル状態がOffの場合
        else if(!this.toggle.isOn)
        {
            this.isOn.SetActive(false);
            this.isOff.SetActive(true);
        }
    }
}
