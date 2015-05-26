using System;
using System.Text.RegularExpressions;
using Coypu;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Shows = Coypu.NUnit.Matchers.Shows;

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
					WaitBeforeClick = _configuration.WaitBeforeClick,
					Match = _configuration.Match,
					TextPrecision = _configuration.TextPrecision
				};
			}
		}

		public object Javascript(string javascript)
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

		public void ClickContaining(string selector, string text)
		{
			_browser.FindCss(selector, new Regex(Regex.Escape(text)), options()).Click(options());
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

		public void HoverOver(string selector, string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				_browser.FindCss(selector, options()).Hover();
			}
			else
			{
				_browser.FindCss(selector, value, options()).Hover();
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
			var globalOption = options();
			var option = new Options
			{
				ConsiderInvisibleElements = true,
				RetryInterval = globalOption.RetryInterval,
				Timeout = globalOption.Timeout,
				WaitBeforeClick = globalOption.WaitBeforeClick,
				Match = globalOption.Match,
				TextPrecision = globalOption.TextPrecision
			};
			assert(_browser.FindCss(selector, option).Exists(), Is.True, "Could not find element matching selector " + selector);
			/*Assert.That(_browser, Shows.Css(selector, options()), "Could not find element matching selector " + selector);*/
		}

		public void AssertNotExists(string existsSelector, string notExistsSelector)
		{
			AssertExists(existsSelector);

			var globalOption = options();
			var option = new Options
			{
				ConsiderInvisibleElements = true,
				RetryInterval = globalOption.RetryInterval,
				Timeout = globalOption.Timeout,
				WaitBeforeClick = globalOption.WaitBeforeClick,
				Match = globalOption.Match,
				TextPrecision = globalOption.TextPrecision
			};

			assert(_browser.FindCss(notExistsSelector, option).Missing(), Is.True, "Found element matching selector " + notExistsSelector + " although I shouldnt");
			/*Assert.That(_browser, Shows.No.Css(notExistsSelector, options()),
				"Found element matching selector " + notExistsSelector + " although I shouldnt");*/
		}

		public void AssertNotVisible(string existsSelector, string notExistsSelector)
		{
			AssertExists(existsSelector);

			var globalOption = options();
			var option = new Options
			{
				ConsiderInvisibleElements = false,
				RetryInterval = globalOption.RetryInterval,
				Timeout = globalOption.Timeout,
				WaitBeforeClick = globalOption.WaitBeforeClick,
				Match = globalOption.Match,
				TextPrecision = globalOption.TextPrecision
			};

			assert(_browser.FindCss(notExistsSelector, option).Missing(), Is.True, "Found visible element matching selector " + notExistsSelector + " although I shouldnt");
		}

		public void AssertAnyContains(string selector, string text)
		{
			Console.WriteLine("Assert exists element match selector \"{0}\" contain text \"{1}\"", selector, text);
			var regex = new Regex(Regex.Escape(text));
			var hasCss = _browser.FindCss(selector, regex, options()).Exists();
			var message = string.Format("Could not find element matching selector \"{0}\" with text \"{1}\"", selector, text);
			assert(hasCss, Is.True, message);
		}

		public void AssertNoContains(string existsSelector, string notExistsSelector, string text)
		{
			AssertExists(existsSelector);
			var regex = new Regex(Regex.Escape(text));

			assert(_browser.FindCss(notExistsSelector, regex, options()).Missing(), Is.True,
				"Failed to assert that " + notExistsSelector + " did not find anything containing text " + text);
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
			eventualAssert(() => _browser.FindCss(selector, options()).Value, Is.EqualTo(value),
				"Failed to assert that input value was " + value);
		}

		public void AssertUrlContains(string url)
		{
			eventualAssert(() => _browser.Location.ToString(), Is.StringContaining(url),
				"Failed to assert that current url contains " + url);
		}

		public void AssertUrlNotContains(string urlContains, string urlNotContains)
		{
			AssertUrlContains(urlContains);
			eventualAssert(() => _browser.Location.ToString(), Is.Not.StringContaining(urlNotContains),
				"Failed to assert that current url did not contain " + urlNotContains);
		}

		public void CloseWindow(string name)
		{
			_browser.FindWindow(name).ExecuteScript("window.close();");
		}

		public void AssertJavascriptResultContains(string javascript, string text)
		{
			eventualAssert(() =>
			{
				var result = _browser.ExecuteScript(javascript);
				return result == null ? string.Empty : result.ToString();
			}, Is.StringContaining(text),
				string.Format("Failed to assert that javascript \"{0}\" returned a value containing \"{1}\"", javascript, text));
		}

		public void DumpInfo(Action<string> writer)
		{
			writer(string.Format(" Time: {0}", DateTime.Now));

			var url = _browser.Location.ToString();
			writer(string.Format(" Url: {0}", url));

			if (url == "about:blank" || url.EndsWith("Test/ClearConnections"))
			{
				return;
			}

			object domSource;
			const string scriptToGetDomSource = "return document.documentElement.outerHTML;";
			try
			{
				domSource = _browser.ExecuteScript(scriptToGetDomSource);
			}
			catch (Exception ex)
			{
				domSource = string.Format("Failed to get DOM source with \"" + scriptToGetDomSource + "\"\r\n{0}",
					ex.Message);
			}

			writer(string.Format(" Html: {0}", domSource));
		}

		public void DumpUrl(Action<string> writer)
		{
			writer(_browser.Location.ToString());
		}

		private Options options()
		{
			return _options ?? _configuration;
		}

		private object retryJavascript(string javascript)
		{
			object result = null;
			_browser.RetryUntilTimeout(() => { result = _browser.ExecuteScript(javascript); }, options());

			return result;
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

		private void eventualAssert<T>(Func<T> value, Constraint constraint, string message)
		{
			EventualAssert.That(value, constraint, () => message, new SeleniumExceptionCatcher());
		}
	}
}