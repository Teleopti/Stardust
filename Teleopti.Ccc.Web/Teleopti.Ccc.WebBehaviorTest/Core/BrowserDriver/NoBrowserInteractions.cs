using System;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public class NoBrowserInteractions : IBrowserInteractions
	{
		private const string notAllowedAction = "Not allowed action on this browser interaction.";

		public string Javascript(string javascript)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void GoToWaitForCompleted(string uri)
		{
		}

		public void GoToWaitForUrlAssert(string uri, string assertUrlContains)
		{
		}

		public void Click(string selector)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void ClickContaining(string selector, string text)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void AssertExists(string selector)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void AssertNotExists(string existsSelector, string notExistsSelector)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void AssertAnyContains(string selector, string text)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void AssertFirstContains(string selector, string text)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void AssertFirstNotContains(string selector, string text)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void AssertUrlContains(string url)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void AssertUrlNotContains(string urlContains, string urlNotContains)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void AssertJavascriptResultContains(string javascript, string text)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void DumpInfo(Action<string> writer)
		{
		}

		public void DumpUrl(Action<string> writer)
		{
		}

		public void CloseWindow(string name)
		{
		}

		public void DragnDrop(string selector, int x, int y)
		{
		}
	}
}