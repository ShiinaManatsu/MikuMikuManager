//#define DEBUG

using MikuMikuManager.Data;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;

namespace MikuMikuManager.Services
{
    public class MMMServices : MonoBehaviour
    {
        public static GameObject container;

        public static Action RefreshObservedMmdObjects { get; set; }

        /// <summary>
        /// Static instances
        /// </summary>
        public static MMMServices Instance => container.GetComponent<MMMServices>();

        public ReactiveCollection<string> WatchedFolders { get; set; }

        public ReactiveCollection<MMDObject> ObservedMMDObjects { get; set; }

        public AppSettingsXML AppSettings { get; private set; }

        #region Private Members

        private ReactiveCollection<FileSystemWatcher> FileSystemWatchers { get; set; }

        #endregion


        private void Awake()
        {
            container = transform.gameObject;
            WatchedFolders = new ReactiveCollection<string>();
            ObservedMMDObjects = new ReactiveCollection<MMDObject>();
            ObservableSettings();
            SetupSettings();
            RefreshObservedMmdObjects += () =>
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

                ObservedMMDObjects.Clear();
                Debug.Log("Reload");
                foreach (var watchedFolder in WatchedFolders)
                {
                    GetMmdObjects(watchedFolder);
                }

                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            };
        }

        private void ObservableSettings()
        {
            // Handle add new
            // TODO: Add filter
            var watchNew = WatchedFolders.ObserveAdd();

            // Get mmd objects
            watchNew.Subscribe(x => GetMmdObjects(x.Value));

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

            Observable.Timer(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    GameObject.Find("MMDRenderer").GetComponent<PreviewBuilder.PreviewBuilder>().OnRenderComplete +=
                        () =>
                        {
                            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

                            ObservedMMDObjects.Clear();
                            Debug.Log("Reload");
                            foreach (var watchedFolder in WatchedFolders)
                            {
                                GetMmdObjects(watchedFolder);
                            }

                            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                        };
                });
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

        private void GetMmdObjects(string path)
        {
            var objects = Directory
                .EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(s =>
                    Path.GetExtension(s).EndsWith(".pmx", true, CultureInfo.CurrentCulture));

            foreach (var x in objects)
            {
                var obj = new MMDObject(x, x.Remove(x.LastIndexOf("\\")), path);
                ObservedMMDObjects.Add(obj);
            }
        }
    }
}