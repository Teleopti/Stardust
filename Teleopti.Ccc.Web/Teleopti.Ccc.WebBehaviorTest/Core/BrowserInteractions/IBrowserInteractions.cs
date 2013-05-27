namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserInteractions
{
	public interface IBrowserInteractions
	{
		object Javascript(string javascript);
		void GoToWaitForCompleted(string uri);
		void GoToWaitForUrlAssert(string uri, string assertUrlContains);
		void Click(string selector);
		void AssertExists(string selector);
		void AssertNotExists(string existsSelector, string notExistsSelector);
		void AssertContains(string selector, string text);
		void AssertNotContains(string selector, string text);
		void AssertUrlContains(string url);
		void AssertUrlNotContains(string urlContains, string urlNotContains);
		void AssertJavascriptResultContains(string javascript, string text);
		void AssertInputValue(string selector, string value);
	}

	public static class BrowserInteractionsExtensions
	{
		public static void Javascript(this IBrowserInteractions interactions, string javascript, params object[] args)
		{
			interactions.Javascript(string.Format(javascript, args));
		}

		public static void AssertExists(this IBrowserInteractions interactions, string selector, params object[] args)
		{
			interactions.AssertExists(string.Format(selector, args));
		}

		public static void SelectOptionByTextUsingJQuery(this IBrowserInteractions interactions, string selectSelector, string text)
		{
			selectSelector = selectSelector + ":enabled";
			var optionSelector = string.Format(selectSelector + " option:contains('{0}')", text);
			interactions.AssertExists(selectSelector);
			interactions.AssertExists(optionSelector);
			interactions.Javascript("$(\"{0}\").val($(\"{1}\").val());", selectSelector, optionSelector);
			interactions.Javascript("$(\"{0}\").change();", selectSelector);
		}

		public static void TypeTextIntoInputTextUsingJQuery(this IBrowserInteractions interactions, string selectSelector, string text)
		{
			selectSelector = selectSelector + ":enabled";
			interactions.AssertExists(selectSelector);
			interactions.Javascript("$(\"{0}\").val(\"{1}\");", selectSelector, text);
			interactions.Javascript("$(\"{0}\").change();", selectSelector);
		}

		public static void AssertElementsAreVisible(this IBrowserInteractions interactions, string selectSelector)
		{
			var js = string.Format("$('{0}').filter(\":visible\").length > 0 ? 'visible' : 'not visible';", selectSelector);
			interactions.AssertJavascriptResultContains(js, "visible");
		}
	}
}