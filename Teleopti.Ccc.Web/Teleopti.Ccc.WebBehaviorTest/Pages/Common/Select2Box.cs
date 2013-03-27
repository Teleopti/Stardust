using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public class Select2Box : Control<TextField>
	{
		public Div Container { get { return Element.PreviousSibling as Div; } }

		public string Value
		{
			get { return Element.Value; }
		}

		public bool IsClosed {
			get
			{
				return Container.Div(QuicklyFind.ByClass("select2-offscreen")).Exists;
			}
		}

		public bool IsOpen
		{
			get
			{
				return Element.DomContainer.Div(QuicklyFind.ByClass("select2-search")).Parent.Exists;
			}
		}

		public IEnumerable<string> OptionsTexts
		{
			get
			{
				var items = Element.DomContainer.ListItems.Filter(QuicklyFind.ByClass("select2-result-selectable"));
				return from item in items select item.Div(QuicklyFind.ByClass("select2-result-label")).Text;
			}
		}

		public void Open()
		{
			Element.DomContainer.Eval("$('#" + Element.Id + "').select2('open')");
		}

		public void SelectTeamByIdAndText(string id, string text)
		{
			Element.DomContainer.Eval("$('#" + Element.Id + "').select2('data', {id:'" + id + "', text:'" + text + "'}).trigger('change')");
		}

		public void SelectTeamByText(string text)
		{
			//var items = Document.ListItems.Filter(QuicklyFind.ByClass("select2-result-selectable"));
			//var item = items.First(x => x.Children().First().InnerHtml.Contains(text));
			//item.MouseEnter();
			//item.EventualClick();
			//item.ev

			//JQuery.Select(string.Format(".select2-result-selectable div:contains('{0}')", text))
			//	.Trigger("mouseover")
			//	.Trigger("click");
			//	//.EvalIn(Element.DomContainer);
		}

	}
}