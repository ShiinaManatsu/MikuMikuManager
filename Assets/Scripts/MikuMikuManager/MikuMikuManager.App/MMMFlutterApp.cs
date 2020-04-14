using System.Collections;
using System.Collections.Generic;
using MikuMikuManager.Services;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
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
    public class MMMFlutterApp : UIWidgetsPanel
    {
        protected override void OnEnable()
        {
            FontManager.instance.addFont(Resources.Load<Font>(path: "Fonts/MATERIALICONS-REGULAR"), "Material Icons");
            base.OnEnable();
        }

        const string homePage = "/";
        const string previewPage = "/Preview";
        static TextEditingController controller = new TextEditingController();

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
                    default:
                        screen = new Center(child: new Text("Error route"));
                        break;
                }
                return new MaterialPageRoute(builder: (context) => screen);
            },
            title: "MikuMikuManager"
        );

        Widget home = new DefaultTabController(
            length: 3,
            child: new Scaffold(
                appBar: new AppBar(
                    bottom: new TabBar(
                        tabs: new List<Widget> {
                            new Text ("Home"),
                            new Text ("Settings"),
                            new Text ("About"),
                        },
                        labelStyle: new TextStyle(fontSize: 24f),
                        indicatorSize: TabBarIndicatorSize.tab
                    ),
                    title: new Row(
                        mainAxisAlignment: MainAxisAlignment.spaceAround,
                        mainAxisSize: MainAxisSize.max,
                        children: new List<Widget> {
                            new Text ("MikuMikuManager"),
                            new Container (
                                decoration: new BoxDecoration (border : Border.all (color: Unity.UIWidgets.ui.Color.white)),
                                child:
                                new TextField (
                                    controller: controller,
                                    decoration: InputDecoration.collapsed (
                                        hintText: "Search..."
                                    ),
                                    autofocus : false,
                                    //onChanged: onChanged,
                                    //onSubmitted: onSubmitted,
                                    //onTap: onTap,
                                    style : new TextStyle (fontSize: 17)
                                ))
                        }
                    )
                ),
                body: new Padding(
                    padding: EdgeInsets.all(8),
                    child: new TabBarView(
                        children: new List<Widget> {
                            new HomePage (),
                            new SettingPage (),
                            new AboutPage ()
                        }
                    )
                )
            )
        );
    }
}