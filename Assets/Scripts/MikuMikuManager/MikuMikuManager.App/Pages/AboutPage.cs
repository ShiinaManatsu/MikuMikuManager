using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using System.IO;

namespace MikuMikuManager.App
{

	public class AboutPage : StatefulWidget
	{
		public override State createState() => new AboutPageState();
	}

	public class AboutPageState : State<AboutPage>
	{
		public override Widget build(BuildContext context) => new Icon(Icons.accessibility,color:Colors.amber);
	}

}