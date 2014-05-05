using System;
using System.Text;
using System.Text.RegularExpressions;
using Coypu;
using NUnit.Framework;
using NUnit.Framework.Constraints;

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
			_browser.FindCss(selector, options()).Click(options());
		}

		public void ClickContaining(string selector, string text)
		{
			_browser.FindCss(selector, new Regex(Regex.Escape(text)), options()).Click(options());
		}
		
		public void AssertExists(string selector)
		{
			assert(_browser.HasCss(selector, options()), Is.True, "Could not find element matching selector " + selector);
		}

		public void AssertNotExists(string existsSelector, string notExistsSelector)
		{
			AssertExists(existsSelector);
			assert(_browser.HasNoCss(notExistsSelector, options()), Is.True, "Found element matching selector " + notExistsSelector + " although I shouldnt");
		}

		public void AssertAnyContains(string selector, string text)
		{
			Console.WriteLine("Assert exists element match selector \"{0}\" contain text \"{1}\"", selector, text);
			var regex = new Regex(Regex.Escape(text));
			var hasCss = _browser.HasCss(selector, regex, options());
			var message = string.Format("Could not find element matching selector \"{0}\" with text \"{1}\"", selector, text);
			assert(hasCss, Is.True, message);
		}

		public void AssertFirstContains(string selector, string text)
		{
			Console.WriteLine("Assert first element match selector \"{0}\" contain text \"{1}\"", selector, text);
			assert(_browser.FindCss(selector, options()).HasContentMatch(new Regex(Regex.Escape(text))), Is.True, "Failed to assert that " + selector + " contained text " + text);
		}

		public void AssertFirstNotContains(string selector, string text)
		{
			assert(_browser.FindCss(selector, options()).HasNoContentMatch(new Regex(Regex.Escape(text))), Is.True, "Failed to assert that " + selector + " did not contain text " + text);
		}

		public void AssertUrlContains(string url)
		{
			eventualAssert(() => _browser.Location.ToString(), Is.StringContaining(url), "Failed to assert that current url contains " + url);
		}

		public void AssertUrlNotContains(string urlContains, string urlNotContains)
		{
			AssertUrlContains(urlContains);
			eventualAssert(() => _browser.Location.ToString(), Is.Not.StringContaining(urlNotContains), "Failed to assert that current url did not contain " + urlNotContains);
		}

		public void AssertJavascriptResultContains(string javascript, string text)
		{
			eventualAssert(() => _browser.ExecuteScript(javascript), Is.StringContaining(text), "Failed to assert that javascript " + javascript + " returned a value containing " + text);
		}

		public void DumpInfo(Action<string> writer)
		{
			writer(" Time: ");
			writer(DateTime.Now.ToString());
			writer(" Url: ");
			writer(_browser.Location.ToString());
			writer(" Html: ");
			writer(succeedOrIgnore(() => _browser.ExecuteScript("return $('body').html();")));
		}

		public void DumpUrl(Action<string> writer)
		{
			writer(_browser.Location.ToString());
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

		private void assert<T>(T value, Constraint constraint, string message)
		{
			try
			{
				Assert.That(value, constraint);
			}
			catch (AssertionException)
			{
				Assert.Fail(buildMessage(message));
			}
		}

		private void eventualAssert<T>(Func<T> value, Constraint constraint, string message)
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