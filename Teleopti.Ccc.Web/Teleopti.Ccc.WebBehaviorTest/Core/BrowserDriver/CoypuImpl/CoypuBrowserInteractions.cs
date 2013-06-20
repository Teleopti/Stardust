using System;
using System.Text.RegularExpressions;
using Coypu;
using NUnit.Framework;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuImpl
{
	public class CoypuBrowserInteractions : IBrowserInteractions
	{
		private readonly BrowserSession _browser;
		private Options _options;
		private readonly SessionConfiguration _configuration;
		private readonly JQueryInteractions _jquery;

		public CoypuBrowserInteractions(BrowserSession browser, SessionConfiguration configuration)
		{
			_browser = browser;
			_configuration = configuration;
			_jquery = new JQueryInteractions(new JavascriptInteractions(Javascript));
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
			string result = null;
			_browser.RetryUntilTimeout(() =>
				{
					result = _browser.ExecuteScript(javascript);
				}, options());
			return result;
		}

		public void GoToWaitForCompleted(string uri)
		{
			_browser.Visit(uri);
		}

		public void GoToWaitForUrlAssert(string uri, string assertUrlContains)
		{
			_browser.Visit(uri);
		}

		private Options options()
		{
			return _options ?? _configuration;
		}

		public void Click(string selector)
		{
			_jquery.Click(selector);
		}

		public void AssertExists(string selector)
		{
			Assert.That(_browser.HasJQueryCss(selector, options()));
		}

		public void AssertNotExists(string existsSelector, string notExistsSelector)
		{
			Assert.That(_browser.HasCss(existsSelector));
			Assert.That(_browser.HasNoCss(notExistsSelector));
		}

		public void AssertContains(string selector, string text)
		{
			Assert.That(_browser.FindCss(selector).HasContent(text));
		}

		public void AssertNotContains(string selector, string text)
		{
			Assert.That(_browser.FindCss(selector).HasNoContent(text));
		}

		public void AssertUrlContains(string url)
		{
			Assert.That(_browser.Location.ToString(), Is.StringContaining(url));
		}

		public void AssertUrlNotContains(string urlContains, string urlNotContains)
		{
			Assert.That(_browser.Location.ToString(), Is.StringContaining(urlContains));
			Assert.That(_browser.Location.ToString(), Is.Not.StringContaining(urlNotContains));
		}

		public void AssertJavascriptResultContains(string javascript, string text)
		{
			var value = Javascript(javascript);
			Assert.That(value, Is.StringContaining(text));
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

	}
}