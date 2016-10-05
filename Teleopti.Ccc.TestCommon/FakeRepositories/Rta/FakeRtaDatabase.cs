using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Interfaces.Domain;
using ExternalLogon = Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.ExternalLogon;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class CloseSnapshotForTest : CloseSnapshotInputModel
	{
		public CloseSnapshotForTest()
		{
			AuthenticationKey = LegacyAuthenticationKey.TheKey;
			SourceId = "sourceId";
		}
	}
	
	public class BatchForTest : BatchInputModel
	{
		public BatchForTest()
		{
			AuthenticationKey = LegacyAuthenticationKey.TheKey;
			PlatformTypeId = Guid.Empty.ToString();
			SourceId = "sourceId";
		}
	}

	public class BatchStateForTest : BatchStateInputModel
	{
		public BatchStateForTest()
		{
			UserCode = "8808";
			StateCode = "AUX2";
		}
	}

	public class StateForTest : StateInputModel
	{
		public StateForTest()
		{
			AuthenticationKey = LegacyAuthenticationKey.TheKey;
			PlatformTypeId = Guid.Empty.ToString();
			SourceId = "sourceId";
			UserCode = "8808";
			StateCode = "AUX2";
		}
	}

	public class FakeRtaDatabase
	{
		private readonly FakeDatabase _database;
		private readonly FakeAgentStateReadModelPersister _agentStateReadModels;
		private readonly FakeAgentStatePersister _agentStates;
		private readonly FakeRtaStateGroupRepository _rtaStateGroupRepository;
		private readonly FakeRtaMapRepository _rtaMapRepository;
		private readonly FakeTenants _tenants;
		private readonly FakeDataSourceForTenant _dataSourceForTenant;
		private readonly FakeBusinessUnitRepository _businessUnits;
		private readonly FakeScheduleProjectionReadOnlyPersister _schedules;
		private readonly AgentStateMaintainer _agentStateMaintainer;
		private readonly FakePersonRepository _persons;
		private readonly FakePersonAssignmentRepository _personAssignments;
		private readonly FakeScenarioRepository _scenarios;
		private readonly IScheduleStorage _scheduleStorage;
		
		public FakeRtaDatabase(
			FakeDatabase database,
			FakeAgentStateReadModelPersister agentStateReadModels,
			FakeAgentStatePersister agentStates,
			FakeRtaStateGroupRepository rtaStateGroupRepository,
			FakeRtaMapRepository rtaMapRepository,
			FakeTenants tenants,
			FakeDataSourceForTenant dataSourceForTenant,
			FakeBusinessUnitRepository businessUnits,
			FakeScheduleProjectionReadOnlyPersister schedules,
			AgentStateMaintainer agentStateMaintainer,
			FakePersonRepository persons,
			FakePersonAssignmentRepository personAssignments,
			FakeScenarioRepository scenarios,
			IScheduleStorage scheduleStorage
			)
		{
			_database = database;
			_agentStateReadModels = agentStateReadModels;
			_agentStates = agentStates;
			_rtaStateGroupRepository = rtaStateGroupRepository;
			_rtaMapRepository = rtaMapRepository;
			_tenants = tenants;
			_dataSourceForTenant = dataSourceForTenant;
			_businessUnits = businessUnits;
			_schedules = schedules;
			_agentStateMaintainer = agentStateMaintainer;
			_persons = persons;
			_personAssignments = personAssignments;
			_scenarios = scenarios;
			_scheduleStorage = scheduleStorage;

			withTenant("default", LegacyAuthenticationKey.TheKey);
			WithSource(new StateForTest().SourceId);
			WithPlatform(new Guid(new StateForTest().PlatformTypeId));
		}

		public AgentState StoredState => _agentStates.GetStates().SingleOrDefault();
		public AgentState StoredStateFor(Guid personId) => _agentStates.Get(personId);
		public AgentStateReadModel PersistedReadModel => _agentStateReadModels.Models.SingleOrDefault();
		public IEnumerable<IRtaState> StateCodes => _rtaStateGroupRepository.LoadAll().Single().StateCollection;
		
		public FakeRtaDatabase WithPlatform(Guid platformTypeId)
		{
			_database.WithPlatform(platformTypeId);
			return this;
		}
		
		public FakeRtaDatabase WithTenant(string key)
		{
			return withTenant(key, key);
		}

		private FakeRtaDatabase withTenant(string name, string key)
		{
			// only required without multi-tenancy I think...
			// because then the rta requires all tenants to be loaded at startup to find the datasource from a connection string
			var dataSource = new FakeDataSource(name)
			{
				Application = new FakeUnitOfWorkFactory {Name = name}
			};
			_dataSourceForTenant.Has(dataSource);

			_tenants.Has(new Infrastructure.MultiTenancy.Server.Tenant(name) {RtaKey = key});
			return this;
		}

		public string TenantName()
		{
			return _tenants.Tenants().Single().Name;
		}

		public FakeRtaDatabase WithSource(string sourceId)
		{
			_database.WithDataSource(new Random().Next(0, 1000), sourceId);
			return this;
		}

		public FakeRtaDatabase WithBusinessUnit(Guid businessUnitId)
		{
			if (_businessUnits.LoadAll().Any(x => x.Id.Equals(businessUnitId)))
				return this;
			_database.WithBusinessUnit(businessUnitId);
			return this;
		}

		public FakeRtaDatabase WithUser(string userCode, Guid personId, Guid? businessUnitId, Guid? teamId, Guid? siteId)
		{
			_database.WithAgent(personId, userCode, teamId, siteId, businessUnitId);
			
			_agentStates.Prepare(new AgentStatePrepare
			{
				PersonId = personId,
				BusinessUnitId = _database.CurrentBusinessUnitId(),
				TeamId = _database.CurrentTeamId(),
				SiteId = _database.CurrentSiteId(),
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = _database.CurrentDataSourceId(),
						UserCode = userCode
					}
				}
			}, DeadLockVictim.Yes);
			return this;
		}

		public FakeRtaDatabase WithSchedule(Guid personId, Guid activityId, string start, string end, string belongsToDate, string name, Color? color)
		{
			_database.WithPerson(personId, null);
			_database.WithAssignment(personId, belongsToDate ?? start);
			_database.WithActivity(activityId, name, color);
			_database.WithAssignedActivity(start, end);

			_schedules.Clear();
			MakeScheduledActivities()
				.Select(l => JsonConvert.DeserializeObject<ScheduleProjectionReadOnlyModel>(JsonConvert.SerializeObject(l)))
				.ForEach(x => _schedules.AddActivity(x));

			_agentStateMaintainer.Handle(new ScheduleProjectionReadOnlyChangedEvent
			{
				PersonId = personId
			});
			return this;
		}

		[FullPermissions]
		protected virtual IEnumerable<ScheduledActivity> MakeScheduledActivities()
		{
			var min = _personAssignments.LoadAll().SelectMany(x => x.ShiftLayers).Min(x => x.Period.StartDateTime);
			var max = _personAssignments.LoadAll().SelectMany(x => x.ShiftLayers).Max(x => x.Period.EndDateTime);
			var period = new DateOnlyPeriod(new DateOnly(min.AddDays(-2)), new DateOnly(max.AddDays(2)));
			return FromPersonAssignment.MakeScheduledActivities(
				_scenarios.Load(_database.CurrentScenarioId()),
				_scheduleStorage,
				_persons.LoadAll(),
				period);
		}

		public FakeRtaDatabase ClearSchedule(Guid personId)
		{
			_schedules.Clear(personId);
			_database.ClearScheduleData(personId);
			_agentStateMaintainer.Handle(new ScheduleProjectionReadOnlyChangedEvent
			{
				PersonId = personId
			});
			return this;
		}

		public FakeRtaDatabase WithRule(Guid? ruleId, string stateCode, Guid? platformTypeId, Guid? activityId, int staffingEffect, string name, Adherence? adherence)
		{
			_database.WithRule(ruleId, stateCode, platformTypeId, activityId, staffingEffect, name, adherence, null);
			return this;
		}

		public FakeRtaDatabase WithMapWithStateGroupWithoutStateCodes()
		{
			_database.WithMapWithStateGroupWithoutStateCodes();
			return this;
		}

		public FakeRtaDatabase ClearRuleMap()
		{
			_rtaMapRepository.Clear();
			return this;
		}

		public FakeRtaDatabase ClearStates()
		{
			_rtaStateGroupRepository.Clear();
			return this;
		}

		public FakeRtaDatabase WithAlarm(TimeSpan threshold)
		{
			_database.WithAlarm(threshold, null);
			return this;
		}
		
	}

	public static class FakeDatabaseUserExtensions
	{
		public static FakeRtaDatabase WithUser(this FakeRtaDatabase fakeDataBuilder, string userCode)
		{
			return fakeDataBuilder.WithUser(userCode, Guid.NewGuid(), null, null, null);
		}

		public static FakeRtaDatabase WithUser(this FakeRtaDatabase fakeDataBuilder, string userCode, Guid personId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, null, null, null);
		}

		public static FakeRtaDatabase WithUser(this FakeRtaDatabase fakeDataBuilder, string userCode, string source, Guid personId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, null, null, null);
		}

		public static FakeRtaDatabase WithUser(this FakeRtaDatabase fakeDataBuilder, string userCode, Guid personId, Guid? businessUnitId, Guid? teamId, Guid? siteId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, businessUnitId, teamId, siteId);
		}
	}

	public static class FakeDatabaseScheduleExtensions
	{
		public static FakeRtaDatabase WithSchedule(this FakeRtaDatabase fakeDataBuilder, Guid personId, Guid activityId, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, start, end, null, null, null);
		}

		public static FakeRtaDatabase WithSchedule(this FakeRtaDatabase fakeDataBuilder, Guid personId, Guid activityId, string name, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, start, end, null, name, null);
		}
		
		public static FakeRtaDatabase WithSchedule(this FakeRtaDatabase fakeDataBuilder, Guid personId, Guid activityId, string name, string belongsToDate, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, start, end, belongsToDate, null, Color.Black);
		}

		public static FakeRtaDatabase WithSchedule(this FakeRtaDatabase fakeDataBuilder, Guid personId, Color color, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, Guid.NewGuid(), start, end, null, null, color);
		}

		public static FakeRtaDatabase WithSchedule(this FakeRtaDatabase fakeDataBuilder, Guid personId, string name, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, Guid.NewGuid(), start, end, null, name, null);
		}

	}

	public static class FakeDatabaseRuleExtensions
	{
		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder)
		{
			return fakeDataBuilder.WithRule(null, "", null, null, 0, null, null);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode)
		{
			return fakeDataBuilder.WithRule(null, stateCode, null, null, 0, null, null);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode, Guid? activityId)
		{
			return fakeDataBuilder.WithRule(Guid.NewGuid(), stateCode, null, activityId, 0, null, null);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode, Guid? activityId, int staffingEffect)
		{
			return fakeDataBuilder.WithRule(Guid.NewGuid(), stateCode, null, activityId, staffingEffect, null, null);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode, Guid? activityId, Guid? ruleId)
		{
			return fakeDataBuilder.WithRule(ruleId, stateCode, null, activityId, 0, null, null);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode, Guid? activityId, Guid? ruleId, string name)
		{
			return fakeDataBuilder.WithRule(ruleId, stateCode, null, activityId, 0, name, null);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode, Guid? activityId, string name)
		{
			return fakeDataBuilder.WithRule(Guid.NewGuid(), stateCode, null, activityId, 0, name, null);
		}
		
		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode, Guid platformTypeId, Guid activityId, int staffingEffect, Adherence adherence)
		{
			return fakeDataBuilder.WithRule(Guid.NewGuid(), stateCode, platformTypeId, activityId, staffingEffect, null, adherence);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode, Guid? activityId, int staffingEffect, Adherence adherence)
		{
			return fakeDataBuilder.WithRule(Guid.NewGuid(), stateCode, null, activityId, staffingEffect, null, adherence);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, Guid ruleId, string stateCode, Guid? activityId)
		{
			return fakeDataBuilder.WithRule(ruleId, stateCode, null, activityId, 0, null, null);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, Guid ruleId, string stateCode, Guid? activityId, string name)
		{
			return fakeDataBuilder.WithRule(ruleId, stateCode, null, activityId, 0, name, null);
		}
	}
}