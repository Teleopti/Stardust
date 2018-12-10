using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;


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
			var dateOnlyPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var daysOff = new MinMax<int>(1, 3);

			virtualSchedulePeriodProvider.Stub(x => x.VirtualSchedulePeriodForDate(DateOnly.Today)).Return(virtualSchedulePeriod);
			virtualSchedulePeriod.Stub(x => x.DateOnlyPeriod).Return(dateOnlyPeriod);
			schedulePeriodTargetDayOffCalculator.Stub(x => x.TargetDaysOff(virtualSchedulePeriod)).Return(daysOff);

			var target = new PreferencePeriodFeedbackProvider(
				virtualSchedulePeriodProvider, 
				schedulePeriodTargetDayOffCalculator, 
				MockRepository.GenerateMock<IPeriodScheduledAndRestrictionDaysOff>(), 
				MockRepository.GenerateMock<ISchedulePeriodTargetTimeCalculator>(), 
				MockRepository.GenerateMock<IScheduleProvider>());

			var result = target.PeriodFeedback(DateOnly.Today);

			result.TargetDaysOff.Should().Be(daysOff);
		}

		[Test]
		public void ShouldReturnPossibleResultDaysOff()
		{
			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var periodScheduledAndRestrictionDaysOff = MockRepository.GenerateMock<IPeriodScheduledAndRestrictionDaysOff>();
			var scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			var dateOnlyPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var scheduleDays = new[] {MockRepository.GenerateMock<IScheduleDay>()};

			virtualSchedulePeriodProvider.Stub(x => x.VirtualSchedulePeriodForDate(DateOnly.Today)).Return(virtualSchedulePeriod);
			virtualSchedulePeriod.Stub(x => x.DateOnlyPeriod).Return(dateOnlyPeriod);
			scheduleProvider.Stub(x => x.GetScheduleForPeriod(dateOnlyPeriod)).Return(scheduleDays);
			periodScheduledAndRestrictionDaysOff.Stub(x => x.CalculatedDaysOff(scheduleDays, true, true, false)).Return(3);

			var target = new PreferencePeriodFeedbackProvider(
				virtualSchedulePeriodProvider,
				MockRepository.GenerateMock<ISchedulePeriodTargetDayOffCalculator>(), 
				periodScheduledAndRestrictionDaysOff, 
				MockRepository.GenerateMock<ISchedulePeriodTargetTimeCalculator>(), 
				scheduleProvider);

			var result = target.PeriodFeedback(DateOnly.Today);

			result.PossibleResultDaysOff.Should().Be(3);
		}

		[Test]
		public void ShouldReturnTargetHours()
		{
			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var schedulePeriodTargetTimeCalculator = MockRepository.GenerateMock<ISchedulePeriodTargetTimeCalculator>();
			var scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			var dateOnlyPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var scheduleDays = new[] { MockRepository.GenerateMock<IScheduleDay>() };

			virtualSchedulePeriodProvider.Stub(x => x.VirtualSchedulePeriodForDate(DateOnly.Today)).Return(virtualSchedulePeriod);
			virtualSchedulePeriod.Stub(x => x.DateOnlyPeriod).Return(dateOnlyPeriod);
			scheduleProvider.Stub(x => x.GetScheduleForPeriod(dateOnlyPeriod)).Return(scheduleDays);
			var targetTime = new TimePeriod(TimeSpan.FromHours(2), TimeSpan.FromHours(5));
			schedulePeriodTargetTimeCalculator.Stub(x => x.TargetTimeWithTolerance(virtualSchedulePeriod, scheduleDays)).Return(targetTime);

			var target = new PreferencePeriodFeedbackProvider(
				virtualSchedulePeriodProvider,
				MockRepository.GenerateMock<ISchedulePeriodTargetDayOffCalculator>(),
				MockRepository.GenerateMock<IPeriodScheduledAndRestrictionDaysOff>(), 
				schedulePeriodTargetTimeCalculator, 
				scheduleProvider);

			var result = target.PeriodFeedback(DateOnly.Today);

			result.TargetTime.Should().Be(targetTime);
		}

	}
	
}