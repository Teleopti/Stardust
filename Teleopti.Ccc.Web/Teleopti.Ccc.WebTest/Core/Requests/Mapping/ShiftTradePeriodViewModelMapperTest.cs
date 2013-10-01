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
			var date = new DateTime(2001, 1, 1);

			now.Stub(x => x.UtcDateTime()).Return(date);

			var result = mapper.Map(new WorkflowControlSet(), now);

			result.NowYear.Should().Be.EqualTo(date.Year);
			result.NowMonth.Should().Be.EqualTo(date.Month);
			result.NowDay.Should().Be.EqualTo(date.Day);
		}

		[Test, SetCulture("ar-SA")]
		public void ShouldMapNowWithArabicDateFormat()
		{
			var arabicCalendar = new UmAlQuraCalendar();
			var mapper = new ShiftTradePeriodViewModelMapper();
			var now = MockRepository.GenerateMock<INow>();
			var arabicDate = new DateTime(1435, 1, 1, arabicCalendar);

			now.Stub(x => x.UtcDateTime()).Return(arabicDate);

			var result = mapper.Map(new WorkflowControlSet(), now);

			result.NowYear.Should().Be.EqualTo(arabicCalendar.GetYear(arabicDate));
			result.NowMonth.Should().Be.EqualTo(arabicCalendar.GetMonth(arabicDate));
			result.NowDay.Should().Be.EqualTo(arabicCalendar.GetDayOfMonth(arabicDate));
		}
	}
}
