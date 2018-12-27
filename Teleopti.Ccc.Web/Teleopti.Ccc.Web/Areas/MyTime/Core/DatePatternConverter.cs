using System.Text;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public static class DatePatternConverter
	{
		public static string TojQueryPattern(string dotNetPattern)
		{
			var jQueryPattern = new StringBuilder(dotNetPattern);
			
			jQueryPattern = jQueryPattern.Replace("dddd", "DD");
			jQueryPattern = jQueryPattern.Replace("ddd", "D");

			if (dotNetPattern.Contains("MMMM"))
			{
				jQueryPattern = jQueryPattern.Replace("MMMM", "MM");
			}
			else if (dotNetPattern.Contains("MMM"))
			{
				jQueryPattern = jQueryPattern.Replace("MMM", "M");
			}
			else if (dotNetPattern.Contains("MM"))
			{
				jQueryPattern = jQueryPattern.Replace("MM", "mm");
			}
			else
			{
				jQueryPattern = jQueryPattern.Replace("M", "m");
			}

			jQueryPattern = dotNetPattern.Contains("yyyy") ? jQueryPattern.Replace("yyyy", "yy") : jQueryPattern;

			return jQueryPattern.ToString();
		}
	}
}