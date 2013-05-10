using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class AbsenceTimeProviderTest
	{
		private DateOnlyPeriod _period;
		private IPerson _personWithPersonPeriod;
		private ILoggedOnUser _loggedOnUser;
		private BudgetGroup _budgetGroup;
		private IScenarioRepository _scenarioRepository;
		private IScenario _scenario;

		[SetUp]
		public void Setup()
		{
			_period = new DateOnlyPeriod(2001, 1, 1, 2001, 1, 7);
			_budgetGroup = new BudgetGroup();

			_scenario = new Scenario("default");
			_scenarioRepository = MockRepository.GenerateStub<IScenarioRepository>();
			_scenarioRepository.Expect(s => s.LoadDefaultScenario()).Return(_scenario).Repeat.Any();

			_personWithPersonPeriod = PersonFactory.CreatePersonWithPersonPeriod(_period.StartDate.AddDays(-20), new List<ISkill>());
			_personWithPersonPeriod.PersonPeriodCollection.First().BudgetGroup = _budgetGroup;
			_loggedOnUser = MockRepository.GenerateStub<ILoggedOnUser>();
			_loggedOnUser.Expect(l => l.CurrentUser()).Return(_personWithPersonPeriod).Repeat.Any();
		}

		[Test]
		public void GetAbsenceTimeForPeriod_WhenNoAbsenceOnAnyday_ShouldReturnAbsenceAgentForEachDayWithZeroUsedAbsence()
		{
			var daysWithoutAnyUsedAbsence = createlistWithPayloadWorkTimesForEachDateWithUsedAbsenceOf(0,_period);
			var scheduleProjectionReadOnlyRepository = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();
			
			scheduleProjectionReadOnlyRepository.Expect(s => s.AbsenceTimePerBudgetGroup(_period, null, null))
												.IgnoreArguments()
												.Return(daysWithoutAnyUsedAbsence);

			var target = createTarget(_loggedOnUser, _scenarioRepository, scheduleProjectionReadOnlyRepository);

			verifyThatEachDayHasZeroUsedAbsence(target.GetAbsenceTimeForPeriod(_period));
			
		}

		[Test]
		public void GetAbsenceTimeForPeriod_ShouldUseThePeriodAndTheDefaultScenario()
		{
			var scheduleProjectionReadOnlyRepository = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();

			scheduleProjectionReadOnlyRepository.Expect(s => s.AbsenceTimePerBudgetGroup(_period, _budgetGroup, _scenario))
												.Return(new List<PayloadWorkTime>());

			var target = createTarget(_loggedOnUser, _scenarioRepository, scheduleProjectionReadOnlyRepository);
			target.GetAbsenceTimeForPeriod(_period);
		
			scheduleProjectionReadOnlyRepository.VerifyAllExpectations();
		}

		[Test]
		public void GetAbsenceTimeForPeriod_ShouldGetTheBudgetGroupFromThePeriod()
		{
			var budgetGroupOutsideThePeriod = new BudgetGroup();
			var personPeriodOutsideThePeriod = PersonPeriodFactory.CreatePersonPeriod(_period.EndDate.AddDays(100));
			personPeriodOutsideThePeriod.BudgetGroup = budgetGroupOutsideThePeriod;

			_personWithPersonPeriod.AddPersonPeriod(personPeriodOutsideThePeriod);

			var scheduleProjectionReadOnlyRepository = MockRepository.GenerateStrictMock<IScheduleProjectionReadOnlyRepository>();

			scheduleProjectionReadOnlyRepository.Expect(s => s.AbsenceTimePerBudgetGroup(_period, _budgetGroup, _scenario))
												.Return(new List<PayloadWorkTime>());

			var target = createTarget(_loggedOnUser, _scenarioRepository, scheduleProjectionReadOnlyRepository);
			target.GetAbsenceTimeForPeriod(_period);

			scheduleProjectionReadOnlyRepository.VerifyAllExpectations();
		}

		[Test]
		public void GetAbsenceTimeForPeriod_WhenUsedAbsenceExist_ShouldSetAbsenceInMinutes()
		{
			const int absenceTimeInMinutes = 240;

			var scheduleProjectionReadOnlyRepository = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();

			scheduleProjectionReadOnlyRepository.Expect(s => s.AbsenceTimePerBudgetGroup(_period, _budgetGroup, _scenario))
												.IgnoreArguments()
												.Return(createlistWithPayloadWorkTimesForEachDateWithUsedAbsenceOf(TimeSpan.FromMinutes(absenceTimeInMinutes).Ticks,_period));

			var target = createTarget(_loggedOnUser, _scenarioRepository, scheduleProjectionReadOnlyRepository);
			var result = target.GetAbsenceTimeForPeriod(_period);

			foreach (var agentAbsence in result)
			{
				Assert.That(absenceTimeInMinutes, Is.EqualTo(agentAbsence.AbsenceTime));
			}
			
		}

		[Test]
		public void GetAbsenceTimeForPeriod_WhenNoBudgetGroupExistsForTheperiod_ShouldSetZeroAbsenceOnAllDays()
		{
			_personWithPersonPeriod.RemoveAllPersonPeriods();

			var target = createTarget(_loggedOnUser, _scenarioRepository, MockRepository.GenerateStub<IScheduleProjectionReadOnlyRepository>());
			var result = target.GetAbsenceTimeForPeriod(_period);

			verifyThatEachDayHasZeroUsedAbsence(result);
		}

		[Test]
		public void GetAbsenceTimeForPeriod_WhenNoAbsenceExistInRepository_ShouldCreateAbsenceDaysForEachDayWithZeroAbsence()
		{
			var scheduleProjectionReadOnlyRepository = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();

			scheduleProjectionReadOnlyRepository.Expect(s => s.AbsenceTimePerBudgetGroup(_period, _budgetGroup, _scenario))
												.IgnoreArguments()
												.Return(new List<PayloadWorkTime>());

			var target = createTarget(_loggedOnUser, _scenarioRepository, scheduleProjectionReadOnlyRepository);
			var result = target.GetAbsenceTimeForPeriod(_period);

			verifyThatEachDayHasZeroUsedAbsence(result);
		}

		[Test]
		public void GetAbsenceTimeForPeriod_WhenThereAreDifferentBudgetGroupsDuringThePeriod_ShouldUseTheBudgetGroupThatsConnectedToTheDate()
		{
			var anotherBudgetGroup = new BudgetGroup();
			var nextStart = new DateOnly(_period.StartDate.AddDays(2));
			var personPeriodWithAnotherBudgetGroup = PersonPeriodFactory.CreatePersonPeriod(nextStart);

			personPeriodWithAnotherBudgetGroup.BudgetGroup = anotherBudgetGroup;
			_personWithPersonPeriod.AddPersonPeriod(personPeriodWithAnotherBudgetGroup);
			var firstExpectedPeriod = new DateOnlyPeriod(_period.StartDate, nextStart.AddDays(-1));
			var secondExpectedPeriod = new DateOnlyPeriod(nextStart, _period.EndDate);

			var scheduleProjectionReadOnlyRepository = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();

			var target = createTarget(_loggedOnUser, _scenarioRepository, scheduleProjectionReadOnlyRepository);

			scheduleProjectionReadOnlyRepository.Expect(
				s => s.AbsenceTimePerBudgetGroup(firstExpectedPeriod, _budgetGroup, _scenario))
												.Return(createlistWithPayloadWorkTimesForEachDateWithUsedAbsenceOf(TimeSpan.FromMinutes(100).Ticks, firstExpectedPeriod));
			scheduleProjectionReadOnlyRepository.Expect(
				s => s.AbsenceTimePerBudgetGroup(secondExpectedPeriod, anotherBudgetGroup, _scenario))
			                                    .Return(createlistWithPayloadWorkTimesForEachDateWithUsedAbsenceOf(0,secondExpectedPeriod));

			var result = target.GetAbsenceTimeForPeriod(_period).ToList();

			Assert.That(result[0].AbsenceTime, Is.EqualTo(100), "2001-01-01");
			Assert.That(result[1].AbsenceTime, Is.EqualTo(100), "2001-01-02");
			Assert.That(result[2].AbsenceTime, Is.EqualTo(0),   "2001-01-03");
			Assert.That(result[3].AbsenceTime, Is.EqualTo(0),   "2001-01-04");
			Assert.That(result[4].AbsenceTime, Is.EqualTo(0),   "2001-01-05");
			Assert.That(result[5].AbsenceTime, Is.EqualTo(0),   "2001-01-06");
			Assert.That(result[5].AbsenceTime, Is.EqualTo(0),   "2001-01-07");

		}


		#region helpers
		private static IEnumerable<PayloadWorkTime> createlistWithPayloadWorkTimesForEachDateWithUsedAbsenceOf(long totalContractTimeForEachDay,DateOnlyPeriod period)
		{
			return period.DayCollection().Select(d => new PayloadWorkTime()
				                                          {
					                                          BelongsToDate = d, 
															  TotalContractTime = totalContractTimeForEachDay
				                                          });

		}

		private static IAbsenceTimeProvider createTarget(ILoggedOnUser loggedOnUser,IScenarioRepository scenarioRepository, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
		{
			return new AbsenceTimeProvider(loggedOnUser, scenarioRepository, scheduleProjectionReadOnlyRepository, new ExtractBudgetGroupPeriods());
		}

		private void verifyThatEachDayHasZeroUsedAbsence(IEnumerable<IAbsenceAgents> agentAbsences)
		{
			CollectionAssert.AreEqual(_period.DayCollection(), agentAbsences.Select(s => new DateOnly(s.Date)), "All dates should be included in the result");

			foreach (var absenceCount in agentAbsences)
			{
				Assert.That(absenceCount.AbsenceTime, Is.EqualTo(0), "Should be zero absence per day");
			}
		}

		#endregion

	}
}