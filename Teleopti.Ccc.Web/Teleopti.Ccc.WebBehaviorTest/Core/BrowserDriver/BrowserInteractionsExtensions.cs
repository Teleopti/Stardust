using System;
using System.Text;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public static class BrowserInteractionsExtensions
	{
		public static void Javascript(this IBrowserInteractions interactions, string javascript, params object[] args)
		{
			interactions.Javascript(string.Format(javascript, args));
		}

		public static void Javascript(this IBrowserInteractions interactions, Action<Action<string>> javascript, params object[] args)
		{
			var builder = new StringBuilder();
			javascript.Invoke(s => builder.Append(s));
			interactions.Javascript(string.Format(builder.ToString(), args));
		}

		public static void AssertExists(this IBrowserInteractions interactions, string selector, params object[] args)
		{
			interactions.AssertExists(string.Format(selector, args));
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

		public static void SelectOptionByTextUsingJQuery(this IBrowserInteractions interactions, string selector, string text)
		{
			var selectSelector = selector + ":enabled";
			var optionSelector = string.Format(selectSelector + " option:contains('{0}')", text);
			interactions.AssertExists(selectSelector);
			interactions.AssertExists(optionSelector);
			interactions.Javascript("$(\"{0}\").val($(\"{1}\").val());", selectSelector.JSEncode(), optionSelector.JSEncode());
			interactions.Javascript("$(\"{0}\").change();", selectSelector.JSEncode());
		}

		public static void TypeTextIntoInputTextUsingJQuery(this IBrowserInteractions interactions, string selector, string text)
		{
			selector = selector + ":enabled";
			interactions.AssertExists(selector);
			interactions.Javascript("$(\"{0}\").val(\"{1}\");", selector.JSEncode(), text.JSEncode());
			interactions.Javascript("$(\"{0}\").change();", selector.JSEncode());
		}

	}
}