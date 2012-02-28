using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MobileReports.Core;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports
{
	using Teleopti.Ccc.WebTest.Areas.MobileReports.TestData;

	[TestFixture]
	public class DateBoxGlobalizationViewModelFactoryTest
	{
		[Test, SetCulture("ar-SA"), SetUICulture("ar-SA")]
		public void ShouldCreateModelFromProvidedArSaCulture()
		{
			var target = new DateBoxGlobalizationViewModelFactory(new CurrentThreadCultureProvider());

			var result = target.CreateDateBoxGlobalizationViewModel();

			result.DateFormat.Should().Be.EqualTo("DD/MM/YYYY");
			result.DateFieldOrder.Should().Have.SameSequenceAs(new[] { "d", "m", "y" });
			result.IsRtl.Should().Be.True();
		}

		[Test, SetCulture("en-US"), SetUICulture("en-US")]
		public void ShouldCreateModelFromProvidedEnUsCulture()
		{
			var target = new DateBoxGlobalizationViewModelFactory(new CurrentThreadCultureProvider());

			var result = target.CreateDateBoxGlobalizationViewModel();

			result.DaysOfWeek[0].Should().Be.EqualTo("Sunday");
			result.DateFormat.Should().Be.EqualTo("mm/dd/YYYY");
			result.DateFieldOrder.Should().Have.SameSequenceAs(new[] {"m", "d", "y"});
			result.CalStartDay.Should().Be.EqualTo(0);
			result.IsRtl.Should().Be.False();
		}

		[Test, SetCulture("sv-SE"), SetUICulture("sv-SE")]
		public void ShouldCreateModelFromProvidedSwedishCulture()
		{
			var target = new DateBoxGlobalizationViewModelFactory(new CurrentThreadCultureProvider());

			var result = target.CreateDateBoxGlobalizationViewModel();

			result.SetDateButtonLabel.Should().Be.EqualTo(Resources.SelectDates);
			result.CalTodayButtonLabel.Should().Be.EqualTo(Resources.Today);
			result.Tooltip.Should().Be.EqualTo(Resources.SelectDates);
			result.DateFieldOrder.Should().Have.SameSequenceAs(new[] {"y", "m", "d"});
			result.CalStartDay.Should().Be.EqualTo(1);
			result.IsRtl.Should().Be.False();
			result.DaysOfWeek[0].Should().Be.EqualTo("söndag");
		}
	}
}