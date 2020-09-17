using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleReferenceCountTest : MonoBehaviour
{
    private AssetLoadHandle handle1 = null;
    private AssetLoadHandle handle2 = null;

    private void Start()
    {
        //testQuad1とtestQuad2は同じマテリアル「Materials/test.mat」を参照している。

        //testQuad1, testQuad2, test.matは３つバラバラのアセットバンドルになっているため、
        //testQuad1とtestQuad2からtest.matに対して依存関係が発生している。

        //test.matをロードしなければtestQuad1とtestQuad2は正しく表示されないが、
        //依存関係を自動でロードするので、わざわざtest.matをロードしなくても大丈夫。

        this.handle1 = AssetManager.Load<GameObject>("Prefabs/testQuad1", (asset) =>
        {
            Instantiate(asset);
        });

        this.handle2 = AssetManager.Load<GameObject>("Prefabs/testQuad2", (asset) =>
        {
            Instantiate(asset);
        });
    }

    private void OnGUI()
    {
        //testQuad1を破棄するときに依存関係であるtest.matも破棄しようとするが、
        //testQuad2が参照している間はtest.matは破棄されないようにしているため、
        //testQuad2のマテリアルがMissingになることはない。

        if (GUILayout.Button("Unload testQuad1"))
        {
            AssetManager.Unload(this.handle1);
        }

        if (GUILayout.Button("Unload testQuad2"))
        {
            AssetManager.Unload(this.handle2);
        }
    }
}
