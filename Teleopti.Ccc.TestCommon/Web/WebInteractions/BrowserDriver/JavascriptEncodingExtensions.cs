namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver
{
	public static class JavascriptEncodingExtensions
	{
		public static string JSEncode(this string value)
		{
			return System.Web.HttpUtility.JavaScriptStringEncode(value);
		}
	}
}