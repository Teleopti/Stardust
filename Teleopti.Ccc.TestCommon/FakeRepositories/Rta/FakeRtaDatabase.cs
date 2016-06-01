using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Scheduling;
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
		private readonly FakeAgentStateReadModelPersister _agentStateReadModels;
		private readonly FakeAgentStatePersister _agentStates;
		private readonly FakeRtaStateGroupRepository _rtaStateGroupRepository;
		private readonly FakeRtaMapRepository _rtaMapRepository;
		private readonly FakeTenants _tenants;
		private readonly FakeDataSourceForTenant _dataSourceForTenant;

		private readonly List<KeyValuePair<string, int>> _datasources = new List<KeyValuePair<string, int>>();

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
			public ScheduledActivity ScheduledActivity { get; set; }
		}
		private readonly List<scheduleLayer2> _schedules = new List<scheduleLayer2>();

		private BusinessUnit _businessUnit;
		private Guid _businessUnitId;
		private string _platformTypeId;
		private RtaRule _rtaRule;

		public FakeRtaDatabase(
			INow now,
			FakeAgentStateReadModelPersister agentStateReadModels,
			FakeAgentStatePersister agentStates,
			FakeRtaStateGroupRepository rtaStateGroupRepository,
			FakeRtaMapRepository rtaMapRepository,
			FakeTenants tenants,
			FakeDataSourceForTenant dataSourceForTenant
			)
		{
			_now = now;
			_agentStateReadModels = agentStateReadModels;
			_agentStates = agentStates;
			_rtaStateGroupRepository = rtaStateGroupRepository;
			_rtaMapRepository = rtaMapRepository;
			_tenants = tenants;
			_dataSourceForTenant = dataSourceForTenant;

			withTenant("default", LegacyAuthenticationKey.TheKey);
			WithBusinessUnit(Guid.NewGuid());
			WithSource(new ExternalUserStateForTest().SourceId);
			withPlatform(new Guid(new ExternalUserStateForTest().PlatformTypeId));
		}

		public AgentState StoredState => _agentStates.GetAll().SingleOrDefault();
		public AgentState StoredStateFor(Guid personId) => _agentStates.Get(personId);
		public AgentStateReadModel PersistedReadModel => _agentStateReadModels.Models.SingleOrDefault();
		public IEnumerable<IRtaState> StateCodes => _rtaStateGroupRepository.LoadAll().Single().StateCollection;
		
		public FakeRtaDatabase WithPlatform(Guid platformTypeId)
		{
			withPlatform(platformTypeId);
			return this;
		}
		
		private Guid withPlatform(Guid? platformTypeId)
		{
			if (!platformTypeId.HasValue)
				return new Guid(_platformTypeId);
			_platformTypeId = platformTypeId.Value.ToString();
			return platformTypeId.Value;
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
			if (_datasources.Any(x => x.Key == sourceId))
				return this;
			_datasources.Add(new KeyValuePair<string, int>(sourceId, new Random().Next(0, 1000)));
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
			
			_userInfos.Add(new userData
			{
				ExternalLogOn = userCode,
				DataSourceId = dataSource,
				Data = new PersonOrganizationData
				{
					PersonId = personId,
					BusinessUnitId = _businessUnitId,
					TeamId = teamId.Value,
					SiteId = siteId.Value
				}
			});
			return this;
		}

		public FakeRtaDatabase WithSchedule(Guid personId, Guid activityId, string start, string end, DateOnly? belongsToDate, string name, Color? color)
		{
			if (!belongsToDate.HasValue)
				belongsToDate = new DateOnly(start.Utc());
			if (!color.HasValue)
				color = Color.Black;
			_schedules.Add(new scheduleLayer2
			{
				PersonId = personId,
				ScheduledActivity = new ScheduledActivity
				{
					PayloadId = activityId,
					Name = name,
					StartDateTime = start.Utc(),
					EndDateTime = end.Utc(),
					BelongsToDate = belongsToDate.Value,
					DisplayColor = color.Value.ToArgb()
				}
			});
			return this;
		}
		
		public FakeRtaDatabase ClearSchedule(Guid personId)
		{
			_schedules.RemoveAll(x => x.PersonId == personId);
			return this;
		}

		public FakeRtaDatabase WithRule(Guid? ruleId, string stateCode, Guid? platformTypeId, Guid? activityId, int staffingEffect, string name, Adherence? adherence)
		{
			_rtaRule = null;
			if (ruleId != null)
			{
				_rtaRule = new RtaRule();
				if (name != null)
					_rtaRule.Description = new Description(name);
				_rtaRule.SetId(ruleId);
				_rtaRule.SetBusinessUnit(_businessUnit);
				_rtaRule.StaffingEffect = staffingEffect;
				_rtaRule.Adherence = adherence;
			}

			IRtaStateGroup stateGroup = null;
			if (stateCode != null)
			{
				stateGroup = (
					from g in _rtaStateGroupRepository.LoadAll()
					from s in g.StateCollection
					where s.StateCode == stateCode &&
						  s.PlatformTypeId == withPlatform(platformTypeId)
					select g
					).FirstOrDefault();
				if (stateGroup == null)
				{
					var isDefaultStateGroup = _rtaStateGroupRepository.LoadAll().IsEmpty();
					stateGroup = new RtaStateGroup(name, isDefaultStateGroup, true);
					stateGroup.SetId(Guid.NewGuid());
					stateGroup.SetBusinessUnit(_businessUnit);
					stateGroup.AddState(null, stateCode, withPlatform(platformTypeId));
					_rtaStateGroupRepository.Add(stateGroup);
				}
			}

			IActivity activity = null;
			if (activityId != null)
			{
				activity = new Activity(stateCode ?? "activity");
				activity.SetId(activityId);
				activity.SetBusinessUnit(_businessUnit);
			}

			var mapping = new RtaMap(stateGroup, activity) {RtaRule = _rtaRule};
			mapping.SetId(Guid.NewGuid());
			mapping.SetBusinessUnit(_businessUnit);
			_rtaMapRepository.Add(mapping);

			return this;
		}

		public FakeRtaDatabase WithMapWithStateGroupWithoutStateCodes()
		{
			var stateGroup = new RtaStateGroup("Empty", false, true);
			stateGroup.SetId(Guid.NewGuid());
			stateGroup.SetBusinessUnit(_businessUnit);
			_rtaStateGroupRepository.Add(stateGroup);

			var mapping = new RtaMap(stateGroup, null) {RtaRule = _rtaRule};
			mapping.SetId(Guid.NewGuid());
			mapping.SetBusinessUnit(_businessUnit);
			_rtaMapRepository.Add(mapping);

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
			_rtaRule.IsAlarm = true;
			_rtaRule.ThresholdTime = threshold;
			return this;
		}
		
		public FakeRtaDatabase WithExistingAgentState(Guid personId, string stateCode)
		{
			_agentStates.Has(new AgentState
			{
				PersonId = personId,
				BusinessUnitId = _businessUnitId,
				PlatformTypeId = new Guid(_platformTypeId),
				StateCode = stateCode,
				StaffingEffect = 0,
				SourceId = new ExternalUserStateForTest().SourceId
			});
			return this;
		}

		public IList<ScheduledActivity> GetCurrentSchedule(Guid personId)
		{
			return (
				from l in _schedules
				where
					l.PersonId == personId &&
					l.ScheduledActivity.BelongsToDate.Date >= _now.UtcDateTime().Date.AddDays(-1) &&
					l.ScheduledActivity.BelongsToDate.Date <= _now.UtcDateTime().Date.AddDays(1)
				select l.ScheduledActivity.CopyBySerialization()
				).ToList();
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

	public static class FakeDatabaseUserExtensions
	{
		public static FakeRtaDatabase WithUser(this FakeRtaDatabase fakeDataBuilder, string userCode)
		{
			return fakeDataBuilder.WithUser(userCode, Guid.NewGuid(), null, null, null, null);
		}

		public static FakeRtaDatabase WithUser(this FakeRtaDatabase fakeDataBuilder, string userCode, Guid personId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, null, null, null, null);
		}

		public static FakeRtaDatabase WithUser(this FakeRtaDatabase fakeDataBuilder, string userCode, string source, Guid personId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, source, null, null, null);
		}

		public static FakeRtaDatabase WithUser(this FakeRtaDatabase fakeDataBuilder, string userCode, Guid personId, Guid? businessUnitId, Guid? teamId, Guid? siteId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, null, businessUnitId, teamId, siteId);
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

		public static FakeRtaDatabase WithSchedule(this FakeRtaDatabase fakeDataBuilder, Guid personId, Guid activityId, DateOnly belongsToDate, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, start, end, belongsToDate, null, null);
		}

		public static FakeRtaDatabase WithSchedule(this FakeRtaDatabase fakeDataBuilder, Guid personId, Guid activityId, string name, DateOnly belongsToDate, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, start, end, belongsToDate, null, Color.Black);
		}

		public static FakeRtaDatabase WithSchedule(this FakeRtaDatabase fakeDataBuilder, Guid personId, Color color, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, Guid.NewGuid(), start, end, new DateOnly(start.Utc()), null, color);
		}

		public static FakeRtaDatabase WithSchedule(this FakeRtaDatabase fakeDataBuilder, Guid personId, string name, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, Guid.NewGuid(), start, end, new DateOnly(start.Utc()), name, null);
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