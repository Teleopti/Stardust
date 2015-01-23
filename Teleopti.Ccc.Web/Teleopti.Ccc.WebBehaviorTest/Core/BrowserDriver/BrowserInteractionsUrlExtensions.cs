namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public static class BrowserInteractionsUrlExtensions
	{
		// might not be safe...
		public static bool UrlContains(this IBrowserInteractions interactions, string match)
		{
			var currentUrl = "";
			interactions.DumpUrl(u => currentUrl = u);
			return currentUrl.Contains(match);
		}
	}
}