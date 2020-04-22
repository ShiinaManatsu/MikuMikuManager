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
    using System.Threading.Tasks;

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

        #region Private state
        private SortType SortType { get; set; } = SortType.ByDefault;
        private string SearchPattern = "";
        private List<MMDObject> mMDObjects;
        #endregion

        protected override bool wantKeepAlive => true;
        private readonly string alertTitle = "感谢尝试Alpha版本的MikuMikuManager!";
        private readonly string alertContent = "当前预览需要配合MikuMikuPreview或者将预览文件命名为\"模型名字\".pmx.png并与pmx文件放在同一目录";


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
                        children: CardFilter()
                ));
            }
        }

        private Widget BuildCard(MMDObject x) => new MMDCardWidget(x, (m, b) =>
        {
            if (mounted) { setState(() => mMDObjects.Find(o => o.FilePath == m.FilePath).IsFavored.Value = b); }
        });

        private List<Widget> CardFilter()
        {
            if (SearchPattern != "")
            {
                var s = SearchPattern.Trim().Split(' ');
                return SortType == SortType.ByDefault ?
                            mMDObjects.Where(x => s.Any(p => x.FileName.Contains(p))).OrderBy(x => x.FilePath).Select(x => BuildCard(x)).ToList()
                            : mMDObjects.Where(x => s.Any(p => x.FileName.Contains(p))).OrderByDescending(x => x.IsFavored.Value).Select(x => BuildCard(x)).ToList();
            }
            else
            {
                return SortType == SortType.ByDefault ?
                            mMDObjects.OrderBy(x => x.FilePath).Select(x => BuildCard(x)).ToList()
                            : mMDObjects.OrderByDescending(x => x.IsFavored.Value).Select(x => BuildCard(x)).ToList();
            }
        }

        /// <summary>
        /// The initState.
        /// </summary>
        public override void initState()
        {
            base.initState();
            var observeCountChanged = MMMServices.Instance.ObservedMMDObjects.ObserveCountChanged(true).Select(x=>true);
            var observeMove = MMMServices.Instance.ObservedMMDObjects.ObserveMove().Select(x => true);

            #region Subscription

            // Objects event
            var mmm = MMMServices.Instance;

            var observeEvery = mmm.ObservedMMDObjects.ObserveEveryValueChanged(x => x).Select(x => true);
            var observeCountChanged = mmm.ObservedMMDObjects.ObserveCountChanged(true).Select(x => true);
            var observeMove = mmm.ObservedMMDObjects.ObserveMove().Select(x => true);

            countChanged = observeCountChanged.Merge(observeMove, observeEvery)
                .Subscribe(_ => setState(() => mMDObjects = mmm.ObservedMMDObjects.ToList())
                , onError: e => Debug.Log(e.Message));

            // Sort event
            MMMFlutterApp.SortTypeProperty
                .DistinctUntilChanged()
                .Subscribe(x => setState(() => SortType = x), onError: e => Debug.Log(e.Message));

            // Search pattern changed event
            MMMFlutterApp.SearchPattern
                .Throttle(TimeSpan.FromMilliseconds(100))
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    using (WindowProvider.of(GameObject.Find("Panel")).getScope())
                    {
                        setState(() => SearchPattern = x);
                    }
                });


            Task.Run(() =>
            {
                Task.Delay(TimeSpan.FromSeconds(3));
                Unity.UIWidgets.material.DialogUtils.showDialog(
                    context: context,
                    barrierDismissible: true,
                    builder: (c) => new AlertDialog(
                             title: new Text(alertTitle),
                             content: new Text(alertContent),
                             actions: new List<Widget>
                             {
                                new RaisedButton(child:new Text("Close",style:new TextStyle(color:Colors.white)),onPressed:()=>Navigator.of(c).pop())
                             }
                         ));
            });
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