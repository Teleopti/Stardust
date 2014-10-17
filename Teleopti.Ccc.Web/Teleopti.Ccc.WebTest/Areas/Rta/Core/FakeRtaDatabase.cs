using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core
{
	public class FakeRtaDatabase : IDatabaseReader, IDatabaseWriter, IPersonOrganizationReader
	{
		private readonly List<KeyValuePair<string, int>> _datasources = new List<KeyValuePair<string, int>>();
		private readonly List<KeyValuePair<string, IEnumerable<PersonWithBusinessUnit>>> _externalLogOns = new List<KeyValuePair<string, IEnumerable<PersonWithBusinessUnit>>>();

		public IActualAgentState PersistedActualAgentState { get; set; }
		 
		public void AddTestData(string sourceId, string userCode, Guid personId, Guid businessUnitId)
		{
			const int datasourceId = 0;
			_datasources.Add(new KeyValuePair<string, int>(sourceId, datasourceId));
			var lookupKey = string.Format("{0}|{1}", datasourceId, userCode).ToUpper(); //putting this logic here is just WRONG
			_externalLogOns.Add(new KeyValuePair<string, IEnumerable<PersonWithBusinessUnit>>(lookupKey, new[] { new PersonWithBusinessUnit { PersonId = personId, BusinessUnitId = businessUnitId } }));

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
			return new List<ScheduleLayer>();
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