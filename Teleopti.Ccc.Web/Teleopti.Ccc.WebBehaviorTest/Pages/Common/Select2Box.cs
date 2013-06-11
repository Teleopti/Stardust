using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

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
		
		public void SelectItemByIdAndText(string id, string text)
		{
			Element.DomContainer.Eval("$('#" + Element.Id + "').select2('data', {id:'" + id + "', text:'" + text + "'}).trigger('change')");
		}


		public static void AssertIsOpen(string select2Id)
		{
			Browser.Interactions.AssertExists(string.Format("#s2id_{0}.select2-dropdown-open", select2Id));
		}

		public static void AssertIsClosed(string select2Id)
		{
			Browser.Interactions.AssertNotExists(string.Format("#s2id_{0}", select2Id), string.Format("#s2id_{0}.select2-dropdown-open", select2Id));
		}

		public static void Open(string select2Id)
		{
			AssertIsClosed(select2Id);
			Browser.Interactions.Javascript(string.Format("$('#{0}').select2('open')", select2Id));
			AssertIsOpen(select2Id);
		}
		
		public static void SelectItemByText(string select2Id, string text)
		{
			Browser.Interactions.AssertExists(string.Format(".select2-result-selectable .select2-result-label:contains('{0}')", text));
			Browser.Interactions.Javascript("$('.select2-result-selectable div:contains(\"" + text + "\")').trigger('mouseup');");
			Browser.Interactions.AssertExists(string.Format("#s2id_{0} .select2-choice span:contains('{1}')", select2Id, text));
		}
	}
}