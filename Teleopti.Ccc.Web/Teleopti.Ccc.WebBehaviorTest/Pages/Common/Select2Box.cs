using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public class Select2Box : Control<SelectList>
	{
		public Div Container
		{
			get { return Element.Parent.DomContainer.Div(Find.BySelector("#s2id_" + Element.Id)); }
		}

		public string Value
		{
			get { return Element.SelectedOption.Value; }
		}

		public bool IsClosed {
			get
			{
				return Container.Div(Find.BySelector(".select2-offscreen")).Exists;
			}
		}

		public bool IsOpen
		{
			get
			{
				return !Container.Div(Find.BySelector(".select2-offscreen")).Exists;
			}
		}

		public IEnumerable<string> OptionsTexts
		{
			get
			{
				EventualAssert.That(() => IsOpen, Is.True);
				var items = Element.DomContainer.ListItems.Filter(QuicklyFind.ByClass("select2-result-selectable"));
				return from item in items select item.Children().First().Text;
			}
		}

		public void Open()
		{
			Element.DomContainer.Eval("$('#" + Element.Id + "').select2('open')");
		}

		public void SelectItemByIdAndText(string id, string text)
		{
			Element.DomContainer.Eval("$('#" + Element.Id + "').select2('data', {id:'" + id + "', text:'" + text + "'}).trigger('change')");
		}

		public void SelectItemByText(string text)
		{
			EventualAssert.That(() => OptionsTexts.Contains(text), Is.True);
			Element.DomContainer.Eval("$('.select2-result-selectable div:contains(\""+text+"\")').trigger('mouseup')");
			EventualAssert.That(() => Container.InnerHtml, Contains.Substring(text));
		}
	}
}