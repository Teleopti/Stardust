using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public interface IFakeDataBuilder
	{
		IFakeDataBuilder WithDefaultsFromState(ExternalUserStateForTest state);
		IFakeDataBuilder WithDataFromState(ExternalUserStateForTest state);
		IFakeDataBuilder WithSource(string sourceId);
		IFakeDataBuilder WithBusinessUnit(Guid businessUnitId);
		IFakeDataBuilder WithUser(string userCode, Guid personId, Guid? businessUnitId, Guid? teamId, Guid? siteId);
		IFakeDataBuilder WithSchedule(Guid personId, Guid activityId, string name, DateOnly date, string start, string end);
		IFakeDataBuilder WithAlarm(string stateCode, Guid activityId, Guid alarmId, double staffingEffect, string name, bool isLoggedOutState, TimeSpan threshold);
		FakeRtaDatabase Make();
	}

	public class FakeRtaDatabase : IDatabaseReader, IDatabaseWriter, IPersonOrganizationReader, IFakeDataBuilder
	{
		private readonly List<AgentStateReadModel> _actualAgentStates = new List<AgentStateReadModel>();
		private readonly List<KeyValuePair<string, int>> _datasources = new List<KeyValuePair<string, int>>();
		private readonly List<KeyValuePair<string, IEnumerable<ResolvedPerson>>> _externalLogOns = new List<KeyValuePair<string, IEnumerable<ResolvedPerson>>>();
		private readonly List<StateCodeInfo> _stateCodeInfos = new List<StateCodeInfo>();
		private readonly List<KeyValuePair<Tuple<Guid, Guid, Guid>, List<AlarmMappingInfo>>> _activityAlarms = new List<KeyValuePair<Tuple<Guid, Guid, Guid>, List<AlarmMappingInfo>>>();
		private readonly List<scheduleLayer2> _schedules = new List<scheduleLayer2>();
		private readonly List<PersonOrganizationData> _personOrganizationData = new List<PersonOrganizationData>();

		private class scheduleLayer2
		{
			public Guid PersonId { get; set; }
			public ScheduleLayer ScheduleLayer { get; set; }
		}

		public AgentStateReadModel PersistedAgentStateReadModel { get; set; }
		public StateCodeInfo AddedStateCode { get; set; }

		private Guid _businessUnitId;
		private string _platformTypeId;

		public FakeRtaDatabase()
		{
			_businessUnitId = Guid.NewGuid();
			WithDefaultsFromState(new ExternalUserStateForTest());
		}

		public IFakeDataBuilder WithDefaultsFromState(ExternalUserStateForTest state)
		{
			WithSource(state.SourceId);
			_platformTypeId = state.PlatformTypeId;
			return this;
		}

		public IFakeDataBuilder WithDataFromState(ExternalUserStateForTest state)
		{
			WithDefaultsFromState(state);
			return this.WithUser(state.UserCode, Guid.NewGuid());
		}

		public IFakeDataBuilder WithSource(string sourceId)
		{
			if (_datasources.Any(x => x.Key == sourceId))
				return this;
			_datasources.Add(new KeyValuePair<string, int>(sourceId, 0));
			return this;
		}

		public IFakeDataBuilder WithBusinessUnit(Guid businessUnitId)
		{
			_businessUnitId = businessUnitId;
			return this;
		}

		public IFakeDataBuilder WithUser(string userCode, Guid personId, Guid? businessUnitId, Guid? teamId, Guid? siteId)
		{
			if (!businessUnitId.HasValue) businessUnitId = _businessUnitId;
			if (!teamId.HasValue) teamId = Guid.NewGuid();
			if (!siteId.HasValue) siteId = Guid.NewGuid();

			var lookupKey = string.Format("{0}|{1}", _datasources.Last().Value, userCode).ToUpper(); //putting this logic here is just WRONG
			_externalLogOns.Add(
				new KeyValuePair<string, IEnumerable<ResolvedPerson>>(
					lookupKey, new[]
					{
						new ResolvedPerson
						{
							PersonId = personId,
							BusinessUnitId = businessUnitId.Value
						}
					}));

			_personOrganizationData.Add(new PersonOrganizationData
			{
				PersonId = personId,
				TeamId = teamId.Value,
				SiteId = siteId.Value,
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

		public IFakeDataBuilder WithAlarm(string stateCode, Guid activityId, Guid alarmId, double staffingEffect, string name, bool isLoggedOutState, TimeSpan threshold)
		{
			//putting all this logic here is just WRONG
			var platformTypeIdGuid = new Guid(_platformTypeId);

			var stateCodeInfo = getOrAddStateCodeInfo(stateCode, null, name, isLoggedOutState, platformTypeIdGuid);

			var alarms = new List<AlarmMappingInfo>
			{
				new AlarmMappingInfo
				{
					Name = name,
					StateGroupId = stateCodeInfo.StateGroupId,
					ActivityId = activityId,
					BusinessUnit = _businessUnitId,
					AlarmTypeId = alarmId,
					StaffingEffect = staffingEffect,
					ThresholdTime = threshold.Ticks
				}
			};
			_activityAlarms.Add(new KeyValuePair<Tuple<Guid, Guid, Guid>, List<AlarmMappingInfo>>(new Tuple<Guid, Guid, Guid>(activityId, stateCodeInfo.StateGroupId, _businessUnitId), alarms));
			return this;
		}

		public FakeRtaDatabase Make()
		{
			return this;
		}






		private StateCodeInfo getOrAddStateCodeInfo(string stateCode, string stateDescription, string stateGroupName, bool isLoggedOutState, Guid platformTypeId)
		{
			var match = (from s in _stateCodeInfos
						 where s.StateCode.ToUpper() == stateCode.ToUpper()
							   && s.PlatformTypeId == platformTypeId
							   && s.BusinessUnitId == _businessUnitId
						 select s).FirstOrDefault();

			if (match != null)
				return match;

			var stateGroupId = Guid.NewGuid();
			var stateCodeInfo = new StateCodeInfo
			{
				StateGroupId = stateGroupId,
				StateGroupName = stateGroupName,
				BusinessUnitId = _businessUnitId,
				PlatformTypeId = platformTypeId,
				StateCode = stateCode,
				StateName = stateDescription,
				IsLogOutState = isLoggedOutState
			};
			_stateCodeInfos.Add(stateCodeInfo);
			return stateCodeInfo;
		}

		public AgentStateReadModel GetCurrentActualAgentState(Guid personId)
		{
			return _actualAgentStates.FirstOrDefault(x => x.PersonId == personId);
		}

		public IEnumerable<AgentStateReadModel> GetActualAgentStates()
		{
			return _actualAgentStates.ToList();
		}

		public IEnumerable<StateCodeInfo> StateCodeInfos()
		{
			return _stateCodeInfos;
		}

		public ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<AlarmMappingInfo>> AlarmMappingInfos()
		{
			return new ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<AlarmMappingInfo>>(_activityAlarms);
		}

		public IList<ScheduleLayer> GetCurrentSchedule(Guid personId)
		{
			var layers = from l in _schedules
						 where l.PersonId == personId
						 select l.ScheduleLayer;
			return new List<ScheduleLayer>(layers);
		}

		public IEnumerable<AgentStateReadModel> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId)
		{
			return from s in _actualAgentStates.ToList()
				where s.OriginalDataSourceId == dataSourceId &&
				      (s.BatchId < batchId ||
					  s.BatchId == null)
				select s;
		}

		public TimeZoneInfo GetTimeZone(Guid personId)
		{
			return new UtcTimeZone().TimeZone();
		}

		public ConcurrentDictionary<string, IEnumerable<ResolvedPerson>> ExternalLogOns()
		{
			return new ConcurrentDictionary<string, IEnumerable<ResolvedPerson>>(_externalLogOns);
		}

		public ConcurrentDictionary<string, int> Datasources()
		{
			return new ConcurrentDictionary<string, int>(_datasources);
		}

		public StateCodeInfo AddAndGetStateCode(string stateCode, string stateDescription, Guid platformTypeId, Guid businessUnit)
		{
			AddedStateCode = getOrAddStateCodeInfo(stateCode, stateDescription, null, false, platformTypeId);
			return AddedStateCode;
		}

		public void PersistActualAgentReadModel(AgentStateReadModel model)
		{
			var previousState = (from s in _actualAgentStates where s.PersonId == model.PersonId select s).FirstOrDefault();
			if (previousState != null)
				_actualAgentStates.Remove(previousState);
			_actualAgentStates.Add(model);
			PersistedAgentStateReadModel = model;
		}

		public IEnumerable<PersonOrganizationData> PersonOrganizationData()
		{
			return _personOrganizationData;
		}
	}

	public static class FakeDatabaseBuilderExtensions
	{
		public static IFakeDataBuilder WithUser(this IFakeDataBuilder fakeDataBuilder, string userCode)
		{
			return fakeDataBuilder.WithUser(userCode, Guid.NewGuid(), null, null, null);
		}

		public static IFakeDataBuilder WithUser(this IFakeDataBuilder fakeDataBuilder, string userCode, Guid personId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, null, null, null);
		}

		public static IFakeDataBuilder WithUser(this IFakeDataBuilder fakeDataBuilder, string userCode, Guid personId, Guid businessUnitId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, businessUnitId, null, null);
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

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid activityId)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), 0, null, false, TimeSpan.Zero);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid activityId, double staffingEffect)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), staffingEffect, null, false, TimeSpan.Zero);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid activityId, Guid alarmId)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, alarmId, 0, null, false, TimeSpan.Zero);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid activityId, string name)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), 0, name, false, TimeSpan.Zero);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid activityId, bool isLoggedOutState)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), 0, null, isLoggedOutState, TimeSpan.Zero);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid activityId, TimeSpan threshold)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), 0, null, false, threshold);
		}
	}

}