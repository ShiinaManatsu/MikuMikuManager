using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace MikuMikuManager.App
{
    /// <summary>
    /// Expansion panel style gallery
    /// </summary>
    public class ExpansionCardPanel : StatefulWidget
    {
        /// <summary>
        /// Dictionary contains the title and list of mmd objects
        /// </summary>
        public Dictionary<string, List<Widget>> CardDictionary { get; set; }

        /// <summary>
        /// ExpansionCardPanel constructor
        /// </summary>
        /// <param name="keyValuePairs">Card Dictionary</param>
        public ExpansionCardPanel(Dictionary<string, List<Widget>> keyValuePairs)
        {
            CardDictionary = keyValuePairs;
        }

        public override State createState() => new ExpansionCardPanelState();
    }

    public class ExpansionCardPanelState : AutomaticKeepAliveClientMixin<ExpansionCardPanel>
    {
        List<PanelItem> panelItems;

        public override void initState()
        {
            base.initState();

            panelItems = widget.CardDictionary.Select(x => new PanelItem() { IsExpanded = false, Title = x.Key, Cards = x.Value }).ToList();
        }

        public override Widget build(BuildContext context) => new ExpansionPanelList(
            expansionCallback: (index, isExpanded) => setState(() => panelItems[index].IsExpanded = !isExpanded),
            children: panelItems.Select(x => new ExpansionPanel(
                 headerBuilder: (BuildContext buildContext, bool isExpanded) => new ListTile(title: new Text(x.Title)),
                 body: GridView.count(
                     padding: EdgeInsets.all(50),
                     primary: false,
                     crossAxisSpacing: 8,
                     mainAxisSpacing: 8,
                     childAspectRatio: 0.5f,
                     crossAxisCount: (int)(MediaQuery.of(context).size.width / 230f),
                     shrinkWrap: true,
                     children: x.Cards),
                 isExpanded: x.IsExpanded
                 )).ToList()
            );

        protected override bool wantKeepAlive => true;
    }

    /// <summary>
    /// Expansion panel configuration
    /// </summary>
    internal class PanelItem
    {
        /// <summary>
        /// MMD objects contained
        /// </summary>
        public List<Widget> Cards { get; set; }

        /// <summary>
        /// Card title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Indicate if the expansion expanded
        /// </summary>
        public bool IsExpanded { get; set; }
    }
}