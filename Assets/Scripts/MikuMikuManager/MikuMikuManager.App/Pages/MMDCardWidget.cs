using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using MikuMikuManager.Data;
using MikuMikuManager.Services;
using UniRx;
using Unity.UIWidgets.material;
using Unity.UIWidgets.widgets;
using Material = Unity.UIWidgets.material.Material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using UnityEngine;
using System.Threading.Tasks;

namespace MikuMikuManager.App
{
    public class MMDCardWidget : StatefulWidget
    {
        public MMDObject MMDObject { get; private set; }
        public MMDCardWidget(ref MMDObject mMDObject)
        {
            MMDObject = mMDObject;
        }

        public override State createState() => new MMDCardWidgetState();
    }

    public class MMDCardWidgetState : AutomaticKeepAliveClientMixin<MMDCardWidget>
    {
        private bool isFavored;

        public override void initState()
        {
            base.initState();
            isFavored = widget.MMDObject.IsFavored.Value;
            widget.MMDObject.IsFavored.ObserveEveryValueChanged(x => x.Value)
                .Subscribe(x => setState(() => isFavored = x));
        }

        public override Widget build(BuildContext context)
        {
            var path = $"{widget.MMDObject.FileName}.png";
            var buttonGroup = new Positioned(
                bottom: 0,
                right: 0,
                child: new Container(
                    color:Colors.white30,
                    height: 50,
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.start,
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        mainAxisSize: MainAxisSize.max,
                        children: new List<Widget>
                        {
                            new AspectRatio(
                                    child:
                                    new FlatButton(
                                        child:new Icon(isFavored?Icons.favorite:Icons.favorite_border,
                                        color:isFavored?Colors.pinkAccent:null),
                                        onPressed:()=> widget.MMDObject.IsFavored.Value=!widget.MMDObject.IsFavored.Value
                                    )
                                ),
                            new AspectRatio(
                                    child:
                                    new FlatButton(
                                        child:new Icon(Icons.open_in_browser),
                                        onPressed:()=>System.Diagnostics.Process.Start(path.Remove(path.LastIndexOf("\\")))
                                    )
                                )
                        }
                    )
                )
            ); ; ;

            var card = new GestureDetector(
                    child: File.Exists(path) ? new Card(
                        color: Colors.amber,
                        child: Image.memory(File.ReadAllBytes(path), fit: BoxFit.cover)) :
                            new Card(
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

        private void ShowContextMenu(BuildContext context)
        {
            if (UIClickProperty.IsMRBClicked)
            {
                Debug.Log(context);
                PopupMenuUtils.showMenu(
                    context: context,
                    position: RelativeRect.fromLTRB(Input.mousePosition.x, Screen.height - Input.mousePosition.y, Screen.width - Input.mousePosition.x, Input.mousePosition.y),
                    items: new List<PopupMenuEntry<Widget>>
                    {
                                                new PopupMenuItem<Widget>(
                                                        child:new Text("Test Menu A")
                                                    ),
                                                new PopupMenuItem<Widget>(
                                                        child:new Text("Test Menu B")
                                                    ),
                                                new PopupMenuItem<Widget>(
                                                        child:new Text("Test Menu C")
                                                    ),
                                                new PopupMenuItem<Widget>(
                                                        child:new Text("Test Menu D")
                                                    )
                    },
                    initialValue: null
                );
            }
        }

        protected override bool wantKeepAlive => true;
    }
}