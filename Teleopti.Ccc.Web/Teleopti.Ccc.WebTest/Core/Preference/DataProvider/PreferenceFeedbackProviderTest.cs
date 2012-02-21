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
			var personPeriod = new PersonPeriod(DateOnly.Today, MockRepository.GenerateMock<IPersonContract>(), MockRepository.GenerateMock<ITeam>());
			var person = new Person();
			var ruleSetBag = new RuleSetBag();
			person.AddPersonPeriod(personPeriod);
			personPeriod.RuleSetBag = ruleSetBag;
			var workTimeMinMax = new WorkTimeMinMax();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			workTimeMinMaxCalculator.Stub(x => x.WorkTimeMinMax(ruleSetBag, DateOnly.Today)).Return(workTimeMinMax);

			var target = new PreferenceFeedbackProvider(workTimeMinMaxCalculator, loggedOnUser);

			var result = target.WorkTimeMinMaxForDate(DateOnly.Today);

			result.Should().Be(workTimeMinMax);
		}

		[Test]
		public void ShouldReturnNullIfNoPersonPeriod()
		{
			var workTimeMinMaxCalculator = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var target = new PreferenceFeedbackProvider(workTimeMinMaxCalculator, loggedOnUser);

			var result = target.WorkTimeMinMaxForDate(DateOnly.Today);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullIfNoRuleSetBag()
		{
			var workTimeMinMaxCalculator = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var personPeriod = new PersonPeriod(DateOnly.Today, MockRepository.GenerateMock<IPersonContract>(), MockRepository.GenerateMock<ITeam>());
			var person = new Person();
			person.AddPersonPeriod(personPeriod);
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			var workTimeMinMax = new WorkTimeMinMax();
			workTimeMinMaxCalculator.Stub(x => x.WorkTimeMinMax(null, DateOnly.Today)).Return(workTimeMinMax);

			var target = new PreferenceFeedbackProvider(workTimeMinMaxCalculator, loggedOnUser);

			var result = target.WorkTimeMinMaxForDate(DateOnly.Today);

			result.Should().Be.Null();
		}

	}
}