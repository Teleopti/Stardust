using System;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.WatiNIE
{
	public class WatiNIEBrowserInteractions : IBrowserInteractions
	{
		private readonly IE _browser;

		public WatiNIEBrowserInteractions(IE browser)
		{
			_browser = browser;
		}

		public object Javascript(string javascript)
		{
			return Retrying.Action(() => _browser.Eval(javascript));
		}

		public void GoToWaitForCompleted(string uri)
		{
			Retrying.Action(() => _browser.GoTo(uri));
		}

		public void GoToWaitForUrlAssert(string uri, string assertUrlContains)
		{
			Retrying.Action(() => _browser.GoToNoWait(uri));
			AssertUrlContains(assertUrlContains);
		}

		public void Click(string selector)
		{
			_browser.Element(Find.BySelector(selector)).EventualClick();
		}


		public void AssertUrlContains(string url)
		{
			EventualAssert.That(() => _browser.Url, Is.StringContaining(url));
		}

		public void AssertUrlNotContains(string urlContains, string urlNotContains)
		{
			AssertUrlContains(urlContains);
			EventualAssert.That(() => _browser.Url, Is.Not.StringContaining(urlNotContains));
		}

		public void AssertJavascriptResultContains(string javascript, string text)
		{
			EventualAssert.That(() => _browser.Eval(javascript), Is.StringContaining(text));
		}

		public void AssertInputValue(string selector, string value)
		{
			EventualAssert.That(() => _browser.TextField(Find.BySelector(selector)).Value, Is.EqualTo(value));
		}

		public void AssertExists(string selector)
		{
			EventualAssert.That(() => _browser.Element(Find.BySelector(selector)).Exists, Is.True, () => buildAssertMessage("Could not find element matching selector " + selector));
		}

		public void AssertNotExists(string existsSelector, string notExistsSelector)
		{
			AssertExists(existsSelector);
			EventualAssert.That(() => _browser.Element(Find.BySelector(notExistsSelector)).Exists, Is.False, () => buildAssertMessage("Found element matching selector " + notExistsSelector + " although I shouldnt"));
		}

		public void AssertContains(string selector, string text)
		{
			// use assertExists(selector:contains(text)) here instead?
			// should be faster with better compatibility.
			EventualAssert.That(() => _browser.Element(Find.BySelector(selector)).Text, Is.StringContaining(text));
		}

		public void AssertNotContains(string selector, string text)
		{
			EventualAssert.That(() => _browser.Element(Find.BySelector(selector)).Text, Is.Not.StringContaining(text));
		}

		public void DumpInfo(Action<string> writer)
		{
			writer(" Time: ");
			writer(DateTime.Now.ToString());
			writer(" Url: ");
			writer(tryOperation(() => Browser.Current.Url, "Failed to get Url"));
			writer(" Html: ");
			writer(tryOperation(() => Browser.Current.Html, "Failed to get Html"));
			//writer(" Text: ");
			//writer(tryOperation(() => Browser.Current.Text, ""));
		}

		private static string tryOperation(Func<string> operation, string @default)
		{
			try
			{
				return operation();
			}
			catch (Exception)
			{
				return @default;
			}
		}

		private string buildAssertMessage(string message)
		{
			var builder = new StringBuilder();
			builder.Append(message);
			builder.Append(" ");
			DumpInfo(s => builder.Append(s));
			return builder.ToString();
		}


	}
}