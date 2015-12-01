using System;
using System.Text.RegularExpressions;
using Coypu;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver.CoypuImpl
{
	public class CoypuBrowserInteractions : IBrowserInteractions
	{
		private readonly BrowserSession _browser;
		private readonly SessionConfiguration _configuration;

		private TimeSpan? specialTimeout;

		public CoypuBrowserInteractions(BrowserSession browser, SessionConfiguration configuration)
		{
			_browser = browser;
			_configuration = configuration;
		}

		public void SpecialTimeout(TimeSpan? timeout)
		{
			specialTimeout = timeout;
		}

		private Options newOptions()
		{
			return new Options
			{
				ConsiderInvisibleElements = _configuration.ConsiderInvisibleElements,
				RetryInterval = _configuration.RetryInterval,
				Timeout = _configuration.Timeout,
				WaitBeforeClick = _configuration.WaitBeforeClick,
				Match = _configuration.Match
			};
		}

		private Options optionsVisibleOnly()
		{
			var options = newOptions();
			if (specialTimeout.HasValue)
				options.Timeout = specialTimeout.Value;
			options.ConsiderInvisibleElements = false;
			return options;
		}

		private Options options()
		{
			var options = newOptions();
			if (specialTimeout.HasValue)
				options.Timeout = specialTimeout.Value;
			return options;
		}

		public string Javascript(string javascript)
		{
			return retryJavascript(javascript);
		}

		public void GoTo(string uri)
		{
			_browser.Visit(uri);
		}

		public void Click(string selector)
		{
			_browser.FindCss(selector, options()).Click(options());
		}
		
		public void ClickVisibleOnly(string selector)
		{
			_browser.FindCss(selector, optionsVisibleOnly()).Click(optionsVisibleOnly());
		}

		public void ClickContaining(string selector, string text)
		{
			_browser.FindCss(selector, new Regex(Regex.Escape(text)), optionsVisibleOnly()).Click(options());
		}

		public void Clear(string selector)
		{
			_browser.RetryUntilTimeout(() =>
			{
				var selenium = ((OpenQA.Selenium.Remote.RemoteWebDriver) _browser.Native);
				selenium.FindElement(By.CssSelector(selector)).Clear();
			}, options());
		}

		public void FillWith(string selector, string value)
		{
			_browser.FindCss(selector, options()).FillInWith(value);
		}

		public void PressEnter(string selector)
		{
			_browser.FindCss(selector, options()).SendKeys(Keys.Enter);
		}

		public void HoverOver(string selector, string value = null)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				_browser.FindCss(selector, options()).Hover();
			}
			else
			{
				_browser.FindCss(selector, value).Hover();
			}
		}

		public void DragnDrop(string selector, int x, int y)
		{
			var selenium = ((OpenQA.Selenium.Remote.RemoteWebDriver) _browser.Native);
			var start = selenium.FindElement(By.CssSelector(selector));
			new Actions(selenium).DragAndDropToOffset(start, x, y).Perform();
		}

		public void AssertExists(string selector)
		{
			assert(_browser.FindCss(selector, options()).Exists(options()), Is.True, "Could not find element matching selector " + selector);
		}

		public void AssertNotExists(string existsSelector, string notExistsSelector)
		{
			AssertExists(existsSelector);
			assert(_browser.FindCss(notExistsSelector, options()).Missing(options()), Is.True, "Found element matching selector " + notExistsSelector + " although I shouldnt");
		}

		public void AssertAnyContains(string selector, string text)
		{
			var regex = new Regex(Regex.Escape(text));
			assert(_browser.FindCss(selector, regex, optionsVisibleOnly()).Exists(options()), Is.True, string.Format("Could not find element matching selector \"{0}\" with text \"{1}\"", selector, text));
		}

		public void AssertNoContains(string existsSelector, string notExistsSelector, string text)
		{
			AssertExists(existsSelector);
			var regex = new Regex(Regex.Escape(text));
			assert(_browser.FindCss(notExistsSelector, regex, optionsVisibleOnly()).Missing(options()), Is.True, "Failed to assert that " + notExistsSelector + " did not find anything containing text " + text);
		}

		public void AssertFirstContains(string selector, string text)
		{
			Console.WriteLine("Assert first element match selector \"{0}\" contain text \"{1}\"", selector, text);
			assert(_browser.FindCss(selector, options()).HasContentMatch(new Regex(Regex.Escape(text))), Is.True,
				"Failed to assert that " + selector + " contained text " + text);
		}

		public void AssertFirstNotContains(string selector, string text)
		{
			assert(_browser.FindCss(selector, options()).HasNoContentMatch(new Regex(Regex.Escape(text))), Is.True,
				"Failed to assert that " + selector + " did not contain text " + text);
		}

		public void AssertInputValue(string selector, string value)
		{
			eventualAssert(() => _browser.FindCss(selector, options()).Value, Is.EqualTo(value), () => "Failed to assert that input value was " + value);
		}

		public void AssertUrlContains(string url)
		{
			eventualAssert(() => _browser.Location.ToString(), Is.StringContaining(url), () => "Failed to assert that current url contains " + url);
		}

		public void AssertUrlNotContains(string urlContains, string urlNotContains)
		{
			AssertUrlContains(urlContains);
			eventualAssert(() => _browser.Location.ToString(), Is.Not.StringContaining(urlNotContains), () => "Failed to assert that current url did not contain " + urlNotContains);
		}

		public void CloseWindow(string name)
		{
			_browser.FindWindow(name).ExecuteScript("window.close();");
		}

		public void AssertJavascriptResultContains(string javascript, string text)
		{
			var actual = "";
			eventualAssert(() =>
			{
				var result = _browser.ExecuteScript(javascript);
				actual = result == null ? null : result.ToString();
				return actual;
			},
				Is.StringContaining(text),
				() => string.Format("Failed to assert that javascript \"{0}\" returned a value containing \"{1}\". Last attempt returned \"{2}\". ", javascript, text, actual));
		}

		public void DumpInfo(Action<string> writer)
		{
			writer(" Time: ");
			writer(DateTime.Now.ToString());
			writer(" Url: ");
			writer(_browser.Location.ToString());
			writer(" Html: ");
			try
			{
				writer(_browser.ExecuteScript("return document.documentElement.outerHTML;").ToString());
			}
			catch (Exception)
			{
				writer("Failed");
			}
		}
		
		public void DumpUrl(Action<string> writer)
		{
			writer(_browser.Location.ToString());
		}

		private string retryJavascript(string javascript)
		{
			object result = null;
			_browser.RetryUntilTimeout(() => { result = _browser.ExecuteScript(javascript); }, options());
			return result == null ? null : result.ToString();
		}

		private void assert<T>(T value, Constraint constraint, string message)
		{
			try
			{
				Assert.That(value, constraint);
			}
			catch (AssertionException)
			{
				Assert.Fail(message);
			}
		}

		private void eventualAssert<T>(Func<T> value, Constraint constraint, Func<string> message)
		{
			var o = options();
			EventualAssert.That(value, constraint, message, new SeleniumExceptionCatcher(), o.RetryInterval, o.Timeout);
		}

	}
}