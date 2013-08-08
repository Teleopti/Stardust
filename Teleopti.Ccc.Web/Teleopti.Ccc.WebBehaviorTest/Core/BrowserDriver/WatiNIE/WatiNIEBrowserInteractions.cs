﻿using System;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.WatiNIE
{
	public class WatiNIEBrowserInteractions : IBrowserInteractions
	{
		private readonly IE _browser;
		private readonly IExceptionCatcher exceptionCatcher = new WatiNIEExceptionCatcher();

		public WatiNIEBrowserInteractions(IE browser)
		{
			_browser = browser;
		}

		public object Javascript(string javascript)
		{
			return browserOperation(() => runJavascriptAndAvoidWatiNsIncorrectEscapingInItsEvalFunction(javascript), "Failed to execute javascript " + javascript);
		}

		public void GoToWaitForCompleted(string uri)
		{
			browserOperation(() => _browser.GoTo(uri), "Failed to goto url " + uri + " and wait for completion");
		}

		public void GoToWaitForUrlAssert(string uri, string assertUrlContains)
		{
			browserOperation(() => _browser.GoToNoWait(uri), "Failed to goto url " + uri + " without waiting");
			AssertUrlContains(assertUrlContains);
		}

		public void Click(string selector)
		{
			validateSelector(selector);
			_browser.Element(Find.BySelector(selector)).EventualClick();
		}

		public void ClickContaining(string selector, string text)
		{
			validateSelector(selector);
			selector = string.Format(selector + ":contains('{0}')", text);
			_browser.Element(Find.BySelector(selector)).EventualClick();
		}

		public void AssertUrlContains(string url)
		{
			browserAssert(() => _browser.Url, Is.StringContaining(url), "Failed to assert that current url contains " + url);
		}

		public void AssertUrlNotContains(string urlContains, string urlNotContains)
		{
			AssertUrlContains(urlContains);
			browserAssert(() => _browser.Url, Is.Not.StringContaining(urlNotContains), "Failed to assert that current url did not contain " + urlNotContains);
		}

		public void AssertJavascriptResultContains(string javascript, string text)
		{
			object value = "";
			browserAssert(() =>
				{
					value = runJavascriptAndAvoidWatiNsIncorrectEscapingInItsEvalFunction(javascript);
					return value;
				}, 
				Is.StringContaining(text), 
				() => "Failed to assert that javascript " + javascript + " returned a value containing " + text + ". Last value returned was " + value);
		}

		public void AssertExists(string selector)
		{
			validateSelector(selector);
			browserAssert(() => _browser.Element(Find.BySelector(selector)).Exists, Is.True, "Could not find element matching selector " + selector);
		}

		public void AssertNotExists(string existsSelector, string notExistsSelector)
		{
			validateSelector(existsSelector);
			validateSelector(notExistsSelector);
			AssertExists(existsSelector);
			browserAssert(() => _browser.Element(Find.BySelector(notExistsSelector)).Exists, Is.False, "Found element matching selector " + notExistsSelector + " although I shouldnt");
		}

		public void AssertAnyContains(string selector, string text)
		{
			validateSelector(selector);
			selector = string.Format(selector + ":contains('{0}')", text);
			browserAssert(() => _browser.Element(Find.BySelector(selector)).Exists, Is.True, "Could not find element matching selector " + selector);
		}

		public void AssertFirstContains(string selector, string text)
		{
			validateSelector(selector);
			// use assertExists(selector:contains(text)) here instead?
			// should be faster with better compatibility.
			browserAssert(() => _browser.Element(Find.BySelector(selector)).Text, Is.StringContaining(text), "Failed to assert that " + selector + " contained text " + text);
		}

		public void AssertFirstNotContains(string selector, string text)
		{
			validateSelector(selector);
			browserAssert(() => _browser.Element(Find.BySelector(selector)).Text, Is.Not.StringContaining(text), "Failed to assert that " + selector + " did not contain text " + text);
		}

		public void DumpInfo(Action<string> writer)
		{
			writer(" Time: ");
			writer(DateTime.Now.ToString());
			writer(" Url: ");
			writer(succeedOrIgnore(() => Browser.Current.Url));
			writer(" Html: ");
			writer(succeedOrIgnore(() => Browser.Current.Html));
			//writer(" Text: ");
			//writer(tryOperation(() => Browser.Current.Text));
		}

		public void DumpUrl(Action<string> writer)
		{
			writer(succeedOrIgnore(() => Browser.Current.Url));
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

		private static string succeedOrIgnore(Func<string> operation)
		{
			try
			{
				return operation();
			}
			catch (Exception)
			{
				return "Failed";
			}
		}

		private void browserOperation(Action operation, string message)
		{
			try
			{
				Retrying.Action(operation, exceptionCatcher);
			}
			catch (Exception ex)
			{
				throw new BrowserInteractionException(buildMessage(message), ex);
			}
		}

		private void browserAssert<T>(Func<T> value, Constraint constraint, Func<string> message)
		{
			EventualAssert.That(value, constraint, () => buildMessage(message()), new WatiNIEExceptionCatcher());
		}

		private void browserAssert<T>(Func<T> value, Constraint constraint, string message)
		{
			EventualAssert.That(value, constraint, () => buildMessage(message), new WatiNIEExceptionCatcher());
		}

		private T browserOperation<T>(Func<T> operation, string message)
		{
			try
			{
				return Retrying.Action(operation, exceptionCatcher);
			}
			catch (Exception ex)
			{
				throw new BrowserInteractionException(buildMessage(message), ex);
			}
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