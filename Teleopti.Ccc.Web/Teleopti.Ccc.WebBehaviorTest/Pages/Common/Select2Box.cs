using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public static class Select2Box
	{
		public static void AssertSelectedOptionText(string select2Id, string optionText)
		{
			Browser.Interactions.AssertJavascriptResultContains(string.Format("return $('#Team-Picker option:contains(\"{0}\"):selected').length;", optionText), "1");
		}

		public static void AssertSelectedOptionValue(string select2Id, string optionValue)
		{
			Browser.Interactions.AssertJavascriptResultContains(string.Format("return $('#Team-Picker option[value=\"{0}\"]:selected').length;", optionValue), "1");
		}

		public static void AssertOptionExist(string select2Id, string optionText)
		{
			AssertIsOpen(select2Id);
			Browser.Interactions.AssertExists(string.Format(".select2-result-selectable .select2-result-label:contains('{0}')", optionText));
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
			AssertOptionExist(select2Id, text);
			Browser.Interactions.Javascript("$('.select2-result-selectable div:contains(\"" + text + "\")').trigger('mouseup');");
			Browser.Interactions.AssertExists(string.Format("#s2id_{0} .select2-choice span:contains('{1}')", select2Id, text));
		}

		public static void SelectItemByIdAndText(string select2Id, string optionValue, string optionText)
		{
			Browser.Interactions.Javascript(string.Format(
				"$('#{0}').select2('data', {{id:'{1}', text:'{2}'}}).trigger('change')", select2Id, optionValue, optionText));
			AssertSelectedOptionValue(select2Id, optionValue);
		}
	}
}