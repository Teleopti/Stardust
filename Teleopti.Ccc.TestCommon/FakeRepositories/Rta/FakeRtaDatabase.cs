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
using Teleopti.Ccc.Domain.Security.AuthorizationData;
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

	public class FakeRtaDatabase : FakeDatabase
	{
		private readonly FakeAgentStatePersister _agentStates;
		private readonly FakeAgentStateReadModelPersister _agentStateReadModels;
		private readonly FakeDataSourceForTenant _dataSourceForTenant;
		private readonly FakeScheduleProjectionReadOnlyPersister _schedules;
		private readonly AgentStateMaintainer _agentStateMaintainer;
		private readonly IScheduleStorage _scheduleStorage;

		public FakeRtaDatabase(
			FakeTenants tenants, 
			FakePersonRepository persons, 
			FakeBusinessUnitRepository businessUnits, 
			FakeSiteRepository sites, 
			FakeTeamRepository teams, 
			FakeContractRepository contracts, 
			FakePartTimePercentageRepository partTimePercentages, 
			FakeContractScheduleRepository contractSchedules, 
			FakeApplicationRoleRepository applicationRoles, 
			FakeScenarioRepository scenarios, 
			FakeDayOffTemplateRepository dayOffTemplates, 
			FakePersonAssignmentRepository personAssignments, 
			FakeApplicationFunctionRepository applicationFunctions, 
			FakeAvailableDataRepository availableDatas, 
			IDefinedRaptorApplicationFunctionFactory allApplicationFunctions, 
			FakeAbsenceRepository absences, 
			FakePersonAbsenceRepository personAbsences, 
			FakeActivityRepository activities, 
			FakeCommonAgentNameProvider commonAgentNameProvider, 
			FakeSkillRepository skills, 
			FakeGroupingReadOnlyRepository groupings, 
			FakeRtaStateGroupRepository stateGroups, 
			FakeRtaMapRepository mappings, 
			FakeExternalLogOnRepository externalLogOns, 
			FakeDataSources dataSources,
			FakeAgentStatePersister agentStates,
			FakeAgentStateReadModelPersister agentStateReadModels,
			FakeDataSourceForTenant dataSourceForTenant,
			FakeScheduleProjectionReadOnlyPersister schedules,
			AgentStateMaintainer agentStateMaintainer,
			IScheduleStorage scheduleStorage,
			FakeSiteInAlarmReader siteInAlarmReader,
			FakeTeamInAlarmReader teamInAlarmReader,
			FakeMeetingRepository meetings
			) : base(
				tenants, 
				persons, 
				businessUnits, 
				sites, 
				teams, 
				contracts, 
				partTimePercentages, 
				contractSchedules, 
				applicationRoles, 
				scenarios, 
				dayOffTemplates, 
				personAssignments, 
				applicationFunctions, 
				availableDatas, 
				allApplicationFunctions, 
				absences, 
				personAbsences, 
				activities, 
				commonAgentNameProvider, 
				skills, 
				groupings, 
				stateGroups, 
				mappings, 
				externalLogOns, 
				dataSources,
				siteInAlarmReader, 
				teamInAlarmReader,
				meetings)
		{
			_agentStates = agentStates;
			_agentStateReadModels = agentStateReadModels;
			_dataSourceForTenant = dataSourceForTenant;
			_schedules = schedules;
			_agentStateMaintainer = agentStateMaintainer;
			_scheduleStorage = scheduleStorage;

			WithDataSource(new StateForTest().SourceId);
			WithPlatform(new Guid(new StateForTest().PlatformTypeId));
		}

		public AgentState StoredState => _agentStates.Find(_agentStates.FindAll(), DeadLockVictim.Yes).SingleOrDefault();
		public AgentState StoredStateFor(Guid personId) => _agentStates.ForPersonId(personId);
		public AgentStateReadModel PersistedReadModel => _agentStateReadModels.Models.SingleOrDefault();
		public IEnumerable<IRtaState> StateCodes => _stateGroups.LoadAll().Single().StateCollection;

		public FakeRtaDatabase WithPlatform(Guid platformTypeId)
		{
			return base.WithPlatform(platformTypeId) as FakeRtaDatabase;
		}

		public new FakeRtaDatabase WithTenant(string key)
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

		public FakeRtaDatabase WithDataSource(string sourceId)
		{
			return WithDataSource(new Random().Next(100, 1000), sourceId) as FakeRtaDatabase;
		}

		public FakeRtaDatabase WithBusinessUnit(Guid businessUnitId)
		{
			if (_businessUnits.LoadAll().Any(x => x.Id.Equals(businessUnitId)))
				return this;
			return base.WithBusinessUnit(businessUnitId) as FakeRtaDatabase;
		}

		public FakeRtaDatabase WithAgent(string userCode, Guid personId, Guid? businessUnitId, Guid? teamId, Guid? siteId)
		{
			this.WithAgent(personId, userCode, teamId, siteId, businessUnitId);
			
			_agentStates.Prepare(new AgentStatePrepare
			{
				PersonId = personId,
				BusinessUnitId = CurrentBusinessUnitId(),
				TeamId = CurrentTeamId(),
				SiteId = CurrentSiteId(),
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = CurrentDataSourceId(),
						UserCode = userCode
					}
				}
			}, DeadLockVictim.Yes);
			return this;
		}

		public FakeRtaDatabase WithSchedule(Guid personId, Guid activityId, string start, string end, string belongsToDate, string name, Color? color)
		{
			this.WithPerson(personId, null);
			WithAssignment(personId, belongsToDate ?? start);
			WithActivity(activityId, name, color);
			WithAssignedActivity(start, end);

			_schedules.Clear();
			MakeScheduledActivities()
				.Select(l => JsonConvert.DeserializeObject<ScheduleProjectionReadOnlyModel>(JsonConvert.SerializeObject(l)))
				.ForEach(x => _schedules.AddActivity(x));

			_agentStateMaintainer.Handle(new ScheduleChangedEvent
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
				_scenarios.Load(CurrentScenarioId()),
				_scheduleStorage,
				_persons.LoadAll(),
				period);
		}

		public FakeRtaDatabase ClearSchedule(Guid personId)
		{
			_schedules.Clear(personId);
			ClearScheduleData(personId);
			_agentStateMaintainer.Handle(new ScheduleChangedEvent
			{
				PersonId = personId
			});
			return this;
		}

		public FakeRtaDatabase WithRule(Guid? ruleId, string stateCode, Guid? platformTypeId, Guid? activityId, int staffingEffect, string name, Adherence? adherence)
		{
			base.WithRule(ruleId, stateCode, platformTypeId, activityId, staffingEffect, name, adherence, null);
			return this;
		}

		public new FakeRtaDatabase WithMapWithStateGroupWithoutStateCodes()
		{
			return base.WithMapWithStateGroupWithoutStateCodes() as FakeRtaDatabase;
		}

		public FakeRtaDatabase ClearRuleMap()
		{
			_mappings.Clear();
			return this;
		}

		public FakeRtaDatabase ClearStates()
		{
			_stateGroups.Clear();
			return this;
		}

		public FakeRtaDatabase WithAlarm(TimeSpan threshold)
		{
			return WithAlarm(threshold, null) as FakeRtaDatabase;
		}

		public FakeRtaDatabase WithScenario(Guid? id, bool @default)
		{
			return base.WithScenario(id, @default) as FakeRtaDatabase;
		}

	}

	public static class FakeDatabaseUserExtensions
	{
		public static FakeRtaDatabase WithAgent(this FakeRtaDatabase fakeDataBuilder, string userCode)
		{
			return fakeDataBuilder.WithAgent(userCode, Guid.NewGuid(), null, null, null);
		}

		public static FakeRtaDatabase WithAgent(this FakeRtaDatabase fakeDataBuilder, string userCode, Guid personId)
		{
			return fakeDataBuilder.WithAgent(userCode, personId, null, null, null);
		}

		public static FakeRtaDatabase WithAgent(this FakeRtaDatabase fakeDataBuilder, string userCode, string source, Guid personId)
		{
			return fakeDataBuilder.WithAgent(userCode, personId, null, null, null);
		}

		public static FakeRtaDatabase WithAgent(this FakeRtaDatabase fakeDataBuilder, string userCode, Guid personId, Guid? businessUnitId, Guid? teamId, Guid? siteId)
		{
			return fakeDataBuilder.WithAgent(userCode, personId, businessUnitId, teamId, siteId);
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