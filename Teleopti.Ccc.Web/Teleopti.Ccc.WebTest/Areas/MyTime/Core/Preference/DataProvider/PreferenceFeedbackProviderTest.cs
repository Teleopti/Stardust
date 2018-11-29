using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;


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
			var nightRestChecker = MockRepository.GenerateMock<IPreferenceNightRestChecker>();
			var personRuleSetBagProvider = MockRepository.GenerateMock<IPersonRuleSetBagProvider>();
			var bag = new RuleSetBag();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today))).Return(new[] {scheduleDay});
			personRuleSetBagProvider.Stub(x => x.ForDate(null, DateOnly.Today)).Return(bag);

			var target = new PreferenceFeedbackProvider(workTimeMinMaxCalculator, MockRepository.GenerateMock<ILoggedOnUser>(), scheduleProvider, nightRestChecker, personRuleSetBagProvider);

			target.WorkTimeMinMaxForDate(DateOnly.Today);

			workTimeMinMaxCalculator.AssertWasCalled(x => x.WorkTimeMinMax(DateOnly.Today, bag, scheduleDay));
		}

		[Test]
		public void ShouldReturnWorkTimeMinMaxForScheduleDay()
		{
			var workTimeMinMaxCalculator = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var nightRestChecker = MockRepository.GenerateMock<IPreferenceNightRestChecker>();
			var provider = MockRepository.GenerateMock<IPersonRuleSetBagProvider>();

			var workTimeMinMax = new WorkTimeMinMax();
			var person = new Person();
			var bag = new RuleSetBag();

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			provider.Stub(x => x.ForDate(person, DateOnly.Today)).Return(bag);

			workTimeMinMaxCalculator.Stub(x => x.WorkTimeMinMax(DateOnly.Today, bag, scheduleDay)).Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

			var target = new PreferenceFeedbackProvider(workTimeMinMaxCalculator, loggedOnUser, null, nightRestChecker, provider);

			var result = target.WorkTimeMinMaxForDate(DateOnly.Today, scheduleDay);

			result.WorkTimeMinMax.Should().Be(workTimeMinMax);
		}

	}
}