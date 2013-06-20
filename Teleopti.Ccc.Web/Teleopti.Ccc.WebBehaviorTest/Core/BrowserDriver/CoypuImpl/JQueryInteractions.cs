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
			Javascript(JQueryScript.WhenFoundOrThrow(selector, "{0}.click();"));
		}

		public void AssertExists(string selector)
		{
			Assert.That(Javascript(JQueryScript.WhenFoundOrThrow(selector, "return true;")), Is.EqualTo("true"));
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