using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
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

			WithDataSource(new StateForTest().SourceId);
			WithPlatform(new Guid(new StateForTest().PlatformTypeId));
		}

		public AgentState StoredState => _agentStates.Find(_agentStates.FindForCheck(), DeadLockVictim.Yes).SingleOrDefault();
		public AgentState StoredStateFor(Guid personId) => _agentStates.ForPersonId(personId);
		public AgentStateReadModel PersistedReadModel => _agentStateReadModels.Models.SingleOrDefault();
		public IEnumerable<IRtaState> StateCodes => _stateGroups.LoadAll().Single().StateCollection;
		
		public string TenantName()
		{
			return _tenants.Tenants().Single().Name;
		}

		public FakeRtaDatabase WithBusinessUnit(Guid businessUnitId)
		{
			if (_businessUnits.LoadAll().Any(x => x.Id.Equals(businessUnitId)))
				return this;
			return base.WithBusinessUnit(businessUnitId) as FakeRtaDatabase;
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

		public FakeRtaDatabase WithScenario(Guid? id, bool @default)
		{
			return base.WithScenario(id, @default) as FakeRtaDatabase;
		}
	}
	
	public static class FakeDatabaseScheduleExtensions
	{
		public static FakeRtaDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, Guid activityId, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, start, end, null, null, null);
		}

		public static FakeRtaDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, Guid activityId, string name, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, start, end, null, name, null);
		}
		
		public static FakeRtaDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, Guid activityId, string name, string belongsToDate, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, start, end, belongsToDate, null, Color.Black);
		}

		public static FakeRtaDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, Color color, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, Guid.NewGuid(), start, end, null, null, color);
		}

		public static FakeRtaDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, string name, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, Guid.NewGuid(), start, end, null, name, null);
		}

		public static FakeRtaDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, Guid activityId, string start, string end, string belongsToDate, string name, Color? color)
		{
			fakeDataBuilder.WithPerson(personId, null);
			fakeDataBuilder.WithAssignment(personId, belongsToDate ?? start);
			fakeDataBuilder.WithActivity(activityId, name, color);
			fakeDataBuilder.WithAssignedActivity(start, end);
			return fakeDataBuilder as FakeRtaDatabase;
		}

	}

}