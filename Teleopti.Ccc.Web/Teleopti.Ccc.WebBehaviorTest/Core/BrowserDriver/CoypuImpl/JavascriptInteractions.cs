using System;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuImpl
{
	public class JavascriptInteractions : IBrowserInteractions
	{
		private readonly Func<string, object> _executeScript;

		public JavascriptInteractions(Func<string, object> executeScript)
		{
			_executeScript = executeScript;
		}

		public object Javascript(string javascript)
		{
			return _executeScript(javascript);
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
			throw new NotImplementedException();
		}

		public void AssertExists(string selector)
		{
			throw new NotImplementedException();
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