using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Battle {

/// <summary>
/// 砲台制御インターフェース
/// </summary>
public interface ITurretController
{
    void Run(float deltaTime);
}

}//namespace Battle