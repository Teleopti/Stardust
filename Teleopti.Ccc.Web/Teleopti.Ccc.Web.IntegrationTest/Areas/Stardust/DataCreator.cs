//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Teleopti.Ccc.Domain.AgentInfo;
//using Teleopti.Ccc.Domain.Aop;
//using Teleopti.Ccc.Domain.ApplicationLayer;
//using Teleopti.Ccc.Domain.ApplicationLayer.Events;
//using Teleopti.Ccc.Domain.Collection;
//using Teleopti.Ccc.Domain.Common;
//using Teleopti.Ccc.Domain.Common.Time;
//using Teleopti.Ccc.Domain.Helper;
//using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
//using Teleopti.Ccc.Domain.Repositories;
//using Teleopti.Ccc.Domain.Scheduling.Assignment;
//using Teleopti.Ccc.Domain.UnitOfWork;
//using Teleopti.Ccc.TestCommon;
//using Teleopti.Ccc.TestCommon.TestData.Core;
//using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
//using Teleopti.Ccc.TestCommon.TestData.Setups.Default;

//namespace Teleopti.Ccc.Web.IntegrationTest.Areas.Stardust
//{
//	public class DataCreator
//	{
//		private readonly MutableNow _now;
//		//private readonly TestConfiguration _testConfiguration;
//		private readonly IEventPublisher _eventPublisher;
//		private readonly AnalyticsDatabase _analytics;
//		private readonly WithUnitOfWork _withUnitOfWork;
//		private readonly ITeamRepository _teams;
//		private readonly IPersonRepository _persons;
//		private readonly IExternalLogOnRepository _externalLogOns;
//		private readonly IContractRepository _contracts;
//		private readonly IPartTimePercentageRepository _partTimePercentages;
//		private readonly IContractScheduleRepository _contractSchedules;
//		private readonly ICurrentScenario _scenario;
//		private readonly IPersonAssignmentRepository _assignments;
//		private readonly IActivityRepository _activities;
//		private readonly TestDataFactory _data;

//		public DataCreator(
//			MutableNow now,
//			IEventPublisher eventPublisher,
//			AnalyticsDatabase analytics,
//			WithUnitOfWork withUnitOfWork,
//			ITeamRepository teams,
//			IPersonRepository persons,
//			IExternalLogOnRepository externalLogOns,
//			IContractRepository contracts,
//			IPartTimePercentageRepository partTimePercentages,
//			IContractScheduleRepository contractSchedules,
//			ICurrentScenario scenario,
//			IPersonAssignmentRepository assignments,
//			IActivityRepository activities,
//			TestDataFactory data
//		)
//		{
//			_now = now;
//			_eventPublisher = eventPublisher;
//			_analytics = analytics;
//			_withUnitOfWork = withUnitOfWork;
//			_teams = teams;
//			_persons = persons;
//			_externalLogOns = externalLogOns;
//			_contracts = contracts;
//			_partTimePercentages = partTimePercentages;
//			_contractSchedules = contractSchedules;
//			_scenario = scenario;
//			_assignments = assignments;
//			_activities = activities;
//			_data = data;
//		}

//		[TestLog]
//		public virtual void Create()
//		{
//			_now.Is("2016-02-25 08:00".Utc());

//			//CreateAnalyticsData();
//			CreateCommonData();
//			CreateSchedules(CreatePersons());
//			PublisRecurringEvents();
//		}

////		[TestLog]
////		protected virtual void CreateAnalyticsData()
////		{
////			_analytics
////				.WithDataSource(_testConfiguration.DataSourceId, _testConfiguration.SourceId)
////				;
////		}

//		[UnitOfWork]
//		[TestLog]
//		protected virtual void CreateCommonData()
//		{
//			var data = _data;

//			var businessUnit = DefaultBusinessUnit.BusinessUnit;

//			data.Apply(new ScenarioConfigurable {Name = "Default", BusinessUnit = businessUnit.Name});
//			data.Apply(new ActivitySpec {Name = "Phone"});
//			data.Apply(new ActivitySpec {Name = "Lunch"});
//			data.Apply(new ActivitySpec {Name = "Break"});

//			data.Apply(new ContractConfigurable {Name = "contract"});
//			data.Apply(new PartTimePercentageConfigurable {Name = "partTimePercentage"});
//			data.Apply(new ContractScheduleConfigurable {Name = "contractSchedule"});
//			data.Apply(new SiteConfigurable { Name = "site", BusinessUnit = businessUnit.Name });
//			data.Apply(new TeamConfigurable { Name = "team", Site = "site" });
//		}

//		[UnitOfWork]
//		[TestLog]
//		protected virtual IEnumerable<IPerson> CreatePersons()
//		{
//			IList<IPerson> workingRogers = new List<IPerson>();

//			var contract = _contracts.LoadAll().Single(x => x.Description.Name == "contract");
//			var partTimePercentage = _partTimePercentages.LoadAll().Single(x => x.Description.Name == "partTimePercentage");
//			var contractSchedule = _contractSchedules.LoadAll().Single(x => x.Description.Name == "contractSchedule");
//			var teams = _teams.LoadAll();

//			var person = new Person().WithName(new Name("Ola", "Wedmark"));
//			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
//			_persons.Add(person);
//			workingRogers.Add(person);
			
//			var personContract = new PersonContract(contract, partTimePercentage, contractSchedule);
//			var personPeriod = new PersonPeriod("2001-01-01".Date(), personContract, teams.First());
//			person.AddPersonPeriod(personPeriod);
//			return workingRogers;
//		}

//		[TestLog]
//		protected virtual void CreateSchedules(IEnumerable<IPerson> workingRogers)
//		{
//			var phone = _withUnitOfWork.Get(() => _activities.LoadAll().Single(x => x.Name == "Phone"));
//			var brejk = _withUnitOfWork.Get(() => _activities.LoadAll().Single(x => x.Name == "Break"));
//			var lunch = _withUnitOfWork.Get(() => _activities.LoadAll().Single(x => x.Name == "Lunch"));

//			workingRogers
//				.Batch(100)
//				.ForEach(rogers =>
//				{
//					_withUnitOfWork.Do(() =>
//					{
//						rogers.ForEach(roger =>
//						{
//							var assignment1 = new PersonAssignment(roger, _scenario.Current(), "2016-02-25".Date());
//							assignment1.AddActivity(phone, "2016-02-25 08:00".Utc(), "2016-02-25 17:00".Utc());
//							assignment1.AddActivity(brejk, "2016-02-25 10:00".Utc(), "2016-02-25 10:15".Utc());
//							assignment1.AddActivity(lunch, "2016-02-25 11:30".Utc(), "2016-02-25 12:00".Utc());
//							assignment1.AddActivity(brejk, "2016-02-25 15:00".Utc(), "2016-02-25 15:15".Utc());
//							_assignments.Add(assignment1);

//							var assignment2 = new PersonAssignment(roger, _scenario.Current(), "2016-02-26".Date());
//							assignment2.AddActivity(phone, "2016-02-26 08:00".Utc(), "2016-02-26 17:00".Utc());
//							assignment2.AddActivity(brejk, "2016-02-26 10:00".Utc(), "2016-02-26 10:15".Utc());
//							assignment2.AddActivity(lunch, "2016-02-26 11:30".Utc(), "2016-02-26 12:00".Utc());
//							assignment2.AddActivity(brejk, "2016-02-26 15:00".Utc(), "2016-02-26 15:15".Utc());
//							_assignments.Add(assignment2);

//							var assignment3 = new PersonAssignment(roger, _scenario.Current(), "2016-02-27".Date());
//							assignment3.AddActivity(phone, "2016-02-27 08:00".Utc(), "2016-02-27 17:00".Utc());
//							assignment3.AddActivity(brejk, "2016-02-27 10:00".Utc(), "2016-02-27 10:15".Utc());
//							assignment3.AddActivity(lunch, "2016-02-27 11:30".Utc(), "2016-02-27 12:00".Utc());
//							assignment3.AddActivity(brejk, "2016-02-27 15:00".Utc(), "2016-02-27 15:15".Utc());
//							_assignments.Add(assignment3);
//						});
//					});
//				});
//		}

//		[TestLog]
//		protected virtual void PublisRecurringEvents()
//		{
//			// to create/update any data that is periodically kept up to date
//			// like the rule mappings
//			_eventPublisher.Publish(new TenantDayTickEvent(), new TenantHourTickEvent(), new TenantMinuteTickEvent());
//		}

//		public IEnumerable<string> LoggedOffStates()
//		{
//			yield return "LoggedOff";
//		}

//		public IEnumerable<string> PhoneStates()
//		{
//			yield return "Ready";
//			yield return "Phone";
//		}

////		public IEnumerable<ExternalLogon> Logons()
////		{
////			return Enumerable.Range(0, _testConfiguration.NumberOfAgentsInSystem)
////				.Select(roger => new ExternalLogon
////				{
////					DataSourceId = _testConfiguration.DataSourceId,
////					UserCode = $"roger{roger}"
////				});
////		}

////		public IEnumerable<ExternalLogon> LogonsWorking()
////		{
////			return Enumerable.Range(0, _testConfiguration.NumberOfAgentsWorking)
////				.Select(roger => new ExternalLogon
////				{
////					DataSourceId = _testConfiguration.DataSourceId,
////					UserCode = $"roger{roger}"
////				});
////		}
//	}
//}