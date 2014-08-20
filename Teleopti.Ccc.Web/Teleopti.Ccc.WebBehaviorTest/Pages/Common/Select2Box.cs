using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public static class Select2Box
	{
		public static void AssertSelectedOptionText(string select2Id, string optionText)
		{
			Browser.Interactions.AssertJavascriptResultContains(string.Format("return $('#{0} option:contains(\"{1}\"):selected').length;", select2Id, optionText), "1");
		}

		public static void AssertSelectedOptionValue(string select2Id, string optionValue)
		{
			Browser.Interactions.AssertJavascriptResultContains(string.Format("return $('#{0} option[value=\"{1}\"]:selected').length;", select2Id, optionValue), "1");
		}

		public static void AssertOptionExist(string select2Id, string optionText)
		{
			AssertIsOpen(select2Id);
			Browser.Interactions.Javascript("$('.select2-input').focus().val('').trigger('keyup-change');");
			Browser.Interactions.AssertExistsUsingJQuery(string.Format("#{0} option:contains('{1}')", select2Id, optionText));
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".select2-result-selectable .select2-result-label:contains('{0}')", optionText));
		}

		public static string FirstOptionText
		{
			get { return (string)Browser.Interactions.Javascript("return $('.select2-result-selectable .select2-result-label:first-child');"); }
		}

		public static string LastOptionText
		{
			get { return (string)Browser.Interactions.Javascript("return $('.select2-result-selectable .select2-result-label:last-child');"); }
		}

		public static void AssertIsOpen(string select2Id)
		{
			Browser.Interactions.AssertExists(string.Format("#s2id_{0}.select2-dropdown-open", select2Id));
		}

		public static void AssertIsClosed(string select2Id)
		{
			Browser.Interactions.AssertExists(string.Format("#{0}:enabled", select2Id));
			Browser.Interactions.AssertNotExists(string.Format("#s2id_{0}", select2Id), string.Format("#s2id_{0}.select2-dropdown-open", select2Id));
		}

		public static void Open(string select2Id)
		{
			AssertIsClosed(select2Id);
			Browser.Interactions.ClickUsingJQuery(string.Format("#s2id_{0}", select2Id)); // for chrome
			Browser.Interactions.Javascript(string.Format("$('#{0}').select2('open');", select2Id)); // for IE
			AssertIsOpen(select2Id);
		}

		public static void OpenWhenOptionsAreLoaded(string select2Id)
		{
			AssertIsClosed(select2Id);
			Browser.Interactions.AssertExists(string.Format("#{0} > option", select2Id));
			Browser.Interactions.Click(string.Format("#s2id_{0}", select2Id)); // for chrome
			Browser.Interactions.Javascript(string.Format("$('#{0}').select2('open');", select2Id)); // for IE
			AssertIsOpen(select2Id);
		}
		
		public static void SelectItemByText(string select2Id, string text)
		{
			AssertOptionExist(select2Id, text);
			Browser.Interactions.Javascript("$('.select2-result-selectable div:contains(\"" + text + "\")').trigger('mouseup');");
			Browser.Interactions.AssertExistsUsingJQuery(string.Format("#s2id_{0} .select2-choice span:contains('{1}')", select2Id, text));
		}

		public static void SelectItemByIdAndText(string select2Id, string optionValue, string optionText)
		{
			Browser.Interactions.AssertExists(string.Format("#{0} > option", select2Id));
			Browser.Interactions.Javascript(string.Format(
				"$('#{0}').select2('data', {{id:'{1}', text:'{2}'}}).trigger('change')", select2Id, optionValue, optionText));
			AssertSelectedOptionValue(select2Id, optionValue);
		}

		public static void SelectFirstOption(string select2Id)
		{
			Browser.Interactions.Javascript("$('.select2-result-selectable div').trigger('mouseup');");
		}
	}
}