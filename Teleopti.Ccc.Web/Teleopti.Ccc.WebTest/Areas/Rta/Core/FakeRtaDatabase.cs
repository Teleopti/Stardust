using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core
{
	public interface IFakeDataBuilder
	{
		IFakeDataBuilder AddFromState(ExternalUserStateForTest state);
		IFakeDataBuilder AddSource(string sourceId);
		IFakeDataBuilder AddUser(string userCode, Guid personId, Guid businessUnitId);
		IFakeDataBuilder AddSchedule(DateTime start, DateTime end);
		FakeRtaDatabase Done();
	}

	public class FakeRtaDatabase : IDatabaseReader, IDatabaseWriter, IPersonOrganizationReader, IFakeDataBuilder
	{
		private readonly List<KeyValuePair<string, int>> _datasources = new List<KeyValuePair<string, int>>();
		private readonly List<KeyValuePair<string, IEnumerable<PersonWithBusinessUnit>>> _externalLogOns = new List<KeyValuePair<string, IEnumerable<PersonWithBusinessUnit>>>();
		private readonly List<ScheduleLayer> _schedule = new List<ScheduleLayer>();
		 
		public IActualAgentState PersistedActualAgentState { get; set; }

		public IFakeDataBuilder AddFromState(ExternalUserStateForTest state)
		{
			AddSource(state.SourceId);
			AddUser(state.UserCode, Guid.NewGuid(), Guid.NewGuid());
			return this;
		}

		public IFakeDataBuilder AddSource(string sourceId)
		{
			_datasources.Add(new KeyValuePair<string, int>(sourceId, 0));
			return this;
		}

		public IFakeDataBuilder AddUser(string userCode, Guid personId, Guid businessUnitId)
		{
			var lookupKey = string.Format("{0}|{1}", _datasources.Last().Value, userCode).ToUpper(); //putting this logic here is just WRONG
			_externalLogOns.Add(new KeyValuePair<string, IEnumerable<PersonWithBusinessUnit>>(lookupKey, new[] { new PersonWithBusinessUnit { PersonId = personId, BusinessUnitId = businessUnitId } }));
			return this;
		}

		public IFakeDataBuilder AddSchedule(DateTime start, DateTime end)
		{
			_schedule.Add(new ScheduleLayer {PayloadId = Guid.NewGuid(), StartDateTime = start, EndDateTime = end});
			return this;
		}

		public FakeRtaDatabase Done()
		{
			return this;
		}

		public IActualAgentState GetCurrentActualAgentState(Guid personId)
		{
			return null;
		}

		public ConcurrentDictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>> StateGroups()
		{
			return new ConcurrentDictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>>();
		}

		public ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>> ActivityAlarms()
		{
			return new ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>>();
		}

		public IList<ScheduleLayer> GetCurrentSchedule(Guid personId)
		{
			return _schedule;
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
			PersistedActualAgentState = actualAgentState;
		}

		public IEnumerable<PersonOrganizationData> PersonOrganizationData()
		{
			yield break;
		}
	}

}