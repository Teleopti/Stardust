using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;


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
			var provider = MockRepository.GenerateMock<IPersonRuleSetBagProvider>();
			
			var bag = new RuleSetBag();
			
			scheduleProvider.Stub(x => x.GetScheduleForStudentAvailability(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today)))
				.Return(new[] {scheduleDay});
			provider.Stub(x => x.ForDate(null, DateOnly.Today)).Return(bag);

			var target = new StudentAvailabilityFeedbackProvider(workTimeMinMaxCalculator,
				MockRepository.GenerateMock<ILoggedOnUser>(), scheduleProvider, provider);
			target.WorkTimeMinMaxForDate(DateOnly.Today);

			var restrictionOption = new EffectiveRestrictionOptions
			{
				UseStudentAvailability = true
			};

			workTimeMinMaxCalculator.AssertWasCalled(x => x.WorkTimeMinMax(DateOnly.Today, bag, scheduleDay, restrictionOption));
		}

		[Test]
		public void ShouldReturnWorkTimeMinMaxForScheduleDay()
		{
			var workTimeMinMaxCalculator = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var provider = MockRepository.GenerateMock<IPersonRuleSetBagProvider>();
			var workTimeMinMax = new WorkTimeMinMax();
			var person = new Person();
			var bag = new RuleSetBag();

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			provider.Stub(x => x.ForDate(person, DateOnly.Today)).Return(bag);

			var restrictionOption = new EffectiveRestrictionOptions
			{
				UseStudentAvailability = true
			};

			workTimeMinMaxCalculator.Stub(x => x.WorkTimeMinMax(DateOnly.Today, bag, scheduleDay, restrictionOption))
				.Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

			var target = new StudentAvailabilityFeedbackProvider(workTimeMinMaxCalculator, loggedOnUser, null, provider);
			var result = target.WorkTimeMinMaxForDate(DateOnly.Today, scheduleDay);

			result.WorkTimeMinMax.Should().Be(workTimeMinMax);
		}
	}
}