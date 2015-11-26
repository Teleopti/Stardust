namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver
{
	public static class BrowserInteractionsControlExtensions
	{

		public static void SelectOptionByTextUsingJQuery(this IBrowserInteractions interactions, string selector, string text)
		{
			var selectSelector = selector + ":enabled";
			var optionSelector = string.Format(selectSelector + " option:contains('{0}')", text);
			interactions.AssertExists(selectSelector);
			interactions.AssertExistsUsingJQuery(optionSelector);
			interactions.Javascript("$(\"{0}\").val($(\"{1}\").val());", selectSelector.JSEncode(), optionSelector.JSEncode());
			interactions.Javascript("$(\"{0}\").change();", selectSelector.JSEncode());
			interactions.AssertFirstContainsUsingJQuery(selector + " :selected", text);
		}

		public static void TypeTextIntoInputTextUsingJQuery(this IBrowserInteractions interactions, string selector, string text)
		{
			selector = selector + ":enabled";
			interactions.AssertExists(selector);
			interactions.Javascript("$(\"{0}\").val(\"{1}\");", selector.JSEncode(), text.JSEncode());
			interactions.Javascript("$(\"{0}\").change();", selector.JSEncode());
		}

		public static string GetInputText(this IBrowserInteractions interactions, string selector)
		{
			selector = selector + ":enabled";
			interactions.AssertExists(selector);
			return interactions.Javascript("return document.querySelector(\"{0}\").value;", selector.JSEncode());
		}

		public static string GetText(this IBrowserInteractions interactions, string selector)
		{
			interactions.AssertExists(selector);
			return interactions.Javascript("return document.querySelector(\"{0}\").innerText;", selector.JSEncode());
		}
	}
}