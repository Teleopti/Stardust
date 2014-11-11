using System;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public class NoBrowserActivator : IBrowserActivator
	{
		private readonly IBrowserInteractions _browserInteractions;

		public NoBrowserActivator()
		{
			_browserInteractions = new NoBrowserInteractions();
		}

		public void SetTimeout(TimeSpan timeout)
		{
		}

		public void Start(TimeSpan timeout, TimeSpan retry)
		{
		}

		public IBrowserInteractions GetInteractions()
		{
			return _browserInteractions;
		}

		public void Close()
		{
		}
	}

	public class NoBrowserInteractions : IBrowserInteractions
	{
		public string Javascript(string javascript)
		{
			return string.Empty;
		}

		public void GoToWaitForCompleted(string uri)
		{
		}

		public void GoToWaitForUrlAssert(string uri, string assertUrlContains)
		{
		}

		public void Click(string selector)
		{
		}

		public void ClickContaining(string selector, string text)
		{
		}

		public void AssertExists(string selector)
		{
		}

		public void AssertNotExists(string existsSelector, string notExistsSelector)
		{
		}

		public void AssertAnyContains(string selector, string text)
		{
		}

		public void AssertFirstContains(string selector, string text)
		{
		}

		public void AssertFirstNotContains(string selector, string text)
		{
		}

		public void AssertUrlContains(string url)
		{
		}

		public void AssertUrlNotContains(string urlContains, string urlNotContains)
		{
		}

		public void AssertJavascriptResultContains(string javascript, string text)
		{
		}

		public void DumpInfo(Action<string> writer)
		{
		}

		public void DumpUrl(Action<string> writer)
		{
		}

		public void CloseWindow()
		{
		}

		public void DragnDrop(string selector, int x, int y)
		{
		}
	}
}