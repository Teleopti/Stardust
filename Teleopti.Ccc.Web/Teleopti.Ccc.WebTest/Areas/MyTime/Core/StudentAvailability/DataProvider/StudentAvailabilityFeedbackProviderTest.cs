using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.StudentAvailability.DataProvider
{
	[TestFixture]
	public class StudentAvailabilityFeedbackProviderTest
	{
		[Test]
		public void ShouldRetrieveScheduleDayForDate()
		{
			var workTimeMinMaxCalculator = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();
			var scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			scheduleProvider.Stub(x => x.GetScheduleForPeriod(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today)))
				.Return(new[] {scheduleDay});

			var target = new StudentAvailabilityFeedbackProvider(workTimeMinMaxCalculator,
				MockRepository.GenerateMock<ILoggedOnUser>(), scheduleProvider);
			target.WorkTimeMinMaxForDate(DateOnly.Today);

			var restrictionOption = new EffectiveRestrictionOptions
			{
				UseStudentAvailability = true
			};

			workTimeMinMaxCalculator.AssertWasCalled(x => x.WorkTimeMinMax(DateOnly.Today, null, scheduleDay, restrictionOption));
		}

		[Test]
		public void ShouldReturnWorkTimeMinMaxForScheduleDay()
		{
			var workTimeMinMaxCalculator = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var workTimeMinMax = new WorkTimeMinMax();
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var restrictionOption = new EffectiveRestrictionOptions
			{
				UseStudentAvailability = true
			};

			workTimeMinMaxCalculator.Stub(x => x.WorkTimeMinMax(DateOnly.Today, person, scheduleDay, restrictionOption))
				.Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

			var target = new StudentAvailabilityFeedbackProvider(workTimeMinMaxCalculator, loggedOnUser, null);
			var result = target.WorkTimeMinMaxForDate(DateOnly.Today, scheduleDay);

			result.WorkTimeMinMax.Should().Be(workTimeMinMax);
		}
	}
}