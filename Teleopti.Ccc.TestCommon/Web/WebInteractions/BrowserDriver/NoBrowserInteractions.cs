using System;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver
{
	public class NoBrowserInteractions : IBrowserInteractions
	{
		public void GoTo(string uri)
		{
		}

		public void Click(string selector) => throw new NotImplementedException();
		public void ClickVisibleOnly(string selector) => throw new NotImplementedException();
		public void ClickContaining(string selector, string text) => throw new NotImplementedException();
		public void FillWith(string selector, string value) => throw new NotImplementedException();
		public void PressEnter(string selector) => throw new NotImplementedException();
		public void HoverOver(string selector, string value) => throw new NotImplementedException();
		public void AssertExists(string selector) => throw new NotImplementedException();
		public void AssertNotExists(string existsSelector, string notExistsSelector) => throw new NotImplementedException();
		public void AssertXPathExists(string xpath) => throw new NotImplementedException();
		public void AssertAnyContains(string selector, string text) => throw new NotImplementedException();
		public void AssertNoContains(string existsSelector, string notExistsSelector, string text) => throw new NotImplementedException();
		public void AssertFirstContains(string selector, string text) => throw new NotImplementedException();
		public void AssertFirstNotContains(string selector, string text) => throw new NotImplementedException();
		public void AssertInputValue(string selector, string value) => throw new NotImplementedException();
		public void AssertUrlContains(string url) => throw new NotImplementedException();
		public void AssertUrlNotContains(string urlContains, string urlNotContains) => throw new NotImplementedException();
		public void AssertJavascriptResultContains(string javascript, string text) => throw new NotImplementedException();

		public void DumpInfo(Action<string> writer)
		{
		}

		public void DumpUrl(Action<string> writer)
		{
		}

		public void CloseWindow(string name)
		{
		}

		public void TryUntil_DontUseShouldBeInternal(Action tryThis, Func<bool> until, TimeSpan waitBeforeRetry) => throw new NotImplementedException();
		public bool IsVisible_IsFlaky(string selector) => throw new NotImplementedException();
		public bool IsExists_IsFlaky(string selector) => throw new NotImplementedException();
		public bool IsContain_Flaky(string selector, string text) => throw new NotImplementedException();
		public string Javascript_IsFlaky(string javascript) => null;
	}
}