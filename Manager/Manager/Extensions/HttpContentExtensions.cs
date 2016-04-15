using System.Net.Http;

namespace Stardust.Manager.Extensions
{
	public static class HttpContentExtensions
	{
		public static string ContentToString(this HttpContent httpContent)
		{
			var readAsStringAsync = httpContent.ReadAsStringAsync();
			return readAsStringAsync.Result;
		}
	}
}