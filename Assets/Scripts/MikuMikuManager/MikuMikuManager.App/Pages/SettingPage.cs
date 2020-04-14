namespace MikuMikuManager.App
{
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using MikuMikuManager.Services;
    using SFB;
    using UniRx;
    using Unity.UIWidgets.material;
    using Unity.UIWidgets.rendering;
    using Unity.UIWidgets.widgets;
    using UnityEngine;

    /// <summary>
    /// Defines the <see cref="SettingPage" />.
    /// </summary>
    public class SettingPage : StatefulWidget
    {
        /// <summary>
        /// The createState.
        /// </summary>
        /// <returns>The <see cref="State"/>.</returns>
        public override State createState() => new SettingPageState();
    }

    /// <summary>
    /// Defines the <see cref="SettingPageState" />.
    /// </summary>
    public class SettingPageState : AutomaticKeepAliveClientMixin<SettingPage>
    {
        /// <summary>
        /// Defines the chips.
        /// </summary>
        private List<Chip> chips = new List<Chip>();
        private IDisposable observeCountChanged;

        protected override bool wantKeepAlive => true;

        /// <summary>
        /// The build.
        /// </summary>
        /// <param name="context">The context<see cref="BuildContext"/>.</param>
        /// <returns>The <see cref="Widget"/>.</returns>
        public override Widget build(BuildContext context) => new Container(
            width: MediaQuery.of(context).size.width * 0.8f,
            child: new Column(
                mainAxisAlignment: MainAxisAlignment.start,
                mainAxisSize: MainAxisSize.max,
                children: new List<Widget> {
                    new Card (
                        child: new Column (
                            crossAxisAlignment : CrossAxisAlignment.start,
                            children : BuildColumn ()
                        )
                    )
                }
            )
        );

        /// <summary>
        /// The _buildColumn.
        /// </summary>
        /// <returns>The <see cref="List{Widget}"/>.</returns>
        private List<Widget> BuildColumn()
        {
            var list = new List<Widget>();
            list.Add(new Text("Watched Folders"));
            if (list.Count != 0) { list.AddRange(chips); }
            list.Add(new ButtonBar(children: new List<Widget> {
                new FlatButton (onPressed: () => {
                        var path = StandaloneFileBrowser.OpenFolderPanel ("Select Folder", "", false);
                        var trimed = path[0].Replace ('\\', '/');
                        if (!MMMServices.Instance.WatchedFolders.Contains (trimed)) {
                            MMMServices.Instance.WatchedFolders.Add (trimed);
                        }
                    },
                    child : new Text ("Add Folder"))
            }));
            return list;
        }

        private void Call() => Debug.Log("Call");

        /// <summary>
        /// The initState.
        /// </summary>
        public override void initState()
        {
            base.initState();
            observeCountChanged = MMMServices.Instance.WatchedFolders.ObserveCountChanged(true)
                .Subscribe(x => setState(() => chips = MMMServices.Instance.WatchedFolders.Select(f => new Chip(label: new Text(f), deleteIcon: new Icon(Icons.delete), onDeleted: () => MMMServices.Instance.WatchedFolders.Remove(f))).ToList()));
            Debug.Log(chips.Count);
        }

        public override void dispose()
        {
            base.dispose();
            Debug.Log("Disposed");
            observeCountChanged.Dispose();
        }
    }
}