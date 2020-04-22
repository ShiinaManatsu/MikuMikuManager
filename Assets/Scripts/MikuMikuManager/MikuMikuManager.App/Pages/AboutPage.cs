using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace MikuMikuManager.App
{

	public class AboutPage : StatefulWidget
	{
		public override State createState() => new AboutPageState();
	}

	public class AboutPageState : AutomaticKeepAliveClientWithTickerProviderStateMixin<AboutPage>
	{
		protected override bool wantKeepAlive => true;

		private AnimationController AnimationController;

		private static readonly string AboutTitle = "About MikuMikuManager!";
		private static readonly string Thanks = "感谢尝试Alpha版本的MikuMikuManager!";
		private static readonly string AlertContent = "当前预览需要配合MikuMikuPreview或者将预览文件命名为\"模型名字\".pmx.png并与pmx文件放在同一目录";

		private static TextStyle TitleStyle = new TextStyle(
								fontSize: 30,
								color: Unity.UIWidgets.ui.Color.fromARGB(255, 33, 211, 211));

		private static TextStyle ContentStyle = new TextStyle(
								fontSize: 20,
								color: Colors.black);

		private Widget BuildContentText(string content) => new Text(content, style: ContentStyle, textAlign: Unity.UIWidgets.ui.TextAlign.center);

		public override Widget build(BuildContext context) => new Scaffold(
			appBar:new AppBar(title:new Text("About MikuMikuManager")),
			body:new Center(
				child: new Container(
					width: Screen.width * 0.8f,
						child: new Column(
							mainAxisAlignment: MainAxisAlignment.center,
							mainAxisSize: MainAxisSize.max,
							crossAxisAlignment: CrossAxisAlignment.center,

							children: new List<Widget>
							{
								new Text(AboutTitle,style:TitleStyle),
								new SizedBox(height: 10),	// Spacing
							
								BuildContentText(Thanks),
								new SizedBox(height: 10),	// Spacing

								BuildContentText(AlertContent),
								new SizedBox(height: 10),	// Spacing

								BuildContentText("遇到问题请使用重启大法"),
								new SizedBox(height: 10),	// Spacing

								BuildContentText("或者直接联系我"),
								new SizedBox(height: 10),	// Spacing

								new RichText(
									text:new TextSpan(
										text:"Current application version:",
										children:new List<TextSpan>
										{
											new TextSpan(text:Application.version,style:new TextStyle(color:Colors.green.shade900))
										})
									)
							})

					)
				)
			);
	}

}