using System;
using Coypu;
using NUnit.Framework;

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
			return _browser.ExecuteScript(javascript);
		}

		public void GoToWaitForCompleted(string uri)
		{
			_browser.Visit(uri);
		}

		public void GoToWaitForUrlAssert(string uri, string assertUrlContains)
		{
			_browser.Visit(uri);
		}

		private Options Options()
		{
			return _options ?? _configuration;
		}

		public void Click(string selector)
		{
			var javascript = string.Format("var selection = $(\"{0}\");", selector.JSEncode()) +
							 "if (selection.length > 0) {" +
							 "selection.click();" +
							 "} else {" +
							 "throw 'Cant find it!';" +
							 "}"
				;
			_browser.RetryUntilTimeout(() => _browser.ExecuteScript(javascript), Options());
		}

		public void AssertExists(string selector)
		{
			Assert.That(_browser.HasCss(selector));
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
			Assert.That(_browser.ExecuteScript(javascript), Is.StringContaining(text));
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