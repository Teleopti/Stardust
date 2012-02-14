using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core;

namespace Teleopti.Ccc.WebTest.Core
{
	[TestFixture]
	public class DatePatternConverterTest
	{
		[Test]
		public void ShouldConvertSEFormat()
		{
			const string dotNetPattern = "yyyy-MM-dd";
			const string jQueryPattern = "yy-mm-dd";

			DatePatternConverter.TojQueryPattern(dotNetPattern).Should().Be(jQueryPattern);
		}

		[Test]
		public void ShouldConvertUSFormat()
		{
			const string dotNetPattern = "MM/dd/yyyy";
			const string jQueryPattern = "mm/dd/yy";

			DatePatternConverter.TojQueryPattern(dotNetPattern).Should().Be(jQueryPattern);
		}
	}
}
