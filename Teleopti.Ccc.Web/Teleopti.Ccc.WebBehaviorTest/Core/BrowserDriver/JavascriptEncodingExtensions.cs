namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public static class JavascriptEncodingExtensions
	{
		public static string JSEncode(this string value)
		{
			return System.Web.HttpUtility.JavaScriptStringEncode(value);
		}
	}
}