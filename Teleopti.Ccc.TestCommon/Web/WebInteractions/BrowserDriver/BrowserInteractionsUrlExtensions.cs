namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver
{
	public static class BrowserInteractionsUrlExtensions
	{
		// known to not work on all drivers/browsers
		public static bool TryCheckIfUrlContains(this IBrowserInteractions interactions, string match)
		{
			var currentUrl = "";
			interactions.DumpUrl(u => currentUrl = u);
			return currentUrl.Contains(match);
		}
	}
}