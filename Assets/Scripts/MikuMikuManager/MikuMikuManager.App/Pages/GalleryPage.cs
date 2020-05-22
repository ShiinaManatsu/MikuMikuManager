namespace MikuMikuManager.App
{
    using Data;
    using Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UniRx;
    using Unity.UIWidgets.material;
    using Unity.UIWidgets.painting;
    using Unity.UIWidgets.widgets;
    using UnityEngine;

    /// <summary>
    /// Defines the <see cref="GalleryPage" />.
    /// </summary>
    public class GalleryPage : StatefulWidget
    {
        /// <summary>
        /// The createState.
        /// </summary>
        /// <returns>The <see cref="State"/>.</returns>
        public override State createState() => new GalleryPageState();
    }

    /// <summary>
    /// Defines the <see cref="GalleryPageState" />.
    /// </summary>
    public class GalleryPageState : AutomaticKeepAliveClientMixin<GalleryPage>
    {
        /// <summary>
        /// Defines the countChanged.
        /// </summary>
        private IDisposable countChanged;

        #region Private state

        private SortType SortType { get; set; } = SortType.ByDefault;
        private string SearchPattern = "";
        private List<MMDObject> mMDObjects;

        #endregion

        protected override bool wantKeepAlive => true;

        #region Tip string

        private readonly string alertTitle = "感谢尝试Alpha版本的MikuMikuManager!";
        private readonly string alertContent = "当前预览需要配合MikuMikuPreview或者将预览文件命名为\"模型名字\".pmx.png并与pmx文件放在同一目录";

        #endregion

        /// <summary>
        /// The build.
        /// </summary>
        /// <param name="context">The context<see cref="BuildContext"/>.</param>
        /// <returns>The <see cref="Widget"/>.</returns>
        public override Widget build(BuildContext context)
        {
            if (MMMServices.Instance.ObservedMMDObjects.Count == 0 && MMMServices.Instance.SpecifiedMmdObjects.Count == 0)
            {
                return new Center(child: new Text("Add watch folder in settings"));
            }

            return new Scrollbar(
                child: GridView.count(
                    primary: false,
                    padding: EdgeInsets.only(bottom: 50),
                    crossAxisSpacing: 8,
                    mainAxisSpacing: 8,
                    childAspectRatio: 0.5f,
                    crossAxisCount: (int)(MediaQuery.of(context).size.width / 230f),
                    children: CardFilter()
                ));
        }

        private Widget BuildCard(MMDObject x) => new MMDCardWidget(x,
            (m, b) =>
            {
                if (mounted)
                {
                    setState(() => mMDObjects.Find(o => o.FilePath == m.FilePath).IsFavored.Value = b);
                }
            });

        private List<Widget> CardFilter()
        {
            if (SearchPattern != "")
            {
                var s = SearchPattern.Trim().Split(' ');
                return SortType == SortType.ByDefault
                    ? mMDObjects.Where(x => s.Any(p => x.FileName.Contains(p))).OrderBy(x => x.FilePath)
                        .Select(x => BuildCard(x)).ToList()
                    : mMDObjects.Where(x => s.Any(p => x.FileName.Contains(p)))
                        .OrderByDescending(x => x.IsFavored.Value).Select(x => BuildCard(x)).ToList();
            }
            else
            {
                return SortType == SortType.ByDefault
                    ? mMDObjects.OrderBy(x => x.FilePath).Select(x => BuildCard(x)).ToList()
                    : mMDObjects.OrderByDescending(x => x.IsFavored.Value).Select(x => BuildCard(x)).ToList();
            }
        }

        /// <summary>
        /// The initState.
        /// </summary>
        public override void initState()
        {
            mMDObjects = new List<MMDObject>();
            base.initState();

            #region Subscription

            // Objects event
            var mmm = MMMServices.Instance;

            var observeCountChanged = mmm.ObservedMMDObjects.ObserveCountChanged(true).Select(x => true);
            var observeEveryChanged = mmm.ObservedMMDObjects.ObserveEveryValueChanged(x => x).Select(x => true);
            var specifiedCountChanged = mmm.SpecifiedMmdObjects.ObserveCountChanged(true).Select(x => true);
            var specifiedChanged = mmm.SpecifiedMmdObjects.ObserveEveryValueChanged(x => x).Select(x => true);

            countChanged = observeCountChanged.Merge(observeEveryChanged, specifiedCountChanged, specifiedChanged)
                .Subscribe(_ =>
                    {
                        using (WindowProvider.of(GameObject.Find("Panel")).getScope())
                        {
                            var observed = mmm.ObservedMMDObjects.ToList();
                            var specified = mmm.SpecifiedMmdObjects.ToList();
                            setState(() =>
                            {
                                mMDObjects.Clear();
                                mMDObjects.AddRange(observed);
                                mMDObjects.AddRange(specified);
                            });
                        }
                    }
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

            #endregion

            #region Show tips

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
                            new RaisedButton(child: new Text("Close", style: new TextStyle(color: Colors.white)),
                                onPressed: () => Navigator.of(c).pop())
                        }
                    ));
            });

            #endregion
        }

        public override void didUpdateWidget(StatefulWidget oldWidget)
        {
            base.didUpdateWidget(oldWidget);
            Debug.Log(mMDObjects.Count);
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