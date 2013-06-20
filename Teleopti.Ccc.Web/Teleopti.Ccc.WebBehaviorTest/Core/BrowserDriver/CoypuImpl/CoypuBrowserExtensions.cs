using System;
using Coypu;
using Coypu.Queries;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuImpl
{
	public static class CoypuBrowserExtensions
	{
		public static bool HasJQueryCss(this BrowserSession browser, string selector, Options options)
		{
			return browser.Query(new HasCssJQueryQuery(browser, selector, options));
		}
	}

	public class HasCssJQueryQuery : Query<bool>
	{
		private readonly BrowserSession _browser;
		private readonly string _selector;

		public HasCssJQueryQuery(BrowserSession browser, string selector, Options options)
		{
			_browser = browser;
			_selector = selector;
			Timeout = options.Timeout;
			RetryInterval = options.RetryInterval;
		}

		public TimeSpan Timeout { get; set; }
		public TimeSpan RetryInterval { get; set; }

		public bool ExpectedResult { get { return true; } }

		public bool Run()
		{
			try
			{
				_browser.ExecuteScript(JQueryScript.WhenFoundOrThrow(_selector, "return;"));
				return true;
			}
			catch (MissingHtmlException ex)
			{
				return false;
			}
		}
	}
}