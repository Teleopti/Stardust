using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;


namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradePeriodViewModelMapperTest
	{
		[Test]
		public void ShouldMapHasWorkflowControlSetToFalse()
		{
			var mapper = new ShiftTradePeriodViewModelMapper();
			var result = mapper.Map(null, MockRepository.GenerateMock<INow>(), TimeZoneInfo.Utc);

			result.HasWorkflowControlSet.Should().Be.False();
		}

		[Test]
		public void ShouldMapHasWorkflowControlSetToTrue()
		{
			var mapper = new ShiftTradePeriodViewModelMapper();
			var result = mapper.Map(new WorkflowControlSet(), MockRepository.GenerateMock<INow>(), TimeZoneInfo.Utc);

			result.HasWorkflowControlSet.Should().Be.True();
		}

		[Test]
		public void ShouldMapOpenPeriod()
		{
			var mapper = new ShiftTradePeriodViewModelMapper();
			var result = mapper.Map(new WorkflowControlSet { ShiftTradeOpenPeriodDaysForward = new MinMax<int>(2, 8) }, MockRepository.GenerateMock<INow>(), TimeZoneInfo.Utc);

			result.OpenPeriodRelativeStart.Should().Be.EqualTo(2);
			result.OpenPeriodRelativeEnd.Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldMapNow()
		{
			var mapper = new ShiftTradePeriodViewModelMapper();
			var date = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Local);
			var now = new MutableNow(date);

			var result = mapper.Map(new WorkflowControlSet(), now, TimeZoneInfo.Utc);

			result.NowYear.Should().Be.EqualTo(date.Year);
			result.NowMonth.Should().Be.EqualTo(date.Month);
			result.NowDay.Should().Be.EqualTo(date.Day);
		}

		[Test, SetCulture("ar-SA")]
		public void ShouldMapNowWithArabicDateFormat()
		{
			var arabicCalendar = new UmAlQuraCalendar();
			var mapper = new ShiftTradePeriodViewModelMapper();
			var arabicDate = new DateTime(1435, 1, 1, arabicCalendar);
			var now = new MutableNow(new DateTime(2013, 11, 4, 0, 0, 0, DateTimeKind.Utc));
			var result = mapper.Map(new WorkflowControlSet(), now, TimeZoneInfoFactory.IranTimeZoneInfo());

			result.NowYear.Should().Be.EqualTo(arabicCalendar.GetYear(arabicDate));
			result.NowMonth.Should().Be.EqualTo(arabicCalendar.GetMonth(arabicDate));
			result.NowDay.Should().Be.EqualTo(arabicCalendar.GetDayOfMonth(arabicDate));
		}

		[Test]
		public void ShouldGetLoggedOnUserDate()
		{
			var mapper = new ShiftTradePeriodViewModelMapper();
			var date = new DateTime(2001, 1, 10, 0, 0, 0, DateTimeKind.Utc);
			var now = new MutableNow(date);

			var result = mapper.Map(new WorkflowControlSet(), now, TimeZoneInfoFactory.DenverTimeZoneInfo());

			result.NowYear.Should().Be.EqualTo(date.Year);
			result.NowMonth.Should().Be.EqualTo(date.Month);
			result.NowDay.Should().Be.EqualTo(date.AddDays(-1).Day);
		}
	}
}
