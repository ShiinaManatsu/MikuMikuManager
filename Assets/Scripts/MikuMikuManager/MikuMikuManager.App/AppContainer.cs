using MikuMikuManager.Services;
using System.Collections.Generic;
using UniRx;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace MikuMikuManager.App
{
    public class AppContainer : StatefulWidget
    {
        public AppContainer(ReactiveProperty<string> searchPattern, ReactiveProperty<SortType> sortTypeProperty)
        {
            SearchPattern = searchPattern;
            SortTypeProperty = sortTypeProperty;
        }

        public ReactiveProperty<string> SearchPattern { get; set; }
        public ReactiveProperty<SortType> SortTypeProperty { get; set; }

        public override State createState()
        {
            return new AppContainerState();
        }
    }

    internal class AppContainerState : State<AppContainer>
    {
        private string _renderStatus = "";
        private string _savingStatus = "";

        public override void initState()
        {
            base.initState();
            PreviewBuilder.PreviewBuilder.Instance.IsRendering
                .Subscribe(x =>
                {
                    using (WindowProvider.of(GameObject.Find("Panel")).getScope())
                    {
                        var s = x
                            ? $"Rendering - {PreviewBuilder.PreviewBuilder.Instance._currentMmdObject.FileName} {PreviewBuilder.PreviewBuilder.Instance.MmdObjects.Count} Remain"
                            : "waiting";
                        setState(() => _renderStatus = $"Render Status:{s}");
                    }
                });
            PreviewBuilder.PreviewBuilder.Instance.IsSaving
                .Subscribe(x =>
                {
                    using (WindowProvider.of(GameObject.Find("Panel")).getScope())
                    {
                        var s = x ? "Saving" : "waiting";
                        setState(() => _savingStatus = $"Render Status:{s}");
                    }
                });
        }

        public override Widget build(BuildContext context)
        {
            return new DefaultTabController(
                length: 2,
                child: new Scaffold(
                    extendBody: true,
                    drawer: new Drawer(
                        child: new FlatButton(
                            child: new Text("About"),
                            onPressed: () => Navigator.popAndPushNamed(context, Routes.AboutPageRouteText))),
                    appBar: new AppBar(
                        bottom: new TabBar(
                            tabs: new List<Widget>
                            {
                                new Container(height: 30, child: new Text("Home")),
                                new Container(height: 30, child: new Text("Settings"))
                                //,
                                //new Container (height: 30, child: new Text ("About")),
                            },
                            labelStyle: new TextStyle(fontSize: 24f),
                            indicatorSize: TabBarIndicatorSize.tab
                        ),
                        title: new Container(
                            child: new Row(
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                mainAxisSize: MainAxisSize.max,
                                children: new List<Widget>
                                {
                                    new Text("MikuMikuManager"),
                                    new Container(
                                        padding: EdgeInsets.all(8),
                                        width: Screen.width / 2,
                                        decoration: new BoxDecoration(
                                            Colors.white24,
                                            border: Border.all(Colors.white24),
                                            borderRadius: BorderRadius.circular(5)),
                                        child:
                                        new TextField(
                                            decoration: InputDecoration.collapsed(
                                                "Search..."
                                            ),
                                            autofocus: false,
                                            style: new TextStyle(fontSize: 17),
                                            onSubmitted: x => widget.SearchPattern.Value = x
                                        )),
                                    new PopupMenuButton<SortType>(
                                        child: new Icon(Icons.sort),
                                        itemBuilder: _ => new List<PopupMenuEntry<SortType>>
                                        {
                                            new PopupMenuItem<SortType>(
                                                value: SortType.ByDefault,
                                                child: new Text("By Default")
                                            ),
                                            new PopupMenuItem<SortType>(
                                                value: SortType.ByFavorite,
                                                child: new Text("By Favorite")
                                            )
                                        },
                                        initialValue: widget.SortTypeProperty.Value,
                                        onSelected: x => { widget.SortTypeProperty.Value = x; }
                                    )
                                }
                            )
                        )
                    ),
                    body: new Padding(
                        padding: EdgeInsets.all(8),
                        child: new TabBarView(
                            children: new List<Widget>
                            {
                                new HomePage(),
                                new SettingPage()
                            }
                        )
                    ),
                    floatingActionButton: new Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        mainAxisSize: MainAxisSize.min,
                        crossAxisAlignment: CrossAxisAlignment.end,
                        children: new List<Widget>
                        {
                            FloatingActionButton.extended(
                                icon: new Icon(Icons.track_changes),
                                label: new Text("Generate All"),
                                onPressed: () =>
                                {
                                    var builder = GameObject.Find("MMDRenderer")
                                        .GetComponent<PreviewBuilder.PreviewBuilder>();
                                    builder.StartRender();
                                }),
                            new SizedBox(height: 10, width: 10),
                            FloatingActionButton.extended(
                                icon: new Icon(Icons.refresh),
                                label: new Text("Refresh"),
                                onPressed: () => MMMServices.RefreshObservedMmdObjects())
                        }
                    ),
                    bottomNavigationBar: new Container(
                        height: 30,
                        decoration: new BoxDecoration(
                            Colors.white,
                            boxShadow: new List<BoxShadow>
                            {
                                new BoxShadow(Colors.black.withOpacity(0.3f), new Offset(0, -5), 5)
                            }
                        ),
                        child: new Stack(
                            children: new List<Widget>
                            {
                                SizedBox.expand(
                                    child: new LinearProgressIndicator(
                                        backgroundColor: Colors.transparent,
                                        value: 0,
                                        valueColor: new AlwaysStoppedAnimation<Color>(
                                            Colors.pink.withOpacity(0.3f)))),
                                //Status text
                                new Container(
                                    margin: EdgeInsets.only(5),
                                    alignment: Alignment.centerLeft,
                                    child: new Row(
                                        children: new List<Widget>
                                        {
                                            PaddingText(_renderStatus),
                                            VerticalDivider,
                                            PaddingText(_savingStatus)
                                        }
                                    )
                                )
                            }
                        )
                    )
                )
            );
        }

        private static Widget VerticalDivider =>
            new Container(margin: EdgeInsets.symmetric(horizontal: 3), height: 20,
                width: 1,
                decoration: new BoxDecoration(
                    border: new Border(left: new BorderSide(Colors.black54))));

        private Widget PaddingText(string text)
        {
            return new Container(
                padding: EdgeInsets.only(right: 0),
                child: new Text(text)
            );
        }
    }

    public enum SortType
    {
        ByDefault,
        ByFavorite
    }
}