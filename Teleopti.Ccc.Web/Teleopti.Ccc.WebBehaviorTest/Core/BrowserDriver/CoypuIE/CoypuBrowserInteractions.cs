using System;
using Coypu;
using NUnit.Framework;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuIE
{
	public class CoypuBrowserInteractions : IBrowserInteractions
	{
		private readonly BrowserSession _browser;

		public CoypuBrowserInteractions(BrowserSession browser)
		{
			_browser = browser;
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

		public void Click(string selector)
		{
			_browser.FindCss(selector).Click();
		}

		public void AssertExists(string selector)
		{
			_browser.HasCss(selector);
}

		public void AssertNotExists(string existsSelector, string notExistsSelector)
		{
			_browser.HasNoCss(existsSelector);
			_browser.HasNoCss(notExistsSelector);
		}

		public void AssertContains(string selector, string text)
		{
			_browser.FindCss(selector).HasContent(text);
		}

		public void AssertNotContains(string selector, string text)
		{
			_browser.FindCss(selector).HasNoContent(text);
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
	}
}