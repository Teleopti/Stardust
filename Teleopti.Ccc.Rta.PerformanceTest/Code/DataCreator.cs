using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class DataCreator
	{
		private readonly MutableNow _now;
		private readonly TestConfiguration _testConfiguration;
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IEventPublisher _eventPublisher;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly AnalyticsDatabase _analytics;
		private readonly Database _database;
		private readonly WithUnitOfWork _withUnitOfWork;
		private readonly ITeamRepository _teams;
		private readonly IPersonRepository _persons;
		private readonly IExternalLogOnRepository _externalLogOns;
		private readonly IContractRepository _contracts;
		private readonly IPartTimePercentageRepository _partTimePercentages;
		private readonly IContractScheduleRepository _contractSchedules;
		private readonly ICurrentScenario _scenario;
		private readonly IPersonAssignmentRepository _assignments;
		private readonly IActivityRepository _activities;

		public DataCreator(
			MutableNow now,
			TestConfiguration testConfiguration,
			ICurrentUnitOfWork unitOfWork,
			IEventPublisher eventPublisher,
			ITenantUnitOfWork tenantUnitOfWork,
			ICurrentTenantSession currentTenantSession,
			AnalyticsDatabase analytics,
			Database database,
			WithUnitOfWork withUnitOfWork,
			ITeamRepository teams,
			IPersonRepository persons,
			IExternalLogOnRepository externalLogOns,
			IContractRepository contracts,
			IPartTimePercentageRepository partTimePercentages,
			IContractScheduleRepository contractSchedules,
			ICurrentScenario scenario,
			IPersonAssignmentRepository assignments,
			IActivityRepository activities
			)
		{
			_now = now;
			_testConfiguration = testConfiguration;
			_unitOfWork = unitOfWork;
			_eventPublisher = eventPublisher;
			_tenantUnitOfWork = tenantUnitOfWork;
			_currentTenantSession = currentTenantSession;
			_analytics = analytics;
			_database = database;
			_withUnitOfWork = withUnitOfWork;
			_teams = teams;
			_persons = persons;
			_externalLogOns = externalLogOns;
			_contracts = contracts;
			_partTimePercentages = partTimePercentages;
			_contractSchedules = contractSchedules;
			_scenario = scenario;
			_assignments = assignments;
			_activities = activities;
		}

		[TestLogTime]
		public virtual void Create()
		{
			_now.Is("2016-02-25 08:00".Utc());

			_analytics
				.WithDataSource(_testConfiguration.DataSourceId, _testConfiguration.SourceId)
				;

			createCommonData();
			createPersonsAndSchedules();

			// to create/update any data that is periodically kept up to date
			// like the rule mappings
			_eventPublisher.Publish(new TenantMinuteTickEvent());
			_eventPublisher.Publish(new TenantHourTickEvent());

		}

		private void createPersonsAndSchedules()
		{
			IList<IPerson> rogers = new List<IPerson>();

			_withUnitOfWork.Do(() =>
			{
				var contract = _contracts.LoadAll().Single(x => x.Description.Name == "contract");
				var partTimePercentage = _partTimePercentages.LoadAll().Single(x => x.Description.Name == "partTimePercentage");
				var contractSchedule = _contractSchedules.LoadAll().Single(x => x.Description.Name == "contractSchedule");
				var teams = _teams.LoadAll();

				Enumerable.Range(0, _testConfiguration.NumberOfAgents)
					.ForEach(roger =>
					{
						var name = "roger" + roger;
						var team = teams.Single(x => x.Description.Name == "team" + (roger/10));

						var person = new Person {Name = new Name(name, name)};
						person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
						_persons.Add(person);

						var personContract = new PersonContract(contract, partTimePercentage, contractSchedule);
						var personPeriod = new PersonPeriod("2001-01-01".Date(), personContract, team);
						person.AddPersonPeriod(personPeriod);

						var logon = new ExternalLogOn
						{
							AcdLogOnName = name,// is not used?
							DataSourceId = _testConfiguration.DataSourceId,
							AcdLogOnOriginalId = _testConfiguration.SourceId, // this is what the rta receives
							AcdLogOnMartId = -1
						};
						_externalLogOns.Add(logon);
						person.AddExternalLogOn(logon, personPeriod);

						rogers.Add(person);
					});

			});

			var phone = _withUnitOfWork.Get(() => _activities.LoadAll().Single(x => x.Name == "Phone"));
			var brejk = _withUnitOfWork.Get(() => _activities.LoadAll().Single(x => x.Name == "Break"));
			var lunch = _withUnitOfWork.Get(() => _activities.LoadAll().Single(x => x.Name == "Lunch"));

			rogers.ForEach(roger =>
			{
				_withUnitOfWork.Do(() =>
				{
					var assignment1 = new PersonAssignment(roger, _scenario.Current(), "2016-02-25".Date());
					assignment1.AddActivity(phone, "2016-02-25 08:00".Utc(), "2016-02-25 17:00".Utc());
					assignment1.AddActivity(brejk, "2016-02-25 10:00".Utc(), "2016-02-25 10:15".Utc());
					assignment1.AddActivity(lunch, "2016-02-25 11:30".Utc(), "2016-02-25 12:00".Utc());
					assignment1.AddActivity(brejk, "2016-02-25 15:00".Utc(), "2016-02-25 15:15".Utc());
					_assignments.Add(assignment1);

					var assignment2 = new PersonAssignment(roger, _scenario.Current(), "2016-02-26".Date());
					assignment2.AddActivity(phone, "2016-02-26 08:00".Utc(), "2016-02-26 17:00".Utc());
					assignment2.AddActivity(brejk, "2016-02-26 10:00".Utc(), "2016-02-26 10:15".Utc());
					assignment2.AddActivity(lunch, "2016-02-26 11:30".Utc(), "2016-02-26 12:00".Utc());
					assignment2.AddActivity(brejk, "2016-02-26 15:00".Utc(), "2016-02-26 15:15".Utc());
					_assignments.Add(assignment2);

					var assignment3 = new PersonAssignment(roger, _scenario.Current(), "2016-02-27".Date());
					assignment3.AddActivity(phone, "2016-02-27 08:00".Utc(), "2016-02-27 17:00".Utc());
					assignment3.AddActivity(brejk, "2016-02-27 10:00".Utc(), "2016-02-27 10:15".Utc());
					assignment3.AddActivity(lunch, "2016-02-27 11:30".Utc(), "2016-02-27 12:00".Utc());
					assignment3.AddActivity(brejk, "2016-02-27 15:00".Utc(), "2016-02-27 15:15".Utc());
					_assignments.Add(assignment3);

				});
			});

		}

		[UnitOfWork]
		protected virtual void createCommonData()
		{
			var data = new TestDataFactory(_unitOfWork, _currentTenantSession, _tenantUnitOfWork);

			var businessUnit = DefaultBusinessUnit.BusinessUnit;

			data.Apply(new ScenarioConfigurable {Name = "Default", BusinessUnit = businessUnit.Name});
			data.Apply(new ActivityConfigurable {Name = "Phone"});
			data.Apply(new ActivityConfigurable {Name = "Lunch"});
			data.Apply(new ActivityConfigurable {Name = "Break"});

			data.Apply(new ContractConfigurable {Name = "contract"});
			data.Apply(new PartTimePercentageConfigurable {Name = "partTimePercentage"});
			data.Apply(new ContractScheduleConfigurable {Name = "contractSchedule"});

			data.Apply(new RtaMapConfigurable {Activity = "Phone", PhoneState = "Ready", Adherence = "In", Name = "InAdherence"});
			data.Apply(new RtaMapConfigurable {Activity = "Phone", PhoneState = "LoggedOff", Adherence = "Out", Name = "OutOfAdherence"});

			data.Apply(new RtaMapConfigurable {Activity = "Break", PhoneState = "LoggedOff", Adherence = "In", Name = "InAdherence"});
			data.Apply(new RtaMapConfigurable {Activity = "Break", PhoneState = "Ready", Adherence = "Out", Name = "OutOfAdherence"});

			data.Apply(new RtaMapConfigurable {Activity = "Lunch", PhoneState = "LoggedOff", Adherence = "In", Name = "InAdherence"});
			data.Apply(new RtaMapConfigurable {Activity = "Lunch", PhoneState = "Ready", Adherence = "Out", Name = "OutOfAdherence"});

			data.Apply(new RtaMapConfigurable {Activity = null, PhoneState = "LoggedOff", Adherence = "In", Name = "InAdherence"});

			Enumerable.Range(0, (_testConfiguration.NumberOfMappings/4))
				.ForEach(code =>
				{
					var phoneState = $"Misc{code}";
					data.Apply(new RtaMapConfigurable {Activity = null, PhoneState = phoneState, Adherence = "Neutral", Name = "NeutralAdherence"});
					data.Apply(new RtaMapConfigurable {Activity = "Phone", PhoneState = phoneState, Adherence = "In", Name = "InAdherence"});
					data.Apply(new RtaMapConfigurable {Activity = "Break", PhoneState = phoneState, Adherence = "Out", Name = "OutOfAdherence"});
					data.Apply(new RtaMapConfigurable {Activity = "Lunch", PhoneState = phoneState, Adherence = "Out", Name = "OutOfAdherence"});
				});

			Enumerable.Range(0, (_testConfiguration.NumberOfAgents/100) + 1)
				.ForEach(site =>
				{
					data.Apply(new SiteConfigurable
					{
						Name = "site" + site,
						BusinessUnit = businessUnit.Name
					});
				});

			Enumerable.Range(0, (_testConfiguration.NumberOfAgents/10) + 1)
				.ForEach(team =>
				{
					data.Apply(new TeamConfigurable
					{
						Name = "team" + team,
						Site = "site" + (team/10)
					});
				});

		}
	}
}