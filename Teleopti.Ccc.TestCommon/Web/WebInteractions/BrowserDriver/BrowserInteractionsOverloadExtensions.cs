﻿namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver
{
	public static class BrowserInteractionsOverloadExtensions
	{
		public static string Javascript(this IBrowserInteractions interactions, string javascript, params object[] args)
		{
			return interactions.Javascript_IsFlaky(string.Format(javascript, args));
		}

		public static void AssertExists(this IBrowserInteractions interactions, string selector, params object[] args)
		{
			interactions.AssertExists(string.Format(selector, args));
		}
	}
}