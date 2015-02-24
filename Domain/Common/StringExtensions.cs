namespace Teleopti.Ccc.Domain.Common
{
	public static class StringExtensions
	{
		public static bool IsAnUrl(this string potentialUrl)
		{
			return potentialUrl.StartsWith("http://") || potentialUrl.StartsWith("https://");
		}
	}
}