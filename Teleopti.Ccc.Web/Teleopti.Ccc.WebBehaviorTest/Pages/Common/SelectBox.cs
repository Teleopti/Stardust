using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Pages.jQuery;
using WatiN.Core;
using List = WatiN.Core.List;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public class SelectBox : Control<Div>
	{
		public override WatiN.Core.Constraints.Constraint ElementConstraint { get { return Find.ByClass(s => s.Contains("ui-selectbox")); } }

		public SelectList SelectList { get { return Element.SelectList(Find.First()); } }
		public OptionCollection Options { get { return SelectList.Options; } }

		public Button Button { get { return Element.Button(SelectList.Id + "-button"); } }

		public Div AutoComplete { get { return Element.Div(SelectList.Id + "-menu"); } }
		public List Menu { get { return AutoComplete.Lists.Single(); } }
		public LinkCollection MenuItems { get { return Menu.Links; } }

		public IEnumerable<string> Texts() { return from o in Options where o.Value != "-" select o.Text; }

		public void Select(string text)
		{
			Button.EventualClick();
			AutoComplete.WaitUntilDisplayed();
			Menu.WaitUntilDisplayed();
			EventualAssert.That(() => Menu.InnerHtml, Contains.Substring(text));
			JQuery.Select(string.Format("#{0} a:contains('{1}')", AutoComplete.Id, text))
				.Trigger("mouseover")
				.Trigger("click")
				.EvalIn(Element.DomContainer);
			EventualAssert.That(() => Button.InnerHtml, Contains.Substring(text));
		}
	}
}