using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Rta;
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
		IFakeDataBuilder WithSchedule(Guid personId, Guid activityId, string name, string start, string end);
		IFakeDataBuilder WithAlarm(string stateCode, Guid activityId, Guid alarmId, double staffingEffect, string name, bool isLoggedOutState);
		FakeRtaDatabase Make();
	}

	public class FakeRtaDatabase : IDatabaseReader, IDatabaseWriter, IPersonOrganizationReader, IFakeDataBuilder
	{
		private readonly List<IActualAgentState> _actualAgentStates = new List<IActualAgentState>();
		private readonly List<KeyValuePair<string, int>> _datasources = new List<KeyValuePair<string, int>>();
		private readonly List<KeyValuePair<string, IEnumerable<PersonWithBusinessUnit>>> _externalLogOns = new List<KeyValuePair<string, IEnumerable<PersonWithBusinessUnit>>>();
		//private readonly List<KeyValuePair<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>>> _stateGroups = new List<KeyValuePair<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>>>();
		private readonly Dictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>> _stateGroups = new Dictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>>();
		private readonly List<KeyValuePair<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>>> _activityAlarms = new List<KeyValuePair<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>>>();
		private readonly List<scheduleLayer2> _schedules = new List<scheduleLayer2>();
		private readonly List<PersonOrganizationData> _personOrganizationData = new List<PersonOrganizationData>();

		private class scheduleLayer2
		{
			public Guid PersonId { get; set; }
			public ScheduleLayer ScheduleLayer { get; set; }
		}

		public IActualAgentState PersistedActualAgentState { get; set; }

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
				new KeyValuePair<string, IEnumerable<PersonWithBusinessUnit>>(
					lookupKey, new[]
					{
						new PersonWithBusinessUnit
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

		public IFakeDataBuilder WithSchedule(Guid personId, Guid activityId, string name, string start, string end)
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
					BelongsToDate = new DateOnly(start.Utc().Date)
				}
			});
			return this;
		}


		public IFakeDataBuilder ClearSchedule(Guid personId)
		{
			_schedules.RemoveAll(x => x.PersonId == personId);
			return this;
		}

		public IFakeDataBuilder WithAlarm(string stateCode, Guid activityId, Guid alarmId, double staffingEffect, string name, bool isLoggedOutState)
		{
			//putting all this logic here is just WRONG
			var platformTypeIdGuid = new Guid(_platformTypeId);

			var stateGroup = getOrAddState(stateCode, name, isLoggedOutState, platformTypeIdGuid);

			var alarms = new List<RtaAlarmLight>
			{
				new RtaAlarmLight
				{
					Name = name,
					StateGroupId = stateGroup.StateGroupId,
					ActivityId = activityId,
					BusinessUnit = _businessUnitId,
					AlarmTypeId = alarmId,
					StaffingEffect = staffingEffect,
					StateGroupName = name
				}
			};
			_activityAlarms.Add(new KeyValuePair<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>>(new Tuple<Guid, Guid, Guid>(activityId, stateGroup.StateGroupId, _businessUnitId), alarms));
			return this;
		}

		private RtaStateGroupLight getOrAddState(string stateCode, string name, bool isLoggedOutState, Guid platformTypeIdGuid)
		{
			var stateGroupId = Guid.NewGuid();
			var stateId = Guid.NewGuid();
			var stateGroupKey = new Tuple<string, Guid, Guid>(stateCode.ToUpper(), platformTypeIdGuid, _businessUnitId);
			
			if (_stateGroups.ContainsKey(stateGroupKey)) return _stateGroups[stateGroupKey].Single();

			var states = new List<RtaStateGroupLight>
			{
				new RtaStateGroupLight
				{
					StateGroupId = stateGroupId,
					StateGroupName = name,
					BusinessUnitId = _businessUnitId,
					PlatformTypeId = platformTypeIdGuid,
					StateCode = stateCode,
					StateId = stateId,
					IsLogOutState = isLoggedOutState
				}
			};
			_stateGroups.Add(new Tuple<string, Guid, Guid>(stateCode.ToUpper(), platformTypeIdGuid, _businessUnitId), states);
			return states.Single();
		}

		public FakeRtaDatabase Make()
		{
			return this;
		}



		public IActualAgentState GetCurrentActualAgentState(Guid personId)
		{
			return _actualAgentStates.FirstOrDefault(x => x.PersonId == personId);
		}

		public IEnumerable<IActualAgentState> GetActualAgentStates()
		{
			return _actualAgentStates;
		}

		public ConcurrentDictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>> StateGroups()
		{
			return new ConcurrentDictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>>(_stateGroups);
		}

		public ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>> ActivityAlarms()
		{
			return new ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>>(_activityAlarms);
		}

		public IList<ScheduleLayer> GetCurrentSchedule(Guid personId)
		{
			var layers = from l in _schedules
						 where l.PersonId == personId
						 select l.ScheduleLayer;
			return new List<ScheduleLayer>(layers);
		}

		public IEnumerable<IActualAgentState> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId)
		{
			return from s in _actualAgentStates.ToList()
				where s.OriginalDataSourceId == dataSourceId &&
				      (s.BatchId < batchId ||
					  s.BatchId == null)
				select s;
		}

		public ConcurrentDictionary<string, IEnumerable<PersonWithBusinessUnit>> ExternalLogOns()
		{
			return new ConcurrentDictionary<string, IEnumerable<PersonWithBusinessUnit>>(_externalLogOns);
		}

		public ConcurrentDictionary<string, int> Datasources()
		{
			return new ConcurrentDictionary<string, int>(_datasources);
		}

		public RtaStateGroupLight AddAndGetNewRtaState(string stateCode, Guid platformTypeId, Guid businessUnit)
		{
			return getOrAddState(stateCode, null, false, platformTypeId);
		}

		public void PersistActualAgentState(IActualAgentState actualAgentState)
		{
			var previousState = (from s in _actualAgentStates where s.PersonId == actualAgentState.PersonId select s).FirstOrDefault();
			if (previousState != null)
				_actualAgentStates.Remove(previousState);
			_actualAgentStates.Add(actualAgentState);
			PersistedActualAgentState = actualAgentState;
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
			return fakeDataBuilder.WithSchedule(personId, activityId, null, start, end);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid activityId)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), 0, null, false);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid activityId, double staffingEffect)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), staffingEffect, null, false);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid activityId, Guid alarmId)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, alarmId, 0, null, false);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid activityId, string name)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), 0, name, false);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid activityId, bool isLoggedOutState)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), 0, null, isLoggedOutState);
		}
	}

}