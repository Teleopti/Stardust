using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class ExternalUserStateForTest : ExternalUserStateInputModel
	{
		public ExternalUserStateForTest()
		{
			AuthenticationKey = ConfiguredKeyAuthenticator.LegacyAuthenticationKey;
			PlatformTypeId = Guid.Empty.ToString();
			SourceId = "sourceId";
			UserCode = "8808";
			StateCode = "AUX2";
			IsLoggedOn = true;
		}
	}

	public interface IFakeDataBuilder
	{
		IFakeDataBuilder WithDefaultsFromState(ExternalUserStateForTest state);
		IFakeDataBuilder WithDataFromState(ExternalUserStateForTest state);
		IFakeDataBuilder WithTenant(string name, string key);
		IFakeDataBuilder WithSource(string sourceId);
		IFakeDataBuilder WithBusinessUnit(Guid businessUnitId);
		IFakeDataBuilder WithUser(string userCode, Guid personId, string source, Guid? businessUnitId, Guid? teamId, Guid? siteId);
		IFakeDataBuilder WithSchedule(Guid personId, Guid activityId, string name, DateOnly date, string start, string end);
		IFakeDataBuilder WithRule(string stateCode, Guid? activityId, Guid? alarmId, int staffingEffect, string name, bool isLoggedOutState, TimeSpan? threshold, Adherence? adherence, Guid? platformTypeId);
		IFakeDataBuilder WithDefaultStateGroup();
		IFakeDataBuilder WithStateCode(string statecode);
		IFakeDataBuilder WithStateCode(string statecode, string platformTypeId);
		IFakeDataBuilder WithExistingState(Guid personId, string stateCode, int staffingEffect);
	}

	public class FakeRtaDatabase : IDatabaseReader, IFakeDataBuilder
	{
		private readonly IConfigReader _config;
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

		private readonly List<KeyValuePair<string, int>> _datasources = new List<KeyValuePair<string, int>>();
		private readonly List<scheduleLayer2> _schedules = new List<scheduleLayer2>();

		private readonly List<KeyValuePair<string, IEnumerable<ResolvedPerson>>> _externalLogOns = new List<KeyValuePair<string, IEnumerable<ResolvedPerson>>>();
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
			IConfigReader config,
			INow now,
			FakeAgentStateReadModelStorage agentStateReadModels,
			FakeRtaStateGroupRepository rtaStateGroupRepository,
			FakeRtaMapRepository rtaMapRepository,
			FakeTenants tenants,
			FakeTeamOutOfAdherenceReadModelPersister teamOutOfAdherenceReadModelPersister,
			FakeSiteOutOfAdherenceReadModelPersister siteOutOfAdherenceReadModelPersister,
			FakeAdherenceDetailsReadModelPersister adherenceDetailsReadModelPersister,
			FakeAdherencePercentageReadModelPersister adherencePercentageReadModelPersister,
			FakeDataSourceForTenant dataSourceForTenant, 
			IProperAlarm properAlarm)
		{
			_config = config;
			_now = now;
			_properAlarm = properAlarm;
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
			WithTenant("default", ConfiguredKeyAuthenticator.LegacyAuthenticationKey);
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

		private readonly IProperAlarm _properAlarm;

		public StoredStateInfo StoredStateFor(Guid personId)
		{
			return new StoredStateInfo(personId, AgentStateReadModels.GetCurrentActualAgentState(personId));
		}

		public IRtaState AddedStateCode
		{
			get { return RtaStateGroupRepository.LoadAll().Single(x => x.DefaultStateGroup).StateCollection.SingleOrDefault(); }
		}

		public IEnumerable<IRtaState> AddedStateCodes
		{
			get { return RtaStateGroupRepository.LoadAll().Single().StateCollection; }
		}

		public IFakeDataBuilder WithDefaultsFromState(ExternalUserStateForTest state)
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

		public IFakeDataBuilder WithDataFromState(ExternalUserStateForTest state)
		{
			WithDefaultsFromState(state);
			return this.WithUser(state.UserCode, Guid.NewGuid());
		}

		public IFakeDataBuilder WithTenant(string name, string key)
		{
			// only required without multi-tenancy I think...
			// because then the rta requires all tenants to be loaded at startup to find the datasource from a connection string

			var dataSource = new FakeDataSource(name)
			{
				Application = new FakeUnitOfWorkFactory
				{
					Name = name,
					ConnectionString = _config.ConnectionString("RtaApplication")
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

		public IFakeDataBuilder WithSource(string sourceId)
		{
			if (_datasources.Any(x => x.Key == sourceId))
				return this;
			_datasources.Add(new KeyValuePair<string, int>(sourceId, random.Next(0, 1000)));
			return this;
		}

		public IFakeDataBuilder WithBusinessUnit(Guid businessUnitId)
		{
			_businessUnitId = businessUnitId;
			_businessUnit = new BusinessUnit(".");
			_businessUnit.SetId(_businessUnitId);
			return this;
		}

		public IFakeDataBuilder WithUser(string userCode, Guid personId, string source, Guid? businessUnitId, Guid? teamId, Guid? siteId)
		{
			if (businessUnitId.HasValue)
				WithBusinessUnit(businessUnitId.Value);
			
			if (!teamId.HasValue) teamId = Guid.NewGuid();
			if (!siteId.HasValue) siteId = Guid.NewGuid();

			var dataSource = _datasources.Last().Value;
			if (_datasources.Any(x => x.Key == source))
				dataSource = _datasources.Single(x => x.Key == source).Value;

			var lookupKey = string.Format("{0}|{1}", dataSource, userCode).ToUpper(); //putting this logic here is just WRONG
			_externalLogOns.Add(
				new KeyValuePair<string, IEnumerable<ResolvedPerson>>(
					lookupKey, new[]
					{
						new ResolvedPerson
						{
							PersonId = personId,
							BusinessUnitId = _businessUnitId
						}
					}));

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

		public IFakeDataBuilder WithSchedule(Guid personId, Guid activityId, string name, DateOnly belongsToDate, string start, string end)
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

		public IFakeDataBuilder ClearSchedule(Guid personId)
		{
			_schedules.RemoveAll(x => x.PersonId == personId);
			return this;
		}

		public IFakeDataBuilder WithRule(string stateCode, Guid? activityId, Guid? alarmId, int staffingEffect, string name, bool isLoggedOutState, TimeSpan? threshold, Adherence? adherence, Guid? platformTypeId)
		{
			IRtaRule _rtaRule = null;
			if (alarmId != null)
			{
				_rtaRule = new RtaRule();
				_rtaRule.SetId(alarmId);
				_rtaRule.SetBusinessUnit(_businessUnit);
				_rtaRule.StaffingEffect = staffingEffect;
				_rtaRule.IsAlarm = _properAlarm.IsAlarm(threshold);
				_rtaRule.ThresholdTime = threshold ?? TimeSpan.Zero;
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
					stateGroup = new RtaStateGroup(name, false, true);
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

		public IFakeDataBuilder WithDefaultStateGroup()
		{
			var defaultStateGroup = RtaStateGroupRepository.LoadAll().SingleOrDefault(x => x.DefaultStateGroup);
			if (defaultStateGroup == null)
			{
				defaultStateGroup = new RtaStateGroup(".", true, true);
				defaultStateGroup.SetId(Guid.NewGuid());
				defaultStateGroup.SetBusinessUnit(_businessUnit);
				RtaStateGroupRepository.Add(defaultStateGroup);
			}
			return this;
		}

		public IFakeDataBuilder WithStateCode(string statecode)
		{
			WithStateCode(statecode, _platformTypeId);
			return this;
		}

		public IFakeDataBuilder WithStateCode(string statecode, string platformTypeId)
		{
			var defaultStateGroup = RtaStateGroupRepository.LoadAll().SingleOrDefault(x => x.DefaultStateGroup);
			if (defaultStateGroup == null)
				return this;
			defaultStateGroup.AddState(statecode, statecode, withPlatform(platformTypeId));
			return this;
		}

		public IFakeDataBuilder WithExistingState(Guid personId, string stateCode)
		{
			AgentStateReadModels.Has(new AgentStateReadModel
			{
				PersonId = personId,
				BusinessUnitId = _businessUnitId,
				PlatformTypeId = new Guid(_platformTypeId),
				StateCode = stateCode,
			});
			return this;
		}

		public IFakeDataBuilder WithExistingState(Guid personId, string stateCode, int staffingEffect)
		{
			AgentStateReadModels.Has(new AgentStateReadModel
			{
				PersonId = personId,
				BusinessUnitId = _businessUnitId,
				PlatformTypeId = new Guid(_platformTypeId),
				StateCode = stateCode,
				StaffingEffect = staffingEffect
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
		
		public ConcurrentDictionary<string, IEnumerable<ResolvedPerson>> ExternalLogOns()
		{
			return new ConcurrentDictionary<string, IEnumerable<ResolvedPerson>>(_externalLogOns);
		}

		public IEnumerable<PersonOrganizationData> PersonOrganizationData()
		{
			return _personOrganizationData
				.Select(m => JsonConvert.DeserializeObject<PersonOrganizationData>(JsonConvert.SerializeObject(m)))
				.ToArray();
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
		public static IFakeDataBuilder WithTenant(this IFakeDataBuilder fakeDataBuilder, string key)
		{
			return fakeDataBuilder.WithTenant(key, key);
		}

		public static IFakeDataBuilder WithUser(this IFakeDataBuilder fakeDataBuilder, string userCode)
		{
			return fakeDataBuilder.WithUser(userCode, Guid.NewGuid(), null, null, null, null);
		}

		public static IFakeDataBuilder WithUser(this IFakeDataBuilder fakeDataBuilder, string userCode, Guid personId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, null, null, null, null);
		}

		public static IFakeDataBuilder WithUser(this IFakeDataBuilder fakeDataBuilder, string userCode, Guid personId, Guid businessUnitId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, null, businessUnitId, null, null);
		}
		
		public static IFakeDataBuilder WithUser(this IFakeDataBuilder fakeDataBuilder, string userCode, string source, Guid personId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, source, null, null, null);
		}

		public static IFakeDataBuilder WithUser(this IFakeDataBuilder fakeDataBuilder, string userCode, Guid personId, Guid? businessUnitId, Guid? teamId, Guid? siteId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, null, businessUnitId, teamId, siteId);
		}

		public static IFakeDataBuilder WithSchedule(this IFakeDataBuilder fakeDataBuilder, Guid personId, Guid activityId, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, null, new DateOnly(start.Utc()), start, end);
		}

		public static IFakeDataBuilder WithSchedule(this IFakeDataBuilder fakeDataBuilder, Guid personId, Guid activityId, string name, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, name, new DateOnly(start.Utc()), start, end);
		}

		public static IFakeDataBuilder WithSchedule(this IFakeDataBuilder fakeDataBuilder, Guid personId, Guid activityId, DateOnly belongsToDate, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, null, belongsToDate, start, end);
		}
		
		public static IFakeDataBuilder WithRule(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid? activityId)
		{
			return fakeDataBuilder.WithRule(stateCode, activityId, Guid.NewGuid(), 0, null, false, null, null, null);
		}

		public static IFakeDataBuilder WithRule(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid? activityId, int staffingEffect)
		{
			return fakeDataBuilder.WithRule(stateCode, activityId, Guid.NewGuid(), staffingEffect, null, false, null, null, null);
		}

		public static IFakeDataBuilder WithRule(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid? activityId, Guid? alarmId)
		{
			return fakeDataBuilder.WithRule(stateCode, activityId, alarmId, 0, null, false, null, null, null);
		}

		public static IFakeDataBuilder WithRule(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid? activityId, string name)
		{
			return fakeDataBuilder.WithRule(stateCode, activityId, Guid.NewGuid(), 0, name, false, null, null, null);
		}

		public static IFakeDataBuilder WithRule(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid? activityId, bool isLoggedOutState)
		{
			return fakeDataBuilder.WithRule(stateCode, activityId, Guid.NewGuid(), 0, null, isLoggedOutState, null, null, null);
		}

		public static IFakeDataBuilder WithRule(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid? activityId, TimeSpan threshold)
		{
			return fakeDataBuilder.WithRule(stateCode, activityId, Guid.NewGuid(), 0, null, false, threshold, null, null);
		}

		public static IFakeDataBuilder WithRule(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid platformTypeId, Guid activityId, int staffingEffect, Adherence adherence)
		{
			return fakeDataBuilder.WithRule(stateCode, activityId, Guid.NewGuid(), staffingEffect, null, false, null, adherence, platformTypeId);
		}

		public static IFakeDataBuilder WithRule(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid? activityId, int staffingEffect, Adherence adherence)
		{
			return fakeDataBuilder.WithRule(stateCode, activityId, Guid.NewGuid(), staffingEffect, null, false, null, adherence, null);
		}
		
		public static IFakeDataBuilder WithRule(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid? activityId, int staffingEffect, Adherence adherence, TimeSpan thresholdTime)
		{
			return fakeDataBuilder.WithRule(stateCode, activityId, Guid.NewGuid(), staffingEffect, null, false, thresholdTime, adherence, null);
		}

		public static IFakeDataBuilder WithRule(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid activityId, Guid alarmId, int staffingEffect, Adherence adherence, TimeSpan thresholdTime)
		{
			return fakeDataBuilder.WithRule(stateCode, activityId, alarmId, staffingEffect, null, false, thresholdTime, adherence, null);
		}

		public static IFakeDataBuilder WithExistingState(this IFakeDataBuilder fakeDataBuilder, Guid personId, string stateCode)
		{
			return fakeDataBuilder.WithExistingState(personId, stateCode, 0);
		}
	}

}