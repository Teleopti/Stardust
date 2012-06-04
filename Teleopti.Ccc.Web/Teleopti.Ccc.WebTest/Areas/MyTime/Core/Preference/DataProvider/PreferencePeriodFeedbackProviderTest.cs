using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferencePeriodFeedbackProviderTest
	{
		[Test]
		public void ShouldReturnTargetDaysOff()
		{
			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var schedulePeriodTargetDayOffCalculator = MockRepository.GenerateMock<ISchedulePeriodTargetDayOffCalculator>();
			var daysOff = new MinMax<int>(1, 3);
			virtualSchedulePeriodProvider.Stub(x => x.VirtualSchedulePeriodForDate(DateOnly.Today)).Return(virtualSchedulePeriod);
			schedulePeriodTargetDayOffCalculator.Stub(x => x.TargetDaysOff(virtualSchedulePeriod)).Return(daysOff);

			var target = new PreferencePeriodFeedbackProvider(virtualSchedulePeriodProvider, schedulePeriodTargetDayOffCalculator, null, null);

			var result = target.TargetDaysOff(DateOnly.Today);

			result.Should().Be(daysOff);
		}

		[Test]
		public void ShouldReturnPossibleResultDaysOff()
		{
			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			virtualSchedulePeriodProvider.Stub(x => x.VirtualSchedulePeriodForDate(DateOnly.Today)).Return(virtualSchedulePeriod);
			var periodScheduledAndRestrictionDaysOff = MockRepository.GenerateMock<IPeriodScheduledAndRestrictionDaysOff>();
			var scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			var dateOnlyPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var scheduleDays = new[] {MockRepository.GenerateMock<IScheduleDay>()};

			virtualSchedulePeriodProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(dateOnlyPeriod);
			scheduleProvider.Stub(x => x.GetScheduleForPeriod(dateOnlyPeriod)).Return(scheduleDays);
			periodScheduledAndRestrictionDaysOff.Stub(x => x.CalculatedDaysOff(scheduleDays, true, true, false)).Return(3);

			var target = new PreferencePeriodFeedbackProvider(virtualSchedulePeriodProvider, null, periodScheduledAndRestrictionDaysOff, scheduleProvider);

			var result = target.PossibleResultDaysOff(DateOnly.Today);

			result.Should().Be(3);
		}
	}

	
}