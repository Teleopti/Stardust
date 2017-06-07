using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public static class RtaExtensions
	{
		public static void CheckForActivityChanges(this Domain.ApplicationLayer.Rta.Service.Rta rta, string tenant, Guid personId)
		{
			rta.CheckForActivityChanges(tenant);
		}

		public static void ProcessState(this Domain.ApplicationLayer.Rta.Service.Rta rta, StateForTest input)
		{
			rta.Process(new BatchInputModel
			{
				AuthenticationKey = input.AuthenticationKey,
				SourceId = input.SourceId,
				SnapshotId = input.SnapshotId,
				States = new[]
				{
					new BatchStateInputModel
					{
						StateCode = input.StateCode,
						StateDescription = input.StateDescription,
						UserCode = input.UserCode
					}
				}
			});
		}

		public static void CloseSnapshot(this Domain.ApplicationLayer.Rta.Service.Rta rta, CloseSnapshotForTest input)
		{
			rta.Process(new BatchInputModel
			{
				AuthenticationKey = input.AuthenticationKey,
				SourceId = input.SourceId,
				SnapshotId = input.SnapshotId,
				CloseSnapshot = true
			});
		}

	}

	public class StateForTest
	{
		public string AuthenticationKey { get; set; }
		public string SourceId { get; set; }
		public string UserCode { get; set; }
		public string StateCode { get; set; }
		public string StateDescription { get; set; }
		public DateTime? SnapshotId { get; set; }

		public StateForTest()
		{
			AuthenticationKey = LegacyAuthenticationKey.TheKey;
			SourceId = "sourceId";
			UserCode = "8808";
			StateCode = "AUX2";
		}
	}

	public class CloseSnapshotForTest
	{
		public string AuthenticationKey { get; set; }
		public string SourceId { get; set; }
		public DateTime SnapshotId { get; set; }

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
			FakeRtaRuleRepository rules,
			FakeExternalLogOnRepository externalLogOns, 
			FakeDataSources dataSources,
			FakeAgentStatePersister agentStates,
			FakeAgentStateReadModelPersister agentStateReadModels,
			//FakeSiteInAlarmReader siteInAlarmReader,
			FakeTeamsInAlarmReader teamsInAlarmReader,
			FakeMeetingRepository meetings,
			HardcodedSkillGroupingPageId hardcodedSkillGroupingPageId
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
				rules,
				externalLogOns, 
				dataSources,
				//siteInAlarmReader, 
				teamsInAlarmReader,
				meetings,
				agentStateReadModels,
				hardcodedSkillGroupingPageId)
		{
			_agentStates = agentStates;
			_agentStateReadModels = agentStateReadModels;

			WithDataSource(new StateForTest().SourceId);
		}

		public AgentState StoredState => _agentStates.LockNLoad(_agentStates.FindForCheck().Select(x => x.PersonId), DeadLockVictim.Yes).AgentStates.SingleOrDefault();
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
			_mappings.LoadAll().ForEach(x => _mappings.Remove(x));
			return this;
		}

		public FakeRtaDatabase ClearStates()
		{
			_stateGroups.LoadAll().ForEach(x => _stateGroups.Remove(x));
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

		public static FakeRtaDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, Guid activityId, Color color, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, start, end, null, null, color);
		}

		public static FakeRtaDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, Color color, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, Guid.NewGuid(), start, end, null, null, color);
		}

		public static FakeRtaDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, string name, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, Guid.NewGuid(), start, end, null, name, null);
		}
		
		public static FakeDatabase WithSchedule(this FakeDatabase database, Guid personId, string start, string end)
		{
			return database.WithSchedule(personId, Guid.NewGuid(), start, end, null, null, null);
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