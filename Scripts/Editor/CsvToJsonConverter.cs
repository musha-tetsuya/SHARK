using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor;
using Microsoft.VisualBasic.FileIO;

/// <summary>
/// CSVからJsonへの変換
/// </summary>
public class CsvToJsonConverter : EditorWindow
{
    /// <summary>
    /// 入力元のEditorPrefsキー
    /// </summary>
    private const string INPUT_PATH_KEY = "CsvToJsonConverter.InputPath";
    /// <summary>
    /// 出力先のEditorPrefsキー
    /// </summary>
    private const string OUTPUT_PATH_KEY = "CsvToJsonConverter.OutputPath";
    /// <summary>
    /// 入力元
    /// </summary>
    private string inputPath = null;
    /// <summary>
    /// 出力先
    /// </summary>
    private string outputPath = null;

    /// <summary>
    /// GUIウィンドウを開く
    /// </summary>
    [MenuItem("Tools/Csv To Json Converter")]
    private static void Open()
    {
        EditorWindow.GetWindow<CsvToJsonConverter>();
    }

    /// <summary>
    /// OnEnable
    /// </summary>
    private void OnEnable()
    {
        this.inputPath = EditorPrefs.GetString(INPUT_PATH_KEY, "");
        this.outputPath = EditorPrefs.GetString(OUTPUT_PATH_KEY, "");
    }

    /// <summary>
    /// OnGUI
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.TextField("Input", this.inputPath);

            if (GUILayout.Button("選択", GUILayout.ExpandWidth(false)))
            {
                this.inputPath = EditorUtility.OpenFolderPanel("CSVフォルダの選択", this.inputPath, "");
                EditorPrefs.SetString(INPUT_PATH_KEY, this.inputPath);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.TextField("Output", this.outputPath);

            if (GUILayout.Button("選択", GUILayout.ExpandWidth(false)))
            {
                this.outputPath = EditorUtility.OpenFolderPanel("Json出力先選択", this.outputPath, "");
                EditorPrefs.SetString(OUTPUT_PATH_KEY, this.outputPath);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(this.inputPath) || string.IsNullOrEmpty(this.outputPath));
        {
            if (GUILayout.Button("Convert Csv To Json"))
            {
                Convert(this.inputPath, this.outputPath);
                this.MoveBuiltinJson();
                AssetDatabase.Refresh();
            }
        }
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// CSVからJsonへ変換
    /// </summary>
    public static void Convert(string inputPath, string outputPath)
    {
        var csvFiles = Directory.GetFiles(inputPath, "*.csv");

        foreach (var csvFile in csvFiles)
        {
            var dataList = new List<Dictionary<string, object>>();

            using (var parser = new TextFieldParser(new StreamReader(csvFile)))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                string[] properties = parser.ReadFields();

                while (!parser.EndOfData)
                {
                    string[] items = parser.ReadFields();

                    var data = new Dictionary<string, object>();
                    for (int x = 0; x < items.Length; x++)
                    {
                        if (string.IsNullOrEmpty(items[x]) || items[x].Equals("null"))
                        {
                            data.Add(properties[x], null);
                        }
                        else
                        {
                            decimal num;
                            if (decimal.TryParse(items[x], out num))
                            {
                                data.Add(properties[x], num);
                            }
                            else
                            {
                                data.Add(properties[x], items[x]);
                            }
                        }
                    }
                    dataList.Add(data);
                }
            }

            string jsonFileName = string.Format("{0}/{1}.json", outputPath, Path.GetFileNameWithoutExtension(csvFile));
            string json = JsonConvert.SerializeObject(dataList, Formatting.Indented);
            File.WriteAllText(jsonFileName, json);
        }

        Debug.Log("Success Convert Csv To Json");
    }

	/// <summary>
	/// ビルトインJsonファイルの移動
	/// </summary>
	private void MoveBuiltinJson()
	{
		if (this.outputPath.Contains("BuiltinResources"))
		{
			return;
		}

		//対象Json
		string[] targetFileName =
		{
			"BuiltinLocalizeTextData.json"
		};

		//ビルトインJsonフォルダ
		string builtinJsonDirectory = Path.Combine(Application.dataPath, "Sunchoi/BuiltinResources/Resources/Json");

		foreach (var fileName in targetFileName)
		{
			string path = Path.Combine(this.outputPath, fileName);

			if (File.Exists(path))
			{
				string destPath = Path.Combine(builtinJsonDirectory, fileName);

				//移動先に存在しているなら削除
				if (File.Exists(destPath))
				{
					File.Delete(destPath);
				}

				//移動
				File.Move(path, destPath);
			}
		}
	}
}
