using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages.jQuery;
using WatiN.Core;
using log4net;
using List = WatiN.Core.List;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public class SelectBox : Control<Div>
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(SelectBox));

		public override WatiN.Core.Constraints.Constraint ElementConstraint { get { return Find.ByClass(s => s.Contains("ui-selectbox")); } }

		public SelectList SelectList { get { return Element.SelectList(Find.First()); } }
		public OptionCollection Options { get { return SelectList.Options; } }

		public Button Button { get { return Element.Button(SelectList.Id + "-button"); } }

		public Div AutoComplete { get { return Element.Div(SelectList.Id + "-menu"); } }
		public List Menu { get { return Element.List(Find.First()); } }
		public LinkCollection MenuItems { get { return Menu.Links; } }

		public string SelectedText { get { return Button.InnerHtml; } }
		public IEnumerable<string> Texts() { return from o in Options where o.Value != "-" select o.Text; }

		public void SelectWait(string text)
		{
			Select(text);
			EventualAssert.That(() => Button.InnerHtml, Contains.Substring(text));
		}

		public void Select(string text)
		{
			Button.EventualClick();
			EventualAssert.That(() => AutoComplete.Exists, Is.True);
			EventualAssert.That(() => AutoComplete.DisplayVisible(), Is.True);
			EventualAssert.That(() => Menu.Exists, Is.True);
			EventualAssert.That(() => Menu.InnerHtml, Contains.Substring(text));
			JQuery.Select(string.Format("#{0} a:contains('{1}')", AutoComplete.Id, text))
				.Trigger("mouseover")
				.Trigger("click")
				.Eval();
		}

		public void Open()
		{
			Menu.WaitUntilExists();
			Button.WaitUntilExists();

			Button.EventualClick();
			Menu.WaitUntilDisplayed();
		}
	}
}