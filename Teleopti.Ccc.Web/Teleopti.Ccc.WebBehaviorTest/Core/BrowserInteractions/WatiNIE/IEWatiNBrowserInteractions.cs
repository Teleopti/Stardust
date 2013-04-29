using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserInteractions.WatiNIE
{
	public class IEWatiNBrowserInteractions : IBrowserInteractions
	{
		private readonly IE _browser;

		public IEWatiNBrowserInteractions(IE browser)
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

		public void AssertJavascriptResultContains(string javascript, string text)
		{
			EventualAssert.That(() => _browser.Eval(javascript), Is.StringContaining(text));
		}

		public void AssertExists(string selector)
		{
			EventualAssert.That(() => _browser.Element(Find.BySelector(selector)).Exists, Is.True);
		}

		public void AssertNotExists(string existsSelector, string notExistsSelector)
		{
			EventualAssert.That(() => _browser.Element(Find.BySelector(existsSelector)).Exists, Is.True);
			EventualAssert.That(() => _browser.Element(Find.BySelector(notExistsSelector)).Exists, Is.False);
		}

		public void AssertContains(string selector, string text)
		{
			// use assertExists(selector:contains(text)) here instead?
			// should be faster with better compatibility.
			EventualAssert.That(() => _browser.Element(Find.BySelector(selector)).Text, Is.StringContaining(text));
		}

	}
}