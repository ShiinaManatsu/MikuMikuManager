using MikuMikuManager.App;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MikuMikuManager.Services
{
    public class MMMServicesManager : MonoBehaviour
    {
        public GameObject panel;

        void Start()
        {
            Observable
                .Timer(TimeSpan.FromSeconds(1))
                .Subscribe(_ => panel.GetComponent<MMMFlutterApp>().enabled = true);
#if !UNITY_EDITOR
            SceneManager.LoadScene(1, LoadSceneMode.Additive);
#endif
        }
    }
}