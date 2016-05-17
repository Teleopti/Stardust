using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class ExternalUserStateForTest : ExternalUserStateInputModel
	{
		public ExternalUserStateForTest()
		{
			AuthenticationKey = LegacyAuthenticationKey.TheKey;
			PlatformTypeId = Guid.Empty.ToString();
			SourceId = "sourceId";
			UserCode = "8808";
			StateCode = "AUX2";
			IsLoggedOn = true;
		}
	}

	public class FakeRtaDatabase : IDatabaseReader
	{
		private readonly INow _now;
		public readonly FakeAgentStateReadModelStorage AgentStateReadModels;
		public readonly FakeRtaStateGroupRepository RtaStateGroupRepository;
		public readonly FakeRtaMapRepository RtaMapRepository;
		public readonly FakeTenants Tenants;
		public readonly FakeTeamOutOfAdherenceReadModelPersister TeamOutOfAdherenceReadModelPersister;
		public readonly FakeSiteOutOfAdherenceReadModelPersister SiteOutOfAdherenceReadModelPersister;
		public readonly FakeAdherenceDetailsReadModelPersister AdherenceDetailsReadModelPersister;
		public readonly FakeAdherencePercentageReadModelPersister AdherencePercentageReadModelPersister;
		private readonly FakeDataSourceForTenant _dataSourceForTenant;
		private static readonly Random random = new Random();

		private BusinessUnit _businessUnit;
		private Guid _businessUnitId;
		private string _platformTypeId;
		private RtaRule _rtaRule;

		private readonly List<KeyValuePair<string, int>> _datasources = new List<KeyValuePair<string, int>>();
		private readonly List<scheduleLayer2> _schedules = new List<scheduleLayer2>();

		private readonly List<PersonOrganizationData> _personOrganizationData = new List<PersonOrganizationData>();
		private class userData
		{
			public int DataSourceId;
			public string ExternalLogOn;
			public PersonOrganizationData Data;
		}
		private readonly List<userData> _userInfos = new List<userData>();

		private class scheduleLayer2
		{
			public Guid PersonId { get; set; }
			public ScheduleLayer ScheduleLayer { get; set; }
		}

		public FakeRtaDatabase(
			INow now,
			FakeAgentStateReadModelStorage agentStateReadModels,
			FakeRtaStateGroupRepository rtaStateGroupRepository,
			FakeRtaMapRepository rtaMapRepository,
			FakeTenants tenants,
			FakeTeamOutOfAdherenceReadModelPersister teamOutOfAdherenceReadModelPersister,
			FakeSiteOutOfAdherenceReadModelPersister siteOutOfAdherenceReadModelPersister,
			FakeAdherenceDetailsReadModelPersister adherenceDetailsReadModelPersister,
			FakeAdherencePercentageReadModelPersister adherencePercentageReadModelPersister,
			FakeDataSourceForTenant dataSourceForTenant
			)
		{
			_now = now;
			AgentStateReadModels = agentStateReadModels;
			RtaStateGroupRepository = rtaStateGroupRepository;
			RtaMapRepository = rtaMapRepository;
			Tenants = tenants;
			TeamOutOfAdherenceReadModelPersister = teamOutOfAdherenceReadModelPersister;
			SiteOutOfAdherenceReadModelPersister = siteOutOfAdherenceReadModelPersister;
			AdherenceDetailsReadModelPersister = adherenceDetailsReadModelPersister;
			AdherencePercentageReadModelPersister = adherencePercentageReadModelPersister;
			_dataSourceForTenant = dataSourceForTenant;
			WithBusinessUnit(Guid.NewGuid());
			WithDefaultsFromState(new ExternalUserStateForTest());
			WithTenant("default", LegacyAuthenticationKey.TheKey);
		}

		public StoredStateInfo StoredState
		{
			get
			{
				return AgentStateReadModels
					.Models
					.Select(x => new StoredStateInfo(x.PersonId, x))
					.SingleOrDefault();
			}
		}

		public AgentStateReadModel PersistedReadModel { get { return AgentStateReadModels.Models.SingleOrDefault(); } }

		public StoredStateInfo StoredStateFor(Guid personId)
		{
			return new StoredStateInfo(personId, AgentStateReadModels.Get(personId));
		}

		public IEnumerable<IRtaState> StateCodes
		{
			get { return RtaStateGroupRepository.LoadAll().Single().StateCollection; }
		}

		public FakeRtaDatabase WithDefaultsFromState(ExternalUserStateForTest state)
		{
			WithSource(state.SourceId);
			withPlatform(state.PlatformTypeId);
			return this;
		}

		private Guid withPlatform(string platformTypeId)
		{
			return withPlatform(new Guid(platformTypeId));
		}

		private Guid withPlatform(Guid? platformTypeId)
		{
			if (platformTypeId.HasValue)
			{
				_platformTypeId = platformTypeId.Value.ToString();
				return platformTypeId.Value;
			}
			return new Guid(_platformTypeId);
		}

		public FakeRtaDatabase WithDataFromState(ExternalUserStateForTest state)
		{
			WithDefaultsFromState(state);
			return this.WithUser(state.UserCode, Guid.NewGuid());
		}

		public FakeRtaDatabase WithTenant(string name, string key)
		{
			// only required without multi-tenancy I think...
			// because then the rta requires all tenants to be loaded at startup to find the datasource from a connection string

			var dataSource = new FakeDataSource(name)
			{
				Application = new FakeUnitOfWorkFactory
				{
					Name = name
				}
			};
			_dataSourceForTenant.Has(dataSource);

			Tenants.Has(new Infrastructure.MultiTenancy.Server.Tenant(name) {RtaKey = key});
			return this;
		}

		public string TenantName()
		{
			return Tenants.Tenants().Single().Name;
		}

		public FakeRtaDatabase WithSource(string sourceId)
		{
			if (_datasources.Any(x => x.Key == sourceId))
				return this;
			_datasources.Add(new KeyValuePair<string, int>(sourceId, random.Next(0, 1000)));
			return this;
		}

		public FakeRtaDatabase WithBusinessUnit(Guid businessUnitId)
		{
			_businessUnitId = businessUnitId;
			_businessUnit = new BusinessUnit(".");
			_businessUnit.SetId(_businessUnitId);
			return this;
		}

		public FakeRtaDatabase WithUser(string userCode, Guid personId, string source, Guid? businessUnitId, Guid? teamId, Guid? siteId)
		{
			if (businessUnitId.HasValue)
				WithBusinessUnit(businessUnitId.Value);
			
			if (!teamId.HasValue) teamId = Guid.NewGuid();
			if (!siteId.HasValue) siteId = Guid.NewGuid();

			var dataSource = _datasources.Last().Value;
			if (_datasources.Any(x => x.Key == source))
				dataSource = _datasources.Single(x => x.Key == source).Value;

			_personOrganizationData.Add(new PersonOrganizationData
			{
				PersonId = personId,
				TeamId = teamId.Value,
				SiteId = siteId.Value,
			});

			_userInfos.Add(new userData
			{
				ExternalLogOn = userCode,
				DataSourceId = dataSource,
				Data = new PersonOrganizationData
				{
					PersonId = personId,
					BusinessUnitId = _businessUnitId,
					TeamId = teamId.Value,
					SiteId = siteId.Value,
				}
			});
			return this;
		}

		public FakeRtaDatabase WithSchedule(Guid personId, Guid activityId, string name, DateOnly belongsToDate, string start, string end)
		{
			_schedules.Add(new scheduleLayer2
			{
				PersonId = personId,
				ScheduleLayer = new ScheduleLayer
				{
					PayloadId = activityId,
					Name = name,
					StartDateTime = start.Utc(),
					EndDateTime = end.Utc(),
					BelongsToDate = belongsToDate
				}
			});
			return this;
		}
		
		public FakeRtaDatabase ClearSchedule(Guid personId)
		{
			_schedules.RemoveAll(x => x.PersonId == personId);
			return this;
		}

		public FakeRtaDatabase WithRule(Guid? ruleId, string stateCode, Guid? platformTypeId, Guid? activityId, int staffingEffect, string name, bool isLoggedOutState, Adherence? adherence)
		{
			_rtaRule = null;
			if (ruleId != null)
			{
				_rtaRule = new RtaRule();
				_rtaRule.SetId(ruleId);
				_rtaRule.SetBusinessUnit(_businessUnit);
				_rtaRule.StaffingEffect = staffingEffect;
				_rtaRule.Adherence = adherence;
			}

			IRtaStateGroup stateGroup = null;
			if (stateCode != null)
			{
				stateGroup = (
					from g in RtaStateGroupRepository.LoadAll()
					from s in g.StateCollection
					where s.StateCode == stateCode &&
						  s.PlatformTypeId == withPlatform(platformTypeId)
					select g
					).FirstOrDefault();
				if (stateGroup == null)
				{
					var isDefaultStateGroup = RtaStateGroupRepository.LoadAll().IsEmpty();
					stateGroup = new RtaStateGroup(name, isDefaultStateGroup, true);
					stateGroup.SetId(Guid.NewGuid());
					stateGroup.SetBusinessUnit(_businessUnit);
					stateGroup.IsLogOutState = isLoggedOutState;
					stateGroup.AddState(null, stateCode, withPlatform(platformTypeId));
					RtaStateGroupRepository.Add(stateGroup);
				}
			}

			IActivity activity = null;
			if (activityId != null)
			{
				activity = new Activity(stateCode ?? "activity");
				activity.SetId(activityId);
				activity.SetBusinessUnit(_businessUnit);
			}

			var mapping = new RtaMap(stateGroup, activity);
			mapping.RtaRule = _rtaRule;
			mapping.SetId(Guid.NewGuid());
			mapping.SetBusinessUnit(_businessUnit);
			RtaMapRepository.Add(mapping);

			return this;
		}

		public FakeRtaDatabase WithMapWithStateGroupWithoutStateCodes()
		{
			var stateGroup = new RtaStateGroup("Empty", false, true);
			stateGroup.SetId(Guid.NewGuid());
			stateGroup.SetBusinessUnit(_businessUnit);
			stateGroup.IsLogOutState = false;
			RtaStateGroupRepository.Add(stateGroup);
	
			var mapping = new RtaMap(stateGroup, null);
			mapping.RtaRule = _rtaRule;
			mapping.SetId(Guid.NewGuid());
			mapping.SetBusinessUnit(_businessUnit);
			RtaMapRepository.Add(mapping);

			return this;
		}

		public FakeRtaDatabase ClearRuleMap()
		{
			RtaMapRepository.Clear();
			return this;
		}

		public FakeRtaDatabase ClearStates()
		{
			RtaStateGroupRepository.Clear();
			return this;
		}

		public FakeRtaDatabase WithAlarm(TimeSpan threshold)
		{
			_rtaRule.IsAlarm = true;
			_rtaRule.ThresholdTime = threshold;
			return this;
		}

		// Implementation details
		public FakeRtaDatabase WithStateGroup(string statecode, string stateGroupName)
		{
			var stateGroup = new RtaStateGroup(stateGroupName, false, false);
			stateGroup.SetId(Guid.NewGuid());
			stateGroup.SetBusinessUnit(_businessUnit);
			if (statecode != null)
				stateGroup.AddState(statecode, statecode, Guid.Empty);
			RtaStateGroupRepository.Add(stateGroup);
			return this;
		}
		// End

		public FakeRtaDatabase WithExistingAgentState(Guid personId, string stateCode)
		{
			AgentStateReadModels.Has(new AgentStateReadModel
			{
				PersonId = personId,
				BusinessUnitId = _businessUnitId,
				PlatformTypeId = new Guid(_platformTypeId),
				StateCode = stateCode,
				StaffingEffect = 0,
				OriginalDataSourceId = new ExternalUserStateForTest().SourceId
			});
			return this;
		}

		public IList<ScheduleLayer> GetCurrentSchedule(Guid personId)
		{
			var layers = from l in _schedules
				where
					l.PersonId == personId &&
					l.ScheduleLayer.BelongsToDate.Date >= _now.UtcDateTime().Date.AddDays(-1) &&
					l.ScheduleLayer.BelongsToDate.Date <= _now.UtcDateTime().Date.AddDays(1)
				select l.ScheduleLayer;
			return new List<ScheduleLayer>(layers);
		}

		public TimeZoneInfo GetTimeZone(Guid personId)
		{
			return new UtcTimeZone().TimeZone();
		}

		public ConcurrentDictionary<string, int> Datasources()
		{
			return new ConcurrentDictionary<string, int>(_datasources);
		}

		public IEnumerable<PersonOrganizationData> LoadPersonOrganizationData(int dataSourceId, string externalLogOn)
		{
			return _userInfos
				.Where(x =>
					x.ExternalLogOn == externalLogOn &&
					x.DataSourceId == dataSourceId)
				.Select(m => JsonConvert.DeserializeObject<PersonOrganizationData>(JsonConvert.SerializeObject(m.Data)))
				.ToArray();
		}

		public IEnumerable<PersonOrganizationData> LoadAllPersonOrganizationData()
		{
			return _userInfos
				.Select(m => JsonConvert.DeserializeObject<PersonOrganizationData>(JsonConvert.SerializeObject(m.Data)))
				.ToArray();
		}

	}

	public static class FakeDatabaseBuilderExtensions
	{
		public static FakeRtaDatabase WithTenant(this FakeRtaDatabase fakeDataBuilder, string key)
		{
			return fakeDataBuilder.WithTenant(key, key);
		}

		public static FakeRtaDatabase WithUser(this FakeRtaDatabase fakeDataBuilder, string userCode)
		{
			return fakeDataBuilder.WithUser(userCode, Guid.NewGuid(), null, null, null, null);
		}

		public static FakeRtaDatabase WithUser(this FakeRtaDatabase fakeDataBuilder, string userCode, Guid personId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, null, null, null, null);
		}

		public static FakeRtaDatabase WithUser(this FakeRtaDatabase fakeDataBuilder, string userCode, Guid personId, Guid businessUnitId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, null, businessUnitId, null, null);
		}
		
		public static FakeRtaDatabase WithUser(this FakeRtaDatabase fakeDataBuilder, string userCode, string source, Guid personId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, source, null, null, null);
		}

		public static FakeRtaDatabase WithUser(this FakeRtaDatabase fakeDataBuilder, string userCode, Guid personId, Guid? businessUnitId, Guid? teamId, Guid? siteId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, null, businessUnitId, teamId, siteId);
		}

		public static FakeRtaDatabase WithSchedule(this FakeRtaDatabase fakeDataBuilder, Guid personId, Guid activityId, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, null, new DateOnly(start.Utc()), start, end);
		}

		public static FakeRtaDatabase WithSchedule(this FakeRtaDatabase fakeDataBuilder, Guid personId, Guid activityId, string name, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, name, new DateOnly(start.Utc()), start, end);
		}

		public static FakeRtaDatabase WithSchedule(this FakeRtaDatabase fakeDataBuilder, Guid personId, Guid activityId, DateOnly belongsToDate, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, null, belongsToDate, start, end);
		}



		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder)
		{
			return fakeDataBuilder.WithRule(null, "", null, null, 0, null, false, null);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode)
		{
			return fakeDataBuilder.WithRule(null, stateCode, null, null, 0, null, false, null);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode, Guid? activityId)
		{
			return fakeDataBuilder.WithRule(Guid.NewGuid(), stateCode, null, activityId, 0, null, false, null);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode, Guid? activityId, int staffingEffect)
		{
			return fakeDataBuilder.WithRule(Guid.NewGuid(), stateCode, null, activityId, staffingEffect, null, false, null);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode, Guid? activityId, Guid? alarmId)
		{
			return fakeDataBuilder.WithRule(alarmId, stateCode, null, activityId, 0, null, false, null);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode, Guid? activityId, string name)
		{
			return fakeDataBuilder.WithRule(Guid.NewGuid(), stateCode, null, activityId, 0, name, false, null);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode, Guid? activityId, bool isLoggedOutState)
		{
			return fakeDataBuilder.WithRule(Guid.NewGuid(), stateCode, null, activityId, 0, null, isLoggedOutState, null);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode, Guid platformTypeId, Guid activityId, int staffingEffect, Adherence adherence)
		{
			return fakeDataBuilder.WithRule(Guid.NewGuid(), stateCode, platformTypeId, activityId, staffingEffect, null, false, adherence);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, string stateCode, Guid? activityId, int staffingEffect, Adherence adherence)
		{
			return fakeDataBuilder.WithRule(Guid.NewGuid(), stateCode, null, activityId, staffingEffect, null, false, adherence);
		}

		public static FakeRtaDatabase WithRule(this FakeRtaDatabase fakeDataBuilder, Guid ruleId, string stateCode, Guid? activityId)
		{
			return fakeDataBuilder.WithRule(ruleId, stateCode, null, activityId, 0, null, false, null);
		}
	}

}