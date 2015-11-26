namespace Teleopti.Ccc.TestCommon.Web.StartWeb.BrowserDriver
{
	public static class BrowserInteractionsKnockoutExtensions
	{
		public static void AssertKnockoutContextContains(this IBrowserInteractions interactions, string selector, string knockoutBinding, string text)
		{
			interactions.AssertJavascriptResultContains("return ko.contextFor($('" + selector + "').get(0)).$data." + knockoutBinding, text);
		}

	}
}