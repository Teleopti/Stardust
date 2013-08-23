using System;
using System.Text;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public static class BrowserInteractionsOverloadExtensions
	{
		public static void Javascript(this IBrowserInteractions interactions, string javascript, params object[] args)
		{
			interactions.Javascript(string.Format(javascript, args));
		}

		public static void AssertJavascriptResultContains(this IBrowserInteractions interactions, string text, string javascript, params object[] args)
		{
			interactions.AssertJavascriptResultContains(string.Format(javascript, args), text);
		}

		public static void AssertExists(this IBrowserInteractions interactions, string selector, params object[] args)
		{
			interactions.AssertExists(string.Format(selector, args));
		}
	}
}