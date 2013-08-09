using System;
using NUnit.Framework;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.WatiNIE
{
	public class WatiNIEBrowserInteractions : IBrowserInteractions
	{
		private readonly IE _browser;
		private readonly BrowserInteractionsImplementationHelper _helper;

		public WatiNIEBrowserInteractions(IE browser)
		{
			_browser = browser;
			_helper = new BrowserInteractionsImplementationHelper(this, new WatiNIEExceptionCatcher());
		}

		public object Javascript(string javascript)
		{
			return _helper.RetryingOperation(() => runJavascriptAndAvoidWatiNsIncorrectEscapingInItsEvalFunction(javascript), _helper.JavascriptMessage(javascript));
		}

		public void GoToWaitForCompleted(string uri)
		{
			_helper.RetryingOperation(() => _browser.GoTo(uri), _helper.GoToWaitForCompletedMessage(uri));
		}

		public void GoToWaitForUrlAssert(string uri, string assertUrlContains)
		{
			_helper.RetryingOperation(() => _browser.GoToNoWait(uri), _helper.GoToWaitForUrlAssertMessage(uri, assertUrlContains));
			AssertUrlContains(assertUrlContains);
		}

		public void Click(string selector)
		{
			validateSelector(selector);
			_helper.TryOperation(() =>
				{
					var element = _browser.Element(Find.BySelector(selector));
					element.WaitUntilExists(Convert.ToInt32(Timeouts.Timeout.TotalSeconds));
					element.WaitUntil<Element>(e => e.Enabled);
					element.ClickNoWait();
				}, _helper.ClickMessage(selector));
		}

		public void ClickContaining(string selector, string text)
		{
			validateSelector(selector);
			selector = string.Format(selector + ":contains('{0}')", text);
			_helper.TryOperation(() =>
				{
					var element = _browser.Element(Find.BySelector(selector));
					element.WaitUntilExists(Convert.ToInt32(Timeouts.Timeout.TotalSeconds));
					element.WaitUntil<Element>(e => e.Enabled);
					element.ClickNoWait();
				}, _helper.ClickContainingMessage(selector, text));
		}

		public void AssertUrlContains(string url)
		{
			_helper.EventualAssert(() => _browser.Url, Is.StringContaining(url), _helper.AssertUrlContainsMessage(url));
		}

		public void AssertUrlNotContains(string urlContains, string urlNotContains)
		{
			AssertUrlContains(urlContains);
			_helper.EventualAssert(() => _browser.Url, Is.Not.StringContaining(urlNotContains), _helper.AssertUrlNotContainsMessage(urlContains, urlNotContains));
		}

		public void AssertJavascriptResultContains(string javascript, string text)
		{
			object lastActual = "";
			_helper.EventualAssert(() =>
				{
					lastActual = runJavascriptAndAvoidWatiNsIncorrectEscapingInItsEvalFunction(javascript);
					return lastActual;
				}, 
				Is.StringContaining(text), 
				() => _helper.AssertJavascriptResultContainsMessage(javascript, text, lastActual as string));
		}

		public void AssertExists(string selector)
		{
			validateSelector(selector);
			_helper.EventualAssert(() => _browser.Element(Find.BySelector(selector)).Exists, Is.True, _helper.AssertExistsMessage(selector));
		}

		public void AssertNotExists(string existsSelector, string notExistsSelector)
		{
			validateSelector(existsSelector);
			validateSelector(notExistsSelector);
			AssertExists(existsSelector);
			_helper.EventualAssert(() => _browser.Element(Find.BySelector(notExistsSelector)).Exists, Is.False, _helper.AssertNotExistsMessage(existsSelector, notExistsSelector));
		}

		public void AssertAnyContains(string selector, string text)
		{
			validateSelector(selector);
			selector = string.Format(selector + ":contains('{0}')", text);
			_helper.EventualAssert(() => _browser.Element(Find.BySelector(selector)).Exists, Is.True, _helper.AssertAnyContainsMessage(selector, text));
		}

		public void AssertFirstContains(string selector, string text)
		{
			validateSelector(selector);
			_helper.EventualAssert(() => _browser.Element(Find.BySelector(selector)).Text, Is.StringContaining(text), _helper.AssertFirstContainsMessage(selector, text));
		}

		public void AssertFirstNotContains(string selector, string text)
		{
			validateSelector(selector);
			_helper.EventualAssert(() => _browser.Element(Find.BySelector(selector)).Text, Is.Not.StringContaining(text), _helper.AssertFirstNotContainsMessage(selector, text));
		}

		public void DumpInfo(Action<string> writer)
		{
			_helper.DumpInfo(writer, () => _browser.Url, () => _browser.Html);
		}

		public void DumpUrl(Action<string> writer)
		{
			_helper.DumpUrl(writer, () => _browser.Url);
		}

		private void validateSelector(string selector)
		{
			if (selector.Contains(":contains("))
				throw new Exception(":contains() selector should not be used, but was used in " + selector);
		}

		private object runJavascriptAndAvoidWatiNsIncorrectEscapingInItsEvalFunction(string javascript)
		{
			_browser.RunScript("document.driverScriptResult = String( function(){" + javascript + "}() );");
			return _browser.NativeDocument.GetPropertyValue("driverScriptResult");
		}

	}
}