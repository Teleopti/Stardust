using System;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver
{
	public interface IBrowserInteractions
	{
		string Javascript(string javascript);

		void GoTo(string uri);

		void Click(string selector);
		void ClickVisibleOnly(string selector);
		void ClickContaining(string selector, string text);

		void Clear(string selector);
		void FillWith(string selector, string value);
		void PressEnter(string selector);
		void HoverOver(string selector, string value = null);

		void AssertExists(string selector);
		void AssertNotExists(string existsSelector, string notExistsSelector);

		void AssertXPathExists(string xpath);

		void AssertAnyContains(string selector, string text);
		void AssertNoContains(string existsSelector, string notExistsSelector, string text);
		void AssertFirstContains(string selector, string text);
		void AssertFirstNotContains(string selector, string text);

		void AssertInputValue(string selector, string value);

		void AssertUrlContains(string url);
		void AssertUrlNotContains(string urlContains, string urlNotContains);

		void AssertJavascriptResultContains(string javascript, string text);

		void DumpInfo(Action<string> writer);
		void DumpUrl(Action<string> writer);
	    void CloseWindow(string name);
		void DragnDrop(string selector, int x, int y);
	}
}
