using System;

namespace Teleopti.Ccc.TestCommon.Web.StartWeb.BrowserDriver
{
	public class NoBrowserInteractions : IBrowserInteractions
	{
		private const string notAllowedAction = "Not allowed action on this browser interaction.";

		public string Javascript(string javascript)
		{
			return null;
		}

		public void GoTo(string uri)
		{
		}

		public void GoTo(string uri, string assertUrlContains)
		{
		}

		public void Click(string selector)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void ClickVisibleOnly(string selector)
		{
			throw new NotImplementedException();
		}

		public void ClickContaining(string selector, string text)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void Clear(string selector)
		{
			throw new NotImplementedException();
		}

		public void FillWith (string selector, string value)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void PressEnter(string selector)
		{
			throw new NotImplementedException();
		}

		public void HoverOver(string selector, string value)
		{
			throw new NotImplementedException();
		}		

		public void AssertExists(string selector)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void AssertNotExists(string existsSelector, string notExistsSelector)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void AssertEventualExists(string selector)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void AssertEventualNotExists(string existsSelector, string notExistsSelector)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void AssertAnyContains(string selector, string text)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void AssertNoContains(string existsSelector, string notExistsSelector, string text)
		{
			throw new NotImplementedException();
		}

		public void AssertFirstContains(string selector, string text)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void AssertFirstNotContains(string selector, string text)
		{
			throw new NotSupportedException(notAllowedAction);
		}

		public void AssertInputValue(string selector, string value)
		{
			throw new NotImplementedException();
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