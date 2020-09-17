using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class StateDataManager : MonoBehaviour
{

    static GameObject _container;
    static GameObject Container
    {
        get
        {
            return _container;
        }
    }

    static StateDataManager _instance;
    public static StateDataManager Instance
    {
        get
        {
            if (!_instance)
            {
                _container = new GameObject();
                _container.name = "DataController";
                _instance = _container.AddComponent(typeof(StateDataManager)) as StateDataManager;
                DontDestroyOnLoad(_container);
            }
            return _instance;
        }
    }
}