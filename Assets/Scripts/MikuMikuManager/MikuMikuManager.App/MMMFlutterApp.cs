using System.Collections.Generic;
using UniRx;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

/// <summary>
/// Flutter application namespace
/// </summary>
namespace MikuMikuManager.App
{

    public static class ContextProvider
    {
        public static BuildContext BuildContext { get; set; }
    }

    public class MMMFlutterApp : UIWidgetsPanel
    {
        protected override void OnEnable()
        {
            FontManager.instance.addFont(Resources.Load<Font>(path: "Fonts/MATERIALICONS-REGULAR"), "Material Icons");
            base.OnEnable();
        }

        const string homePage = "/";
        const string previewPage = "/Preview";
        const string aboutPage = "/About";

        public static ReactiveProperty<string> SearchPattern { get; set; } = new ReactiveProperty<string>(string.Empty);

        public static ReactiveProperty<SortType> SortTypeProperty { get; set; } = new ReactiveProperty<SortType>(SortType.ByDefault);

        protected override Widget createWidget() => new MaterialApp(
            onGenerateRoute: (settings) =>
            {
                Widget screen;
                switch (settings.name)
                {
                    case homePage:
                        screen = home;
                        break;
                    case previewPage:
                        screen = new ItemPreviewPage();
                        break;
                    case aboutPage:
                        screen = new AboutPage();
                        break;
                    default:
                        screen = new Center(child: new Text("Error route"));
                        break;
                }
                return new MaterialPageRoute(builder: (context) =>
                {
                    ContextProvider.BuildContext = context;
                    return screen;
                });
            },
            title: "MikuMikuManager",
            theme:new ThemeData(
                pageTransitionsTheme:new PageTransitionsTheme(
                    builder:new FadeUpwardsPageTransitionsBuilder()
                    )
                )
        );

        static readonly Widget home = new DefaultTabController(
            length: 2,
            child: new Scaffold(
                drawer:new Drawer(
                    child:new Builder(builder:context =>
                                new FlatButton(
                                    child: new Text("About"),
                                    onPressed: () => Navigator.popAndPushNamed(context, aboutPage)))),
                appBar: new AppBar(
                    bottom: new TabBar(
                        tabs: new List<Widget> {
                            new Container (height: 30, child: new Text ("Home")),
                            new Container (height: 30, child: new Text ("Settings"))
                            //,
                            //new Container (height: 30, child: new Text ("About")),
                        },
                        labelStyle: new TextStyle(fontSize: 24f),
                        indicatorSize: TabBarIndicatorSize.tab
                    ),
                    title: new Builder(builder: context =>
                      new Container(
                          child: new Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              mainAxisSize: MainAxisSize.max,
                              children: new List<Widget> {
                                    new Text ("MikuMikuManager"),
                                    new Container (
                                        padding: EdgeInsets.all(8),
                                        width: Screen.width / 2,
                                        decoration: new BoxDecoration (
                                            color: Colors.white24,
                                            border: Border.all(color:Colors.white24),
                                            borderRadius:BorderRadius.circular(5)),
                                        child:
                                        new TextField (
                                            decoration: InputDecoration.collapsed (
                                                hintText: "Search..."
                                            ),
                                            autofocus : false,
                                            style : new TextStyle (fontSize: 17),
                                            onSubmitted:x=>SearchPattern.Value=x
                                        )),
                                    new PopupMenuButton<SortType>(
                                        child:new Icon(Icons.sort),
                                        itemBuilder:_=>new List<PopupMenuEntry<SortType>>
                                        {
                                            new PopupMenuItem<SortType> (
                                                value: SortType.ByDefault,
                                                child: new Text("By Default")
                                            ),
                                            new PopupMenuItem<SortType> (
                                                value: SortType.ByFavorite,
                                                child: new Text("By Favorite")
                                            ),
                                        },
                                        initialValue:SortTypeProperty.Value,
                                        onSelected: x =>
                                        {
                                            SortTypeProperty.Value=x;
                                        }
                                    )
                              }
                          )
                      )
                    )
                ),
                body: new Padding(
                    padding: EdgeInsets.all(8),
                    child: new TabBarView(
                        children: new List<Widget> {
                            new HomePage (),
                            new SettingPage ()
                            //,
                            //new AboutPage ()
                        }
                    )
                )
            )
        );
    }

    public enum SortType
    {
        ByDefault,
        ByFavorite
    }
}