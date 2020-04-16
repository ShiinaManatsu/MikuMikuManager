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
                    child: new Container(
                        decoration: new BoxDecoration(color: Colors.white70),
                        child: new ButtonBar(
                            alignment: MainAxisAlignment.end,
                            mainAxisSize: MainAxisSize.max,
                        children: new List<Widget>
                        {
                            new IconButton(
                                icon:new Icon(isFavored?Icons.favorite:Icons.favorite_border,
                                color:isFavored?Colors.pinkAccent:null),
                                onPressed:()=> widget.MMDObject.IsFavored.Value=!widget.MMDObject.IsFavored.Value
                            ),
                            new IconButton(
                                icon:new Icon(Icons.delete),
                                onPressed:()=>{ }
                            )
                        }
                    )
                        )
                ); ; ;

            var card = File.Exists(path) ? new Card(
                    color: Colors.amber,
                    child: Image.memory(File.ReadAllBytes(path), fit: BoxFit.cover)
                ) : new Card(
                        color: Colors.amber,
                        child: new Center(child: new Icon(Icons.priority_high))
                    );

            var stack = new Stack(
                    children: new List<Widget>
                    {
                        card,
                        buttonGroup
                    }
                );
            var detect = new GestureDetector(
                    child: stack,
                    onTap: () =>
                    {
                        System.Diagnostics.Process.Start(path.Remove(path.LastIndexOf("\\")));
                    }
                );
            return detect;
        }

        protected override bool wantKeepAlive => true;
    }
}