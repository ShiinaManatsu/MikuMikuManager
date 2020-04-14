using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using MikuMikuManager.Data;
using System.IO;
using System.Linq;
using System.Globalization;
using System;

namespace MikuMikuManager.Services
{

    public class MMMServices
    {
        private static MMMServices instance = null;

        /// <summary>
        /// Static instances
        /// </summary>
        public static MMMServices Instance
        {
            get
            {
                if (instance == null)
                {
                    Instance = new MMMServices();
                    return instance;
                }
                else
                {
                    return instance;
                }
            }
            private set => instance = value;
        }

        public ReactiveCollection<string> WatchedFolders { get; set; }
        public ReactiveCollection<MMDObject> ObservedMMDObjects { get; set; }

        #region Private Members

        private ReactiveCollection<FileSystemWatcher> FileSystemWatchers { get; set; }

        #endregion

        public MMMServices()
        {
            WatchedFolders = new ReactiveCollection<string>();
            ObservedMMDObjects = new ReactiveCollection<MMDObject>();
            ObservableSettings();
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

            folderRemove.Subscribe(x => ObservedMMDObjects
                .Where(m => m.WatchedFolder == x.Value)
                .ToObservable()
                .Delay(TimeSpan.FromMilliseconds(100))
                .Subscribe(o =>
                {
                    var result = ObservedMMDObjects.Remove(o);
                    Debug.Log(result);
                }));

            //folderRemove.Select(x => FileSystemWatchers.Where(f => f.Path == x.Value).FirstOrDefault())
            //    .Do(x => x.Dispose())
            //    .Subscribe(x => FileSystemWatchers.Remove(x));


        }

        private void GetMMDObgects(string path)
        {
            var pmxs = Directory
                .EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(s => Path.GetExtension(s).EndsWith(".pmx", true, CultureInfo.CurrentCulture));

            pmxs.ToObservable()
                .Select(x => new MMDObject(x, x.Remove(x.LastIndexOf("/")), path))
                .Subscribe(x => ObservedMMDObjects.Add(x));
        }

        //public async void ScanFolder()
        //{

        //}

        //public async void LoadDB()
        //{

        //}

        //public async void SaveDB()
        //{

        //}
    }

}