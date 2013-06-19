using System;
using NUnit.Framework;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuImpl
{
	public class JQueryInteractions : IBrowserInteractions
	{
		private readonly JavascriptInteractions _javascript;

		public JQueryInteractions(JavascriptInteractions javascript)
		{
			_javascript = javascript;
		}


		private static string MakeSelector(string selector)
		{
			if (selector.Contains("'"))
			{
				return string.Format(@"$(""{0}"")", selector);
			}
			return string.Format("$('{0}')", selector);
		}

		public static string JQuery(string selector, string jsOnFound)
		{
			var jq = MakeSelector(selector);
			var error = string.Format("throw \"Cannot find element with selector '{0}' using jquery \";", selector);
			return
				"var jq = " + jq + ";" +
				"if (jq.length > 0) {" +
				jsOnFound +
				"} else {" +
				error +
				"}"
				;
		}

		public object Javascript(string javascript)
		{
			return _javascript.Javascript(javascript);
		}

		public void GoToWaitForCompleted(string uri)
		{
			throw new NotImplementedException();
		}

		public void GoToWaitForUrlAssert(string uri, string assertUrlContains)
		{
			throw new NotImplementedException();
		}

		public void Click(string selector)
		{
			Javascript(JQuery(selector, "jq.click();"));
		}

		public void AssertExists(string selector)
		{
			Assert.That(Javascript(JQuery(selector, "return true;")), Is.EqualTo("true"));
		}

		public void AssertNotExists(string existsSelector, string notExistsSelector)
		{
			throw new NotImplementedException();
		}

		public void AssertContains(string selector, string text)
		{
			throw new NotImplementedException();
		}

		public void AssertNotContains(string selector, string text)
		{
			throw new NotImplementedException();
		}

		public void AssertUrlContains(string url)
		{
			throw new NotImplementedException();
		}

		public void AssertUrlNotContains(string urlContains, string urlNotContains)
		{
			throw new NotImplementedException();
		}

		public void AssertJavascriptResultContains(string javascript, string text)
		{
			throw new NotImplementedException();
		}

		public void DumpInfo(Action<string> writer)
		{
			throw new NotImplementedException();
		}

		public void DumpUrl(Action<string> writer)
		{
			throw new NotImplementedException();
		}
	}
}