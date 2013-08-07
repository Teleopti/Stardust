namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public static class BrowserInteractionsJQueryExtensions
	{
		public static void AssertExistsUsingJQuery(this IBrowserInteractions interactions, string selector, params object[] args)
		{
			selector = f(selector, args);
			var jquery = dollar(selector);
			var script = f("var jq = {0};", jquery) +
			             "if (jq.length > 0) {" +
			             "return 'found';" +
			             "} else {" +
			             f("throw \"Cannot find element with selector '{0}' using jquery \";", selector) +
			             "}";
			interactions.AssertJavascriptResultContains(script, "found");
		}

		public static void AssertNotExistsUsingJQuery(this IBrowserInteractions interactions, string existsSelector, string notExistsSelector, params object[] args)
		{
			interactions.AssertExistsUsingJQuery(existsSelector, args);

			notExistsSelector = f(notExistsSelector, args);
			var jquery = dollar(notExistsSelector);
			var script = f("var jq = {0};", jquery) +
			             "if (jq.length == 0) {" +
			             "return 'notfound';" +
			             "} else {" +
			             f("throw \"Found element with selector '{0}' using jquery although I shouldnt \";", notExistsSelector) +
			             "}";
			interactions.AssertJavascriptResultContains(script, "notfound");
		}

		public static void ClickUsingJQuery(this IBrowserInteractions interactions, string selector, params object[] args)
		{
			selector = f(selector, args);
			var jquery = dollar(selector);
			var script = f("var jq = {0};", jquery) +
						 "if (jq.length > 0) {" +
						 "jq.click();" + 
						 "return 'clicked';" +
						 "} else {" +
						 f("throw \"Cannot find element with selector '{0}' using jquery \";", selector) +
						 "}";
			interactions.AssertJavascriptResultContains(script, "clicked");
		}

		public static void AssertInputValueUsingJQuery(this IBrowserInteractions interactions, string selector, string value)
		{
			interactions.AssertJavascriptResultContains("return $('" + selector + "').val();", value);
		}

		public static void AssertVisibleUsingJQuery(this IBrowserInteractions interactions, string selector)
		{

			var js = string.Format("return $('{0}').filter(\":visible\").length > 0 ? 'visible' : 'not visible';", selector.JSEncode());
			interactions.AssertJavascriptResultContains(js, "visible");
		}

		public static void AssertNotVisibleUsingJQuery(this IBrowserInteractions interactions, string selector)
		{
			var js = string.Format("return $('{0}').filter(\":not(:visible)\").length > 0 ? 'not visible but existing' : 'visible or non existing';", selector.JSEncode());
			interactions.AssertJavascriptResultContains(js, "not visible but existing");
		}




		private static string dollar(string selector)
		{
			var jquery = string.Format("$('{0}')", selector);
			if (selector.Contains("'"))
				jquery = string.Format(@"$(""{0}"")", selector);
			return jquery;
		}

		private static string f(string text, params object[] args)
		{
			return string.Format(text, args);
		}

	}
}