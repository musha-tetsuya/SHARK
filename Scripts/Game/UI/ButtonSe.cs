using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ボタンSE
/// </summary>
public class ButtonSe : MonoBehaviour
{
    /// <summary>
    /// ボタン
    /// </summary>
    [SerializeField]
    private Button button = null;
    /// <summary>
    /// SE名
    /// </summary>
    [SerializeField]
    public string seName = SeName.YES;

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        if (this.button != null)
        {
            this.button.onClick.AddListener(() =>
            {
                if (!string.IsNullOrEmpty(this.seName))
                {
                    SoundManager.Instance.PlaySe(this.seName);
                }
            });
        }
    }
}
