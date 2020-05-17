using MikuMikuManager.Services;
using System.Collections.Generic;
using UniRx;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

/// <summary>
/// Flutter application namespace
/// </summary>
namespace MikuMikuManager.App
{
    #region Routes

    public static class Routes
    {
        public const string HomePageRouteText = "/";
        public const string PreviewPageRouteText = "/Preview";
        public const string AboutPageRouteText = "/About";
    }

    #endregion

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
        
        public static ReactiveProperty<string> SearchPattern { get; set; } = new ReactiveProperty<string>(string.Empty);

        public static ReactiveProperty<SortType> SortTypeProperty { get; set; } =
            new ReactiveProperty<SortType>(SortType.ByDefault);

        protected override Widget createWidget() => new MaterialApp(
            onGenerateRoute: (settings) =>
            {
                Widget screen;
                switch (settings.name)
                {
                    case Routes.HomePageRouteText:
                        screen = new AppContainer(SearchPattern,SortTypeProperty);
                        break;
                    case Routes.PreviewPageRouteText:
                        screen = new ItemPreviewPage();
                        break;
                    case Routes.AboutPageRouteText:
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
            theme: new ThemeData(
                pageTransitionsTheme: new PageTransitionsTheme(
                    builder: new FadeUpwardsPageTransitionsBuilder()
                )
            )
        );
    }
}