using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferenceFeedbackProviderTest
	{
		[Test]
		public void ShouldReturnWorkTimeMinMaxForDate()
		{
			var workTimeMinMaxCalculator = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var personPeriod = MockRepository.GenerateMock<IPersonPeriod>();
			var person = MockRepository.GenerateMock<IPerson>();
			var ruleSetBag = MockRepository.GenerateMock<IRuleSetBag>();
			var scenario = MockRepository.GenerateMock<IScenario>();
			var scenarioProvider = MockRepository.GenerateMock<IScenarioProvider>();
			scenarioProvider.Stub(x => x.DefaultScenario()).Return(scenario);
			
			person.Stub(x => x.PersonPeriods(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today))).Return(new[] {personPeriod});
			personPeriod.Stub(x => x.RuleSetBag).Return(ruleSetBag);
			var workTimeMinMax = new WorkTimeMinMax();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			workTimeMinMaxCalculator.Stub(x => x.WorkTimeMinMax(person, DateOnly.Today, scenario)).Return(workTimeMinMax);

			var target = new PreferenceFeedbackProvider(workTimeMinMaxCalculator, loggedOnUser, scenarioProvider);

			var result = target.WorkTimeMinMaxForDate(DateOnly.Today);

			result.Should().Be(workTimeMinMax);
		}

		[Test]
		public void ShouldReturnNullIfNoPersonPeriod()
		{
			var workTimeMinMaxCalculator = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			var scenarioProvider = MockRepository.GenerateMock<IScenarioProvider>();

			var target = new PreferenceFeedbackProvider(workTimeMinMaxCalculator, loggedOnUser, scenarioProvider);

			var result = target.WorkTimeMinMaxForDate(DateOnly.Today);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullIfNoRuleSetBag()
		{
			var workTimeMinMaxCalculator = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var personPeriod = MockRepository.GenerateMock<IPersonPeriod>();
			var person = MockRepository.GenerateMock<IPerson>();
			person.Stub(x => x.PersonPeriods(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today))).Return(new[] { personPeriod });
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			var scenario = MockRepository.GenerateMock<IScenario>();
			var scenarioProvider = MockRepository.GenerateMock<IScenarioProvider>();
			scenarioProvider.Stub(x => x.DefaultScenario()).Return(scenario);
			
			var workTimeMinMax = new WorkTimeMinMax();
			workTimeMinMaxCalculator.Stub(x => x.WorkTimeMinMax(null, DateOnly.Today, scenario)).Return(workTimeMinMax);

			var target = new PreferenceFeedbackProvider(workTimeMinMaxCalculator, loggedOnUser, scenarioProvider);

			var result = target.WorkTimeMinMaxForDate(DateOnly.Today);

			result.Should().Be.Null();
		}

	}
}