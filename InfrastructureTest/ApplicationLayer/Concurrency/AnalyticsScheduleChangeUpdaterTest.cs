using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Activity = Teleopti.Ccc.Domain.Scheduling.Activity;
using Person = Teleopti.Ccc.Domain.Common.Person;
using Scenario = Teleopti.Ccc.Domain.Common.Scenario;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Concurrency
{
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	[Toggle(Toggles.ETL_EventbasedDate_39562)]
	[TestFixture(true)]
	[TestFixture(false)]
	public class AnalyticsScheduleChangeUpdaterTest : ISetup, IConfigureToggleManager
	{
		private readonly bool _resourcePlannerSpeedUpEvents48769;
		public AnalyticsScheduleChangeUpdater Target;
		public IPersonRepository PersonRepository;
		public WithUnitOfWork WithUnitOfWork;
		public IScenarioRepository ScenarioRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IActivityRepository ActivityRepository;
		public ITeamRepository TeamRepository;
		public ISiteRepository SiteRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IContractRepository ContractRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public IShiftCategoryRepository ShiftCategoryRepository;

		public AnalyticsScheduleChangeUpdaterTest(bool resourcePlannerSpeedUpEvents48769)
		{
			_resourcePlannerSpeedUpEvents48769 = resourcePlannerSpeedUpEvents48769;
		}
		
		[Test]
		[Timeout(7000)]
		[Ignore("Failing test for #75053")]
		public void ShouldNotHangWhenMultipleThreadsCallingMultipleDates()
		{
			var targetDate = new DateTime(2010, 1, 5, 0,0,0,DateTimeKind.Utc);
			var scenario = new Scenario("_") {DefaultScenario = true};
			var person = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod();
			var personPeriod = person.Period(new DateOnly(targetDate));
			var activity = new Activity("_");
			var shiftCategory = new ShiftCategory("_");
			
			WithUnitOfWork.Do(() =>
			{
				ContractScheduleRepository.Add(personPeriod.PersonContract.ContractSchedule);
				ContractRepository.Add(personPeriod.PersonContract.Contract);
				PartTimePercentageRepository.Add(personPeriod.PersonContract.PartTimePercentage);
				SiteRepository.Add(personPeriod.Team.Site);
				TeamRepository.Add(personPeriod.Team);
				PersonRepository.Add(person);			
				ScenarioRepository.Add(scenario);
				ActivityRepository.Add(activity);
				ShiftCategoryRepository.Add(shiftCategory);
				PersonAssignmentRepository.Add(
					new PersonAssignment(person, scenario, new DateOnly(targetDate))
						.WithLayer(activity, new TimePeriod(8,17))
						.ShiftCategory(shiftCategory));
			});

			var tasksToRunInParallell = Enumerable.Range(0, 5).Select(x =>
				Task.Factory.StartNew(() =>
				{
					Target.Handle(new ScheduleChangedEvent
					{
						StartDateTime = targetDate,
						EndDateTime = targetDate.AddDays(10),
						PersonId = person.Id.Value,
						ScenarioId = scenario.Id.Value
					});
				}));

			Task.WaitAll(tasksToRunInParallell.ToArray());
		}
		
		public void Configure(FakeToggleManager toggleManager)
		{
			if (_resourcePlannerSpeedUpEvents48769)
				toggleManager.Enable(Toggles.ResourcePlanner_SpeedUpEvents_48769);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<fakeAwayAnalyticsStuffTooHardToSetItUp>()
				.For<IAnalyticsScheduleRepository, IIntervalLengthFetcher, IAnalyticsPersonPeriodRepository, IAnalyticsScenarioRepository>();
		}

		private class fakeAwayAnalyticsStuffTooHardToSetItUp : IAnalyticsScheduleRepository, 
			IIntervalLengthFetcher,
			IAnalyticsPersonPeriodRepository,
			IAnalyticsScenarioRepository
		{
			public void PersistFactScheduleBatch(IList<IFactScheduleRow> factScheduleRows)
			{
			}

			public void PersistFactScheduleDayCountRow(IAnalyticsFactScheduleDayCount dayCount)
			{
				throw new NotImplementedException();
			}

			public void DeleteFactSchedule(int dateId, Guid personCode, int scenarioId)
			{
			}

			public void DeleteFactSchedules(IEnumerable<int> dateIds, Guid personCode, int scenarioId)
			{
				throw new NotImplementedException();
			}

			public int ShiftLengthId(int shiftLength)
			{
				return 0;
			}

			public void InsertStageScheduleChangedServicebus(DateOnly date, Guid personId, Guid scenarioId, Guid businessUnitId, DateTime datasourceUpdateDate)
			{
			}

			public void UpdateUnlinkedPersonids(int[] personPeriodIds)
			{
				throw new NotImplementedException();
			}

			public int GetFactScheduleRowCount(int personId)
			{
				throw new NotImplementedException();
			}

			public int GetFactScheduleDayCountRowCount(int personId)
			{
				throw new NotImplementedException();
			}

			public int GetFactScheduleDeviationRowCount(int personId)
			{
				throw new NotImplementedException();
			}

			public IList<IDateWithDuplicate> GetDuplicateDatesForPerson(Guid personCode)
			{
				throw new NotImplementedException();
			}

			public void RunWithExceptionHandling(Action action)
			{
				throw new NotImplementedException();
			}

			public int IntervalLength { get; } = 15;
			public int GetOrCreateSite(Guid siteCode, string siteName, int businessUnitId)
			{
				throw new NotImplementedException();
			}

			public IList<AnalyticsPersonPeriod> GetPersonPeriods(Guid personCode)
			{
				throw new NotImplementedException();
			}

			public AnalyticsPersonPeriod PersonPeriod(Guid personPeriodCode)
			{
				throw new NotImplementedException();
			}

			public void AddOrUpdatePersonPeriod(AnalyticsPersonPeriod personPeriod)
			{
				throw new NotImplementedException();
			}

			public void DeletePersonPeriod(AnalyticsPersonPeriod analyticsPersonPeriod)
			{
				throw new NotImplementedException();
			}

			public IList<AnalyticsBridgeAcdLoginPerson> GetBridgeAcdLoginPersonsForPerson(int personId)
			{
				throw new NotImplementedException();
			}

			public IList<AnalyticsBridgeAcdLoginPerson> GetBridgeAcdLoginPersonsForAcdLoginPersons(int acdLoginId)
			{
				throw new NotImplementedException();
			}

			public void AddBridgeAcdLoginPerson(AnalyticsBridgeAcdLoginPerson bridgeAcdLoginPerson)
			{
				throw new NotImplementedException();
			}

			public void DeleteBridgeAcdLoginPerson(int acdLoginId, int personId)
			{
				throw new NotImplementedException();
			}

			public void UpdatePersonNames(CommonNameDescriptionSetting commonNameDescriptionSetting, Guid businessUnitCode)
			{
				throw new NotImplementedException();
			}

			public IAnalyticsPersonBusinessUnit PersonAndBusinessUnit(Guid personPeriodCode)
			{
				return new AnalyticsPersonBusinessUnit();
			}

			public void UpdateValidToLocalDateIds(IAnalyticsDate maxDate)
			{
				throw new NotImplementedException();
			}

			public void UpdateTeamName(Guid teamCode, string teamName)
			{
				throw new NotImplementedException();
			}

			public void UpdateSiteName(Guid siteCode, string siteName)
			{
				throw new NotImplementedException();
			}

			public void AddScenario(AnalyticsScenario scenario)
			{
				throw new NotImplementedException();
			}

			public void UpdateScenario(AnalyticsScenario scenario)
			{
				throw new NotImplementedException();
			}

			public IList<AnalyticsScenario> Scenarios()
			{
				throw new NotImplementedException();
			}

			public AnalyticsScenario Get(Guid scenarioCode)
			{
				return new AnalyticsScenario();
			}
		}
	}
}