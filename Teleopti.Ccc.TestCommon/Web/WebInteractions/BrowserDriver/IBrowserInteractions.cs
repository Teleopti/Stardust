﻿using System;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver
{
	public interface IBrowserInteractions
	{
		void GoTo(string uri);

		void Click(string selector);
		void ClickVisibleOnly(string selector);
		void ClickContaining(string selector, string text);

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

		void CloseOtherTabs_Experimental();
		void SwitchToLastTab_Experimental();

		void TryUntil_DontUseShouldBeInternal(Action tryThis, Func<bool> until, TimeSpan waitBeforeRetry);
		bool IsVisible_IsFlaky(string selector);
		bool IsExists_IsFlaky(string selector);
		bool IsContain_Flaky(string selector, string text);
		string Javascript_IsFlaky(string javascript);
	}
}