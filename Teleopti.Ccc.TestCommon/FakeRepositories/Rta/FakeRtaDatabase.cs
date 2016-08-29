using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Interfaces.Domain;

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

	public class FakeRtaDatabase : IDatabaseReader
	{
		private readonly INow _now;
		private readonly FakeDatabase _database;
		private readonly FakeDataSources _dataSources;
		private readonly FakeAgentStateReadModelPersister _agentStateReadModels;
		private readonly FakeAgentStatePersister _agentStates;
		private readonly FakeRtaStateGroupRepository _rtaStateGroupRepository;
		private readonly FakeRtaMapRepository _rtaMapRepository;
		private readonly FakeTenants _tenants;
		private readonly FakeDataSourceForTenant _dataSourceForTenant;
		private readonly FakeBusinessUnitRepository _businessUnits;

		private class userData
		{
			public int DataSourceId;
			public PersonOrganizationData Data;
		}

		private readonly List<userData> _userInfos = new List<userData>();

		private class scheduleLayer2
		{
			public Guid PersonId { get; set; }
			public ScheduledActivity ScheduledActivity { get; set; }
		}
		private readonly List<scheduleLayer2> _schedules = new List<scheduleLayer2>();

		public FakeRtaDatabase(
			INow now,
			FakeDatabase database,
			FakeDataSources dataSources,
			FakeAgentStateReadModelPersister agentStateReadModels,
			FakeAgentStatePersister agentStates,
			FakeRtaStateGroupRepository rtaStateGroupRepository,
			FakeRtaMapRepository rtaMapRepository,
			FakeTenants tenants,
			FakeDataSourceForTenant dataSourceForTenant,
			FakeBusinessUnitRepository businessUnits
			)
		{
			_now = now;
			_database = database;
			_dataSources = dataSources;
			_agentStateReadModels = agentStateReadModels;
			_agentStates = agentStates;
			_rtaStateGroupRepository = rtaStateGroupRepository;
			_rtaMapRepository = rtaMapRepository;
			_tenants = tenants;
			_dataSourceForTenant = dataSourceForTenant;
			_businessUnits = businessUnits;

			withTenant("default", LegacyAuthenticationKey.TheKey);
			WithSource(new StateForTest().SourceId);
			WithPlatform(new Guid(new StateForTest().PlatformTypeId));
		}

		public AgentState StoredState => _agentStates.GetAll().SingleOrDefault();
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
			if (businessUnitId.HasValue)
				WithBusinessUnit(businessUnitId.Value);
			
			if (!teamId.HasValue) teamId = Guid.NewGuid();
			if (!siteId.HasValue) siteId = Guid.NewGuid();
			
			_userInfos.Add(new userData
			{
				DataSourceId = _database.CurrentDataSourceId(),
				Data = new PersonOrganizationData
				{
					UserCode = userCode,
					PersonId = personId,
					BusinessUnitId = _database.CurrentBusinessUnitId(),
					TeamId = teamId.Value,
					SiteId = siteId.Value
				}
			});

			_agentStates.Prepare(new AgentStatePrepare
			{
				PersonId = personId,
				BusinessUnitId = _database.CurrentBusinessUnitId(),
				TeamId = teamId.Value,
				SiteId = siteId.Value,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = _database.CurrentDataSourceId(),
						UserCode = userCode
					}
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

		public IEnumerable<ScheduledActivity> GetCurrentSchedules(IEnumerable<Guid> personIds)
		{
			return (
				from l in _schedules
				where
					personIds.Contains(l.PersonId) &&
					l.ScheduledActivity.BelongsToDate.Date >= _now.UtcDateTime().Date.AddDays(-1) &&
					l.ScheduledActivity.BelongsToDate.Date <= _now.UtcDateTime().Date.AddDays(1)
				select l.ScheduledActivity.CopyBySerialization()
				).ToList();
		}

		public ConcurrentDictionary<string, int> Datasources()
		{
			return new ConcurrentDictionary<string, int>(
				_dataSources
					.Datasources
					.GroupBy(x => x.Key, (key, g) => g.First()
					));
		}

		public IEnumerable<PersonOrganizationData> LoadPersonOrganizationData(int dataSourceId, string externalLogOn)
		{
			return _userInfos
				.Where(x =>
					x.Data.UserCode == externalLogOn &&
					x.DataSourceId == dataSourceId)
				.Select(m => m.Data.CopyBySerialization())
				.ToArray();
		}

		public IEnumerable<PersonOrganizationData> LoadPersonOrganizationDatas(int dataSourceId, IEnumerable<string> externalLogOns)
		{
			return _userInfos
				.Where(x =>
					externalLogOns.Contains(x.Data.UserCode) &&
					x.DataSourceId == dataSourceId)
				.Select(m => m.Data.CopyBySerialization())
				.ToArray();
		}

		public IEnumerable<PersonOrganizationData> LoadAllPersonOrganizationData()
		{
			return _userInfos
				.Select(m => m.Data.CopyBySerialization())
				.ToArray();
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