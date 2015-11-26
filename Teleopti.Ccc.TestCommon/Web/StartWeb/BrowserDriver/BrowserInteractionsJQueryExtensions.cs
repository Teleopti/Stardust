using System.Globalization;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.TestCommon.Web.StartWeb.BrowserDriver
{
	public static class BrowserInteractionsJQueryExtensions
	{
		public static void	AssertExistsUsingJQuery(this IBrowserInteractions interactions, string selector, params object[] args)
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

		public static void AddClassUsingJQuery(this IBrowserInteractions interactions, string className, string selector, params object[] args)
		{
			selector = f(selector, args);
			var jquery = dollar(selector);
			var script = f("var jq = {0};", jquery) +
						 "if (jq.length > 0) {" +
						 f("jq.addClass('{0}');", className) +
						 "return 'added';" +
						 "} else {" +
						 f("throw \"Cannot find element with selector '{0}' using jquery \";", selector) +
						 "}";
			interactions.AssertJavascriptResultContains(script, "added");
		}

		public static void AssertInputValueUsingJQuery(this IBrowserInteractions interactions, string selector, string value)
		{
			interactions.AssertJavascriptResultContains("return $('" + selector + "').val();", value);
		}

		public static void AssertVisibleUsingJQuery(this IBrowserInteractions interactions, string selector)
		{

			var js = string.Format("return $('{0}').filter(\":visible\").length > 0 ? 'is visible' : 'not visible';", selector.JSEncode());
			interactions.AssertJavascriptResultContains(js, "is visible");
		}

		public static void AssertNotVisibleUsingJQuery(this IBrowserInteractions interactions, string selector)
		{
			var js = string.Format("return $('{0}').filter(\":not(:visible)\").length > 0 ? 'not visible but existing' : 'visible or non existing';", selector.JSEncode());
			interactions.AssertJavascriptResultContains(js, "not visible but existing");
		}

		public static void AssertFirstContainsUsingJQuery(this IBrowserInteractions interactions, string selector, string text)
		{
			var jquery = dollar(selector);
			var script = f("var jq = {0};", jquery) +
						 "if (jq.length > 0) {" +
						 "var text = jq.first().text();" +
						 f("if (text.indexOf('{0}') > -1) return 'found';", text.JSEncode()) +
						 f("throw \"Cannot find element with selector '{0}' that contains text '{1}' using jquery \";", selector,
						   text) +
						 "} else {" +
						 f("throw \"Cannot find element with selector '{0}' that contains text '{1}' using jquery \";", selector,
						   text) +
						 "}";
			interactions.AssertJavascriptResultContains(script, "found");
		}

		public static void AssertFirstContainsResourceTextUsingJQuery(this IBrowserInteractions interactions, string selector, string resourceText)
		{
			var english = Resources.ResourceManager.GetString(resourceText, new CultureInfo("en-US"));
			var swedish = Resources.ResourceManager.GetString(resourceText, new CultureInfo("sv-SE"));
			var jquery = dollar(selector);
			var script = f("var jq = {0};", jquery) +
						 "if (jq.length > 0) {" +
						 "var text = jq.first().text();" +
						 f("if (text.indexOf('{0}') > -1) return 'found';", english.JSEncode()) +
						 f("if (text.indexOf('{0}') > -1) return 'found';", swedish.JSEncode()) +
						 f("throw \"Cannot find element with selector '{0}' that contains text '{1}' or '{2}' using jquery \";", selector, english, swedish) +
						 "} else {" +
						 f("throw \"Cannot find element with selector '{0}' that contains text '{1}' or '{2}' using jquery \";", selector, english, swedish) +
						 "}";
			interactions.AssertJavascriptResultContains(script, "found");
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