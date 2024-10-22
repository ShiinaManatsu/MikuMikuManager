﻿using UnityEngine;

namespace MikuMikuManager.App
{
    using MikuMikuManager.Services;
    using SFB;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;
    using Unity.UIWidgets.material;
    using Unity.UIWidgets.painting;
    using Unity.UIWidgets.rendering;
    using Unity.UIWidgets.ui;
    using Unity.UIWidgets.widgets;

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
        private List<Widget> chips = new List<Widget>();

        private IDisposable observeCountChanged;

        protected override bool wantKeepAlive => false;

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
                children: new List<Widget>
                {
                    new CustomElevation(
                        child: new Card(
                        elevation:0,
                        child: new Padding(
                            padding:EdgeInsets.all(10),
                            child:new Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: BuildColumn())
                        )
                    ),
                        height:60,
                        padding:EdgeInsets.all(5)
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
            var list = new List<Widget>
            {
                new Text("Watched Folders")
            };

            if (list.Count != 0)
            {
                list.AddRange(chips);
            }

            list.Add(new ButtonBar(children: new List<Widget>
            {
                new FlatButton(onPressed: () =>
                    {
                        var path = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", false);
                        var trimmed = path[0]; //.Replace ('\\', '/');
                        if (!MMMServices.Instance.WatchedFolders.Contains(trimmed))
                        {
                            MMMServices.Instance.WatchedFolders.Add(trimmed);
                        }
                    },
                    child: new Text("Add Folder"))
            }));
            return list;
        }

        /// <summary>
        /// The initState.
        /// </summary>
        public override void initState()
        {
            base.initState();
            var specifiedChanged = MMMServices.Instance.SpecifiedMmdObjects.ObserveCountChanged(true);
            var folderChanged = MMMServices.Instance.WatchedFolders.ObserveCountChanged(true);

            observeCountChanged = specifiedChanged.Merge(folderChanged)
                .Subscribe(x =>
                {
                    using (WindowProvider.of(GameObject.Find("Panel")).getScope())
                    {
                        setState(() =>
                        {
                            // Add specified pmx chip
                            var specified = MMMServices.Instance.SpecifiedMmdObjects
                                .Select(f => new CustomElevation(
                                    child: new Chip(
                                        label: new Text(f.FileName),
                                        deleteIcon: new Icon(Icons.delete),
                                        onDeleted: () => MMMServices.Instance.SpecifiedMmdObjects.Remove(f),
                                        backgroundColor: Colors.white,
                                        deleteIconColor: Colors.red,
                                        shape: new RoundedRectangleBorder(borderRadius: BorderRadius.circular(17))
                                    ),
                                    height: 30,
                                    padding: EdgeInsets.symmetric(horizontal: 6)
                                    ))
                                .ToList();

                            // Add folder chip
                            var folders = MMMServices.Instance.WatchedFolders
                                .Select(f => new CustomElevation(
                                    child: new Chip(
                                        label: new Text(f.Split('\\').Last()),
                                        deleteIcon: new Icon(Icons.delete),
                                        onDeleted: () => MMMServices.Instance.WatchedFolders.Remove(f),
                                        backgroundColor: Colors.white,
                                        deleteIconColor: Colors.red,
                                        shape: new RoundedRectangleBorder(borderRadius: BorderRadius.circular(17))
                                    ),
                                    height: 30,
                                    padding: EdgeInsets.symmetric(horizontal: 6)
                                    ))
                                .ToList();

                            chips.Clear();
                            chips.AddRange(specified);
                            chips.AddRange(folders);
                        });
                    }
                });
        }

        public override void dispose()
        {
            base.dispose();
            observeCountChanged.Dispose();
        }
    }
}