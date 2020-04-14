namespace MikuMikuManager.App
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System;
    using MikuMikuManager.Data;
    using MikuMikuManager.Services;
    using UniRx;
    using Unity.UIWidgets.material;
    using Unity.UIWidgets.widgets;
    using UnityEngine;
    using Material = Unity.UIWidgets.material.Material;
    using Unity.UIWidgets.painting;
    using Unity.UIWidgets.rendering;

    /// <summary>
    /// Defines the <see cref="HomePage" />.
    /// </summary>
    public class HomePage : StatefulWidget
    {
        /// <summary>
        /// The createState.
        /// </summary>
        /// <returns>The <see cref="State"/>.</returns>
        public override State createState() => new HomePageState();
    }

    /// <summary>
    /// Defines the <see cref="HomePageState" />.
    /// </summary>
    public class HomePageState : AutomaticKeepAliveClientMixin<HomePage>
    {
        /// <summary>
        /// Defines the countChanged.
        /// </summary>
        internal IDisposable countChanged;

        /// <summary>
        /// Defines the mMDObjects.
        /// </summary>
        internal List<MMDObject> mMDObjects;

        protected override bool wantKeepAlive => true;

        /// <summary>
        /// The build.
        /// </summary>
        /// <param name="context">The context<see cref="BuildContext"/>.</param>
        /// <returns>The <see cref="Widget"/>.</returns>
        public override Widget build(BuildContext context)
        {
            if (MMMServices.Instance.ObservedMMDObjects.Count == 0)
            {
                return new Center(child: new Text("Add watch folder in settings"));
            }
            else
            {
                return new Container(
                    child: GridView.count(
                        primary: false,
                        crossAxisSpacing: 8,
                        mainAxisSpacing: 8,
                        childAspectRatio: 0.5f,
                        crossAxisCount: (int)(MediaQuery.of(context).size.width / 230f),
                        children: mMDObjects.ConvertAll<Widget>(x => new MMDCardWidget(x))
                    )
                );
            }
        }

        /// <summary>
        /// The initState.
        /// </summary>
        public override void initState()
        {
            base.initState();
            countChanged = MMMServices.Instance.ObservedMMDObjects.ObserveCountChanged(true)
                .Do(x => Debug.Log($"Current count {x}"))
                .Subscribe(_ => setState(() => mMDObjects = MMMServices.Instance.ObservedMMDObjects.ToList()));
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public override void dispose()
        {
            base.dispose();
            countChanged.Dispose();
        }
    }
}