using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[TestFixture]
	public class DatePickerGlobalizationViewModelDataFactoryTest
	{
		private IDatePickerGlobalizationViewModelFactory target;

		[SetUp]
		public void Setup()
		{
			target = new DatePickerGlobalizationViewModelFactory();
		}

		[Test, SetCulture("en-US"), SetUICulture("en-US")]
		public void ShouldCreateModelWithFromEnUsCulture()
		{
			var result = target.CreateDatePickerGlobalizationViewModel();

			result.FirstDay.Should().Be.EqualTo(0);
			result.IsRtl.Should().Be.False();
		}

		[Test, SetCulture("ar-SA"), SetUICulture("ar-SA")]
		public void ShouldCreateModelWithFromArSaCulture()
		{
			var result = target.CreateDatePickerGlobalizationViewModel();

			// some systems 0, some systems 6 !!
			result.FirstDay.Should().Be.EqualTo(CultureInfo.GetCultureInfo("ar-SA").DateTimeFormat.FirstDayOfWeek);
			result.IsRtl.Should().Be.True();
		}
		[Test, SetCulture("sv-SE"), SetUICulture("sv-SE")]
		public void ShouldCreateModelWithFromSvSeCulture()
		{
			var result = target.CreateDatePickerGlobalizationViewModel();

			result.FirstDay.Should().Be.EqualTo(1);
			result.IsRtl.Should().Be.False();
		}
	}

	
}