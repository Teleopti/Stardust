using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages.jQuery;
using WatiN.Core;
using List = WatiN.Core.List;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public class SplitButton : Control<Div>
	{
		public override WatiN.Core.Constraints.Constraint ElementConstraint { get { return Find.ByClass(s => s.Contains("ui-splitbutton")); } }

		public Button SetButton { get { return Element.Button(Element.Id + "-set-button"); } }
		public Button ListButton { get { return Element.Button(Element.Id + "-list-button"); } }
		public Div AutoComplete { get { return Element.Div(Element.Id + "-menu"); } }
		public List Menu { get { return AutoComplete.Lists.Single(); } }

		public void SelectWait(string text)
		{
			Select(text);
			EventualAssert.That(() => SetButton.InnerHtml, Contains.Substring(text));
		}

		public void Select(string text)
		{
			ListButton.EventualClick();
			JQuery.Select(string.Format("#{0} a:contains('{1}')", AutoComplete.Id, text))
				.Trigger("mouseover")
				.Trigger("click")
				.Eval();
		}

	}
}