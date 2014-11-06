using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public interface IFakeDataBuilder
	{
		IFakeDataBuilder WithDefaultsFromState(ExternalUserStateForTest state);
		IFakeDataBuilder WithDataFromState(ExternalUserStateForTest state);
		IFakeDataBuilder WithSource(string sourceId);
		IFakeDataBuilder WithUser(string userCode);
		IFakeDataBuilder WithUser(string userCode, Guid personId);
		IFakeDataBuilder WithUser(string userCode, Guid personId, Guid businessUnitId);
		IFakeDataBuilder WithUser(string userCode, Guid personId, Guid? businessUnitId, Guid? teamId, Guid? siteId);
		IFakeDataBuilder WithSchedule(Guid personId, Guid activityId, DateTime start, DateTime end);
		IFakeDataBuilder WithAlarm(string stateCode, Guid activityId, double staffingEffect);
		FakeRtaDatabase Make();
	}

	public class FakeRtaDatabase : IDatabaseReader, IDatabaseWriter, IPersonOrganizationReader, IFakeDataBuilder
	{
		private readonly List<IActualAgentState> _actualAgentStates = new List<IActualAgentState>();
		private readonly List<KeyValuePair<string, int>> _datasources = new List<KeyValuePair<string, int>>();
		private readonly List<KeyValuePair<string, IEnumerable<PersonWithBusinessUnit>>> _externalLogOns = new List<KeyValuePair<string, IEnumerable<PersonWithBusinessUnit>>>();
		private readonly List<KeyValuePair<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>>> _stateGroups = new List<KeyValuePair<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>>>();
		private readonly List<KeyValuePair<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>>> _activityAlarms = new List<KeyValuePair<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>>>();
		private readonly IDictionary<Guid, ScheduleLayer> _schedule = new Dictionary<Guid, ScheduleLayer>();
		private readonly List<PersonOrganizationData> _personOrganizationData = new List<PersonOrganizationData>();
		 
		public IActualAgentState PersistedActualAgentState { get; set; }

		private readonly Guid _businessUnitId;
		private string _platformTypeId;

		public FakeRtaDatabase()
		{
			_businessUnitId = Guid.NewGuid();
			_platformTypeId = Guid.Empty.ToString();
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
			WithUser(state.UserCode, Guid.NewGuid());
			return this;
		}

		public IFakeDataBuilder WithSource(string sourceId)
		{
			_datasources.Add(new KeyValuePair<string, int>(sourceId, 0));
			return this;
		}

		public IFakeDataBuilder WithUser(string userCode)
		{
			return WithUser(userCode, Guid.NewGuid(), null, null, null);
		}

		public IFakeDataBuilder WithUser(string userCode, Guid personId)
		{
			return WithUser(userCode, personId, null, null, null);
		}

		public IFakeDataBuilder WithUser(string userCode, Guid personId, Guid businessUnitId)
		{
			WithUser(userCode, personId, businessUnitId, null, null);
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

		public IFakeDataBuilder WithSchedule(Guid personId, Guid activityId, DateTime start, DateTime end)
		{
			_schedule.Add(personId, new ScheduleLayer { PayloadId = activityId, StartDateTime = start, EndDateTime = end });
			return this;
		}

		public IFakeDataBuilder WithAlarm(string stateCode, Guid activityId, double staffingEffect)
		{
			//putting all this logic here is just WRONG
			var stateGroupId = Guid.NewGuid();
			var stateId = Guid.NewGuid();
			var platformTypeIdGuid = new Guid(_platformTypeId);
			var states = new List<RtaStateGroupLight>
			{
				new RtaStateGroupLight
				{
					StateGroupId = stateGroupId,
					StateGroupName = "",
					BusinessUnitId = _businessUnitId,
					StateName = stateCode,
					PlatformTypeId = platformTypeIdGuid,
					StateCode = stateCode,
					StateId = stateId
				}
			};
			_stateGroups.Add(new KeyValuePair<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>>(new Tuple<string, Guid, Guid>(stateCode.ToUpper(), platformTypeIdGuid, _businessUnitId), states));
			var alarms = new List<RtaAlarmLight>
			{
				new RtaAlarmLight
				{
					StateGroupId = stateGroupId,
					ActivityId = activityId,
					BusinessUnit = _businessUnitId,
					AlarmTypeId = Guid.NewGuid(),
					StaffingEffect = staffingEffect
				}
			};
			_activityAlarms.Add(new KeyValuePair<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>>(new Tuple<Guid, Guid, Guid>(activityId, stateGroupId, _businessUnitId), alarms));
			return this;
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
			return _schedule.ContainsKey(personId) ? new List<ScheduleLayer> { _schedule[personId] } : new List<ScheduleLayer>();
		}

		public IEnumerable<IActualAgentState> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId)
		{
			yield break;
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
			return null;
		}

		public void PersistActualAgentState(IActualAgentState actualAgentState)
		{
			_actualAgentStates.Add(actualAgentState);
			PersistedActualAgentState = actualAgentState;
		}

		public IEnumerable<PersonOrganizationData> PersonOrganizationData()
		{
			return _personOrganizationData;
		}

	}

}