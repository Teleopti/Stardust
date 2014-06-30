using System;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public interface IBrowserInteractions
	{
		object Javascript(string javascript);

		void GoToWaitForCompleted(string uri);
		void GoToWaitForUrlAssert(string uri, string assertUrlContains);

		void Click(string selector);
		void ClickContaining(string selector, string text);

		void AssertExists(string selector);
		void AssertNotExists(string existsSelector, string notExistsSelector);

		void AssertAnyContains(string selector, string text);
		void AssertFirstContains(string selector, string text);
		void AssertFirstNotContains(string selector, string text);

		void AssertUrlContains(string url);
		void AssertUrlNotContains(string urlContains, string urlNotContains);

		void AssertJavascriptResultContains(string javascript, string text);

		void DumpInfo(Action<string> writer);
		void DumpUrl(Action<string> writer);
	  void CloseWindow();
		void DragnDrop(string selector, int x, int y);
	}
}
