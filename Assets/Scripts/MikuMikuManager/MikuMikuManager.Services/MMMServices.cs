//#define DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using MikuMikuManager.Data;
using System.IO;
using System.Linq;
using System.Globalization;
using System;
using UniRx.Diagnostics;

namespace MikuMikuManager.Services
{

    public class MMMServices : MonoBehaviour
    {
        private static MMMServices instance = null;
        private static GameObject container;

        /// <summary>
        /// Static instances
        /// </summary>
        public static MMMServices Instance
        {
            get
            {
                if (instance is null)
                {
                    Instance = container.AddComponent<MMMServices>();
                    return instance;
                }
                else
                {
                    return instance;
                }
            }
            set => instance = value;
        }

        public ReactiveCollection<string> WatchedFolders { get; set; }

        public ReactiveCollection<MMDObject> ObservedMMDObjects { get; set; }

        public AppSettingsXML AppSettings { get; private set; }

        #region Private Members

        private ReactiveCollection<FileSystemWatcher> FileSystemWatchers { get; set; }

        #endregion

        public MMMServices()
        {
            WatchedFolders = new ReactiveCollection<string>();
            ObservedMMDObjects = new ReactiveCollection<MMDObject>();
        }

        private void Start()
        {
            container = transform.gameObject;
            ObservableSettings();
            SetupSettings();
        }

        private void ObservableSettings()
        {
            // Handle add new
            // TODO: Add filter
            var watchNew = WatchedFolders.ObserveAdd();

            // Get mmd objects
            watchNew.Subscribe(x => GetMMDObgects(x.Value));

            // Handle watch events
            //watchNew.Select(x => new FileSystemWatcher(x.Value))
            //    .Do(x => x.Created += (_, __) => { })
            //    .Do(x => x.Changed += (_, __) => { })
            //    .Do(x => x.Deleted += (_, __) => { })
            //    .Subscribe(x => FileSystemWatchers.Add(x));

            // Handle remove
            var folderRemove = WatchedFolders.ObserveRemove();

            folderRemove
            .SelectMany(x => ObservedMMDObjects.ToList().Where(f => f.WatchedFolder == x.Value))
            .ForEachAsync(x => ObservedMMDObjects.Remove(x))
            .Subscribe(x => { }, onError: e => Debug.Log(e));

            //folderRemove.Select(x => FileSystemWatchers.Where(f => f.Path == x.Value).FirstOrDefault())
            //    .Do(x => x.Dispose())
            //    .Subscribe(x => FileSystemWatchers.Remove(x));


        }

        private void SetupSettings()
        {
            // Load at first time
            AppSettings = AppSettingsXML.LoadAppSettingsXML();
            if (AppSettings.WatchedFolders.Length > 0)
            {
                foreach (var s in AppSettings.WatchedFolders)
                {
                    WatchedFolders.Add(s);
                }
            }

            // Subscribe to event
            // Ignore the first time save
            WatchedFolders.ObserveCountChanged()
                .Do(_ => AppSettings.WatchedFolders = WatchedFolders.ToArray())
                .Subscribe(_ => AppSettings.SaveToXML());
        }

        private void GetMMDObgects(string path)
        {
            var pmxs = Directory
                .EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(s => Path.GetExtension(s).EndsWith(".pmx", true, CultureInfo.CurrentCulture));

            pmxs.ToObservable()
                .ForEachAsync(x =>
                    {
                        var obj = new MMDObject(x, x.Remove(x.LastIndexOf("\\")), path);
                        ObservedMMDObjects.Add(obj);
                    }
                )
                .Debug()
                .Subscribe(x => { }, onError: e => Debug.Log(e));
        }
    }

}