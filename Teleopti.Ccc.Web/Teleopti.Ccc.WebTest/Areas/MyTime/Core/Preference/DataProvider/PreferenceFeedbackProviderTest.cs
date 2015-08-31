using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferenceFeedbackProviderTest
	{
		[Test]
		public void ShouldRetrieveScheduleDayForDate()
		{
			var workTimeMinMaxCalculator = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();
			var scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today))).Return(new[] {scheduleDay});

			var target = new PreferenceFeedbackProvider(workTimeMinMaxCalculator, MockRepository.GenerateMock<ILoggedOnUser>(), scheduleProvider, MockRepository.GenerateMock<IPersonRuleSetBagProvider>());

			target.WorkTimeMinMaxForDate(DateOnly.Today);

			workTimeMinMaxCalculator.AssertWasCalled(x => x.WorkTimeMinMax(DateOnly.Today, null, scheduleDay));
		}

		[Test]
		public void ShouldReturnWorkTimeMinMaxForScheduleDay()
		{
			var workTimeMinMaxCalculator = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();
			var ruleSetBagProvider = MockRepository.GenerateMock<IPersonRuleSetBagProvider>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var workTimeMinMax = new WorkTimeMinMax();
			var person = new Person();
			var ruleSetBag = new RuleSetBag();

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			ruleSetBagProvider.Stub(x => x.ForDate(person,DateOnly.Today)).Return(ruleSetBag);

			workTimeMinMaxCalculator.Stub(x => x.WorkTimeMinMax(DateOnly.Today, ruleSetBag, scheduleDay)).Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

			var target = new PreferenceFeedbackProvider(workTimeMinMaxCalculator, loggedOnUser, null, ruleSetBagProvider);

			var result = target.WorkTimeMinMaxForDate(DateOnly.Today, scheduleDay);

			result.WorkTimeMinMax.Should().Be(workTimeMinMax);
		}
	}
}