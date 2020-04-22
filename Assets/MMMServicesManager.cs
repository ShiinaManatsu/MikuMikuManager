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
                .Timer(TimeSpan.FromSeconds(2))
                .Subscribe(_ => panel.GetComponent<MMMFlutterApp>().enabled = true);
        }
    }

}