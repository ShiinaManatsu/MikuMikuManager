using MikuMikuManager.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UniRx;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MikuMikuManager.App
{
    public class MMDCardWidget : StatefulWidget
    {
        public MMDCardWidget(MMDObject mMDObject, Action<MMDObject, bool> callback)
        {
            MMDObject = mMDObject;
            FavoriteCallback = callback;
        }

        public MMDObject MMDObject { get; }
        public Action<MMDObject, bool> FavoriteCallback { get; set; }

        public override State createState()
        {
            return new MMDCardWidgetState();
        }
    }

    public class MMDCardWidgetState : AutomaticKeepAliveClientMixin<MMDCardWidget>
    {
        private MMDObject _mmdObject;

        public override void initState()
        {
            base.initState();
            _mmdObject = widget.MMDObject;
            _mmdObject.PreviewPath.ObserveEveryValueChanged(x => x.Value)
                .Subscribe(_ =>
                {
                    using (WindowProvider.of(GameObject.Find("Panel")).getScope())
                    {
                        _mmdObject.PreviewPath = _mmdObject.PreviewPath;
                    }
                });
        }

        protected override bool wantKeepAlive => true;

        public override Widget build(BuildContext context)
        {
            var path = $"{_mmdObject.FilePath}";
            var buttonGroup = new Positioned(
                bottom: 0,
                right: 0,
                child: new Container(
                    color: Colors.white30,
                    height: 50,
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.start,
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        mainAxisSize: MainAxisSize.max,
                        children: new List<Widget>
                        {
                            new Padding(
                                padding: EdgeInsets.symmetric(3),
                                child: new Center(
                                    child: new Text($"{_mmdObject.FileName}")
                                )
                            ),
                            new AspectRatio(
                                child:
                                new FlatButton(
                                    child: new Icon(
                                        _mmdObject.IsFavored.Value ? Icons.favorite : Icons.favorite_border,
                                        color: _mmdObject.IsFavored.Value ? Colors.pinkAccent : null),
                                    onPressed: () =>
                                    {
                                        widget.FavoriteCallback(_mmdObject, !_mmdObject.IsFavored.Value);
                                    }
                                )
                            ),
                            new AspectRatio(
                                child:
                                new FlatButton(
                                    child: new Icon(Icons.open_in_browser),
                                    onPressed: () =>
                                        Process.Start(path.Remove(path.LastIndexOf("\\", StringComparison.Ordinal)))
                                )
                            )
                        }
                    )
                )
            );

            var card = new GestureDetector(
                child: _mmdObject.PreviewPath.Value != string.Empty
                    ? new Card(
                        color: Colors.amber,
                        child: Image.memory(File.ReadAllBytes(_mmdObject.PreviewPath.Value), fit: BoxFit.cover))
                    : new Card(
                        color: Colors.amber,
                        child: new Center(child: new Icon(Icons.priority_high))
                    )
                ,
                onTap: () => ShowContextMenu(context)
            );

            var stack = new Stack(
                children: new List<Widget>
                {
                    card,
                    buttonGroup
                }
            );

            return stack;
        }

        enum MenuItem
        {
            LoadPreview
        }
        
        private void ShowContextMenu(BuildContext context)
        {
            if (!GameObject.Find("Panel").GetComponent<UIClickProperty>().IsMRBClicked) return;
            using (WindowProvider.of(GameObject.Find("Panel")).getScope())
            {
                PopupMenuUtils.showMenu<MenuItem>(
                    context,
                    RelativeRect.fromLTRB(Input.mousePosition.x, Screen.height - Input.mousePosition.y,
                        Screen.width - Input.mousePosition.x, Input.mousePosition.y),
                    new List<PopupMenuEntry<MenuItem>>
                    {
                        new PopupMenuItem<MenuItem>(
                            value: MenuItem.LoadPreview,
                            child: new Text("Load Preview")
                        )
                    },
                    initialValue: MenuItem.LoadPreview
                ).Then(x =>
                {
                    try
                    {
                        var token = (MenuItem) x;
                        switch (token)
                        {
                            case MenuItem.LoadPreview:
                                var builder = GameObject.Find("MMDRenderer")
                                    .GetComponent<PreviewBuilder.PreviewBuilder>();
                                builder.StartRender(_mmdObject);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                });
            }
        }
    }
}