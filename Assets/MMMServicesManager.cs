using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using MikuMikuManager.App;

namespace MikuMikuManager.Services
{
    public class MMMServicesManager : MonoBehaviour
    {
        public GameObject panel;
        void Start()
        {
            Observable
                .Return(true)
                .Delay(TimeSpan.FromSeconds(3))
                .Subscribe(_ => panel.AddComponent<MMMFlutterApp>());
        }
    }

}