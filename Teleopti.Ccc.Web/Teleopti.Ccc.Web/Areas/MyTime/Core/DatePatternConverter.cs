namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public static class DatePatternConverter
	{
		public static string TojQueryPattern(string dotNetPattern)
		{
			string jQueryPatter = dotNetPattern;

			jQueryPatter = jQueryPatter.Replace("dddd", "DD");
			jQueryPatter = jQueryPatter.Replace("ddd", "D");

			if (jQueryPatter.Contains("MMMM"))
			{
				jQueryPatter = jQueryPatter.Replace("MMMM", "MM");
			}
			else if (jQueryPatter.Contains("MMM"))
			{
				jQueryPatter = jQueryPatter.Replace("MMM", "M");
			}
			else if (jQueryPatter.Contains("MM"))
			{
				jQueryPatter = jQueryPatter.Replace("MM", "mm");
			}
			else
			{
				jQueryPatter = jQueryPatter.Replace("M", "m");
			}

			jQueryPatter = jQueryPatter.Contains("yyyy") ? jQueryPatter.Replace("yyyy", "yy") : jQueryPatter.Replace("yy", "y");

			return jQueryPatter;
		}
	}
}