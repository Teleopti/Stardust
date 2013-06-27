using System;
using System.Text;
using Coypu;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuImpl
{
	public class CoypuBrowserInteractions : IBrowserInteractions
	{
		private readonly BrowserSession _browser;
		private Options _options;
		private readonly SessionConfiguration _configuration;

		public CoypuBrowserInteractions(BrowserSession browser, SessionConfiguration configuration)
		{
			_browser = browser;
			_configuration = configuration;
		}

		public void SetTimeout(TimeSpan timeout)
		{
			if (timeout == _configuration.Timeout)
				_options = null;
			else
			{
				_options = new Options
					{
						ConsiderInvisibleElements = _configuration.ConsiderInvisibleElements,
						RetryInterval = _configuration.RetryInterval,
						Timeout = timeout,
						WaitBeforeClick = _configuration.WaitBeforeClick
					};
			}
		}

		public object Javascript(string javascript)
		{
			return retryJavascript(javascript);
		}

		public void GoToWaitForCompleted(string uri)
		{
			_browser.Visit(uri);
		}

		public void GoToWaitForUrlAssert(string uri, string assertUrlContains)
		{
			_browser.Visit(uri);
		}

		public void Click(string selector)
		{
			var script = JQueryScript.WhenFoundOrThrow(selector, "{0}.click();");
			retryJavascript(script);
		}

		public void AssertExists(string selector)
		{
			Assert.That(_browser.HasJQueryCss(selector, options()));
		}

		public void AssertNotExists(string existsSelector, string notExistsSelector)
		{
			AssertExists(existsSelector);
			Assert.That(_browser.HasNoJQueryCss(notExistsSelector, options()));
		}

		public void AssertContains(string selector, string text)
		{
			var script = JQueryScript.WhenFoundOrThrow(selector, "return {0}.text();");
			browserAssert(() => _browser.ExecuteScript(script), Is.StringContaining(text), "Failed to assert that " + selector + " contained text " + text);
		}

		public void AssertNotContains(string selector, string text)
		{
			var script = JQueryScript.WhenFoundOrThrow(selector, "return {0}.text();");
			browserAssert(() => _browser.ExecuteScript(script), Is.Not.StringContaining(text), "Failed to assert that " + selector + " did not contain text " + text);
		}

		public void AssertUrlContains(string url)
		{
			browserAssert(() => _browser.Location.ToString(), Is.StringContaining(url), "Failed to assert that current url contains " + url);
		}

		public void AssertUrlNotContains(string urlContains, string urlNotContains)
		{
			AssertUrlContains(urlContains);
			browserAssert(() => _browser.Location.ToString(), Is.Not.StringContaining(urlNotContains), "Failed to assert that current url did not contain " + urlNotContains);
		}

		public void AssertJavascriptResultContains(string javascript, string text)
		{
			browserAssert(() => _browser.ExecuteScript(javascript), Is.StringContaining(text), "Failed to assert that javascript " + javascript + " returned a value containing " + text);
		}

		public void DumpInfo(Action<string> writer)
		{
			writer(" Time: ");
			writer(DateTime.Now.ToString());
			writer(" Url: ");
			writer(_browser.Location.ToString());
			writer(" Html: ");
			writer(_browser.FindCss("body").Text);
		}

		public void DumpUrl(Action<string> writer)
		{
			writer(_browser.Location.ToString());
		}




		private Options options()
		{
			return _options ?? _configuration;
		}

		private string retryJavascript(string javascript)
		{
			string result = null;
			_browser.RetryUntilTimeout(() => { result = _browser.ExecuteScript(javascript); }, options());
			return result;
		}

		private void browserAssert<T>(Func<T> value, Constraint constraint, string message)
		{
			EventualAssert.That(value, constraint, () => buildMessage(message), new SeleniumExceptionCatcher());
		}

		private string buildMessage(string message)
		{
			var builder = new StringBuilder();
			builder.Append(message);
			builder.Append(" ");
			DumpInfo(s => builder.Append(s));
			return builder.ToString();
		}
	}
}