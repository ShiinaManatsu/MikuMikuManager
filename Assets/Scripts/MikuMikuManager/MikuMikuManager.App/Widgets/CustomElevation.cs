using System.Collections.Generic;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace MikuMikuManager.App
{
    class CustomElevation : StatelessWidget
    {
        private Widget Child { get; set; }
        private float Height { get; set; }
        private EdgeInsets Padding { get; set; }
        private EdgeInsets Margin { get; set; }

        public CustomElevation(Widget child, float height = 10, EdgeInsets padding = null, EdgeInsets margin = null)
        {
            Child = child;
            Height = height;
            Padding = padding;
            Margin = margin;
        }


        public override Widget build(BuildContext context)
        {
            return new Container(
              decoration: new BoxDecoration(
                borderRadius: BorderRadius.all(Radius.circular(Height / 2)),
                boxShadow: new List<BoxShadow>
                {
                    new BoxShadow(
                    color: Colors.black.withOpacity(0.2f),
                    blurRadius: Height / 5,
                    offset:new Offset(0, 0)
                  )
                }
              ),
              child: Child,
              margin: Margin is null ? EdgeInsets.all(Height / 5) : Margin,
              padding: Padding
            );
        }
    }
}