using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradePeriodViewModelMapperTest
	{
		[Test]
		public void ShouldMapHasWorkflowControlSetToFalse()
		{
			var mapper = new ShiftTradePeriodViewModelMapper();
			var result = mapper.Map(null, MockRepository.GenerateMock<INow>());

			result.HasWorkflowControlSet.Should().Be.False();
		}

		[Test]
		public void ShouldMapHasWorkflowControlSetToTrue()
		{
			var mapper = new ShiftTradePeriodViewModelMapper();
			var result = mapper.Map(new WorkflowControlSet(), MockRepository.GenerateMock<INow>());

			result.HasWorkflowControlSet.Should().Be.True();
		}

		[Test]
		public void ShouldMapOpenPeriod()
		{
			var mapper = new ShiftTradePeriodViewModelMapper();
			var result = mapper.Map(new WorkflowControlSet { ShiftTradeOpenPeriodDaysForward = new MinMax<int>(2, 8) }, MockRepository.GenerateMock<INow>());

			result.OpenPeriodRelativeStart.Should().Be.EqualTo(2);
			result.OpenPeriodRelativeEnd.Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldMapNow()
		{
			var mapper = new ShiftTradePeriodViewModelMapper();
			var now = MockRepository.GenerateMock<INow>();
			var dateOnly = new DateOnly(2001, 1, 1);

			now.Stub(x => x.DateOnly()).Return(dateOnly);

			var result = mapper.Map(new WorkflowControlSet(), now);

			result.NowYear.Should().Be.EqualTo(dateOnly.Year);
			result.NowMonth.Should().Be.EqualTo(dateOnly.Month);
			result.NowDay.Should().Be.EqualTo(dateOnly.Day);
		}

		[Test, SetCulture("ar-SA")]
		public void ShouldMapNowWithArabicDateFormat()
		{
			var arabicCalendar = new UmAlQuraCalendar();
			var mapper = new ShiftTradePeriodViewModelMapper();
			var now = MockRepository.GenerateMock<INow>();
			var arabicDate = new DateTime(1435, 1, 1, arabicCalendar);

			now.Stub(x => x.DateOnly()).Return(new DateOnly(arabicDate));

			var result = mapper.Map(new WorkflowControlSet(), now);

			result.NowYear.Should().Be.EqualTo(arabicCalendar.GetYear(arabicDate));
			result.NowMonth.Should().Be.EqualTo(arabicCalendar.GetMonth(arabicDate));
			result.NowDay.Should().Be.EqualTo(arabicCalendar.GetDayOfMonth(arabicDate));
		}
	}
}
