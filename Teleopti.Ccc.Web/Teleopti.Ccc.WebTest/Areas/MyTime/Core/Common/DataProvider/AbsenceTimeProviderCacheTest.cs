using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class AbsenceTimeProviderCacheTest
	{
		private DateOnlyPeriod _period;
		private IPerson _personWithPersonPeriod;
		private ILoggedOnUser _loggedOnUser;
		private BudgetGroup _budgetGroup;
		private IScenarioRepository _scenarioRepository;
		private IScenario _scenario;
		private IAbsenceTimeProviderCache _absenceTimeProviderCache;
		private IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;

		[SetUp]
		public void Setup()
		{
			_period = new DateOnlyPeriod (2001, 1, 1, 2001, 1, 7);
			_budgetGroup = new BudgetGroup();

			_scenario = new Scenario ("default");
			_scenarioRepository = MockRepository.GenerateStub<IScenarioRepository>();
			_scenarioRepository.Expect (s => s.LoadDefaultScenario()).Return (_scenario).Repeat.Any();

			_personWithPersonPeriod = PersonFactory.CreatePersonWithPersonPeriod (_period.StartDate.AddDays (-20),
				new List<ISkill>());
			_personWithPersonPeriod.PersonPeriodCollection.First().BudgetGroup = _budgetGroup;
			_loggedOnUser = MockRepository.GenerateStub<ILoggedOnUser>();
			_loggedOnUser.Expect (l => l.CurrentUser()).Return (_personWithPersonPeriod).Repeat.Any();
			_absenceTimeProviderCache = MockRepository.GenerateStub<AbsenceTimeProviderCache>();
			

			_scheduleProjectionReadOnlyPersister = MockRepository.GenerateMock<IScheduleProjectionReadOnlyPersister>();

			_scheduleProjectionReadOnlyPersister.Expect(s => s.AbsenceTimePerBudgetGroup(_period, _budgetGroup, _scenario))
				.Return(new List<PayloadWorkTime>());
		}

		[Test]
		public void EnsureAbsenceTimeProviderUsesAbsenceTimeCache()
		{
			setCaching (true);

			createTargetAndCallTwice();

			_scheduleProjectionReadOnlyPersister.AssertWasCalled(
				s => s.AbsenceTimePerBudgetGroup(_period, _budgetGroup, _scenario), 
				o => o.Repeat.Once());
		}

		[Test]
		public void EnsureAbsenceTimeProviderDoesNotUseAbsenceTimeCache()
		{
			setCaching(false);

			createTargetAndCallTwice();

			_scheduleProjectionReadOnlyPersister.AssertWasCalled(
				s => s.AbsenceTimePerBudgetGroup(_period, _budgetGroup, _scenario),
				o => o.Repeat.Twice());
		}

		#region Utility Methods

		private void createTargetAndCallTwice()
		{
			var target = createTarget (_loggedOnUser, _scenarioRepository, _scheduleProjectionReadOnlyPersister,
				_absenceTimeProviderCache);
			target.GetAbsenceTimeForPeriod (_period);
			target.GetAbsenceTimeForPeriod (_period);
		}

		private void setCaching(bool enabled)
		{
			_absenceTimeProviderCache.Expect (s => s.GetConfigValue()).Return (enabled ? "1" : null).Repeat.Any();
		}


		private static IAbsenceTimeProvider createTarget(ILoggedOnUser loggedOnUser,IScenarioRepository scenarioRepository, IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister, IAbsenceTimeProviderCache absenceTimeProviderCache)
		{
			return new AbsenceTimeProvider(loggedOnUser, scenarioRepository, scheduleProjectionReadOnlyPersister, new ExtractBudgetGroupPeriods(), absenceTimeProviderCache);
		}

		#endregion
	}
}