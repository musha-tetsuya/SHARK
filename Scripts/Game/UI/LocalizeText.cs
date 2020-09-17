using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ローカライズテキスト
/// </summary>
[RequireComponent(typeof(Text))]
public class LocalizeText : MonoBehaviour
{
    /// <summary>
    /// テキスト
    /// </summary>
    [SerializeField]
    private Text text = null;

    /// <summary>
    /// キー
    /// </summary>
    [SerializeField]
    private string key = null;

    /// <summary>
    /// Reset
    /// </summary>
    private void Reset()
    {
        this.text = GetComponent<Text>();
    }

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        this.Set();
    }

    /// <summary>
    /// Set
    /// </summary>
    private void Set()
    {
        this.text.text = Masters.LocalizeTextDB.Get(this.key);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LocalizeText))]
    private class MyInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var t = this.target as LocalizeText;
            string beforeKey = t.key;

            base.OnInspectorGUI();

            if (beforeKey != t.key)
            {
                t.Set();
            }
        }
    }
#endif
}
