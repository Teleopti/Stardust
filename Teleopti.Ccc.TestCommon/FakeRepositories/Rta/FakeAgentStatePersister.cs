using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NHibernate.Util;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAgentStatePersister : IAgentStatePersister
	{
		private readonly object _lock = new object();
		private readonly List<data> _data = new List<data>();

		private class data
		{
			public AgentStateFound State { get; set; }
			public Guid PersonId { get; set; }
			public int DataSourceId { get; set; }
			public string UserCode { get; set; }
		}

		public void Prepare(AgentStatePrepare model)
		{
			lock(_lock)
				model.ExternalLogons.ForEach(x =>
				{
					_data.Add(new data
					{
						State = new AgentStateFound
						{
							PersonId = model.PersonId,
							BusinessUnitId = model.BusinessUnitId,
							SiteId = model.SiteId,
							TeamId = model.TeamId,
							DataSourceId = x.DataSourceId,
							UserCode = x.UserCode
						},
						PersonId = model.PersonId,
						DataSourceId = x.DataSourceId,
						UserCode = x.UserCode
					});
				});
		}

		public void InvalidateSchedules(Guid personId)
		{
			throw new NotImplementedException();
		}

		public void Delete(Guid personId)
		{
			lock (_lock)
			{
				var existing = _data.Where(x => x.PersonId == personId).ToArray();
				existing.ForEach(x => _data.Remove(x));
			}
		}

		public void Update(AgentState model)
		{
			lock (_lock)
				_data
					.Where(x => x.PersonId == model.PersonId)
					.ForEach(x =>
					{
						var state = JsonConvert.DeserializeObject<AgentStateFound>(JsonConvert.SerializeObject(model));
						state.DataSourceId = x.DataSourceId;
						state.UserCode = x.UserCode;
						x.State = state;
					});
		}

		public IEnumerable<AgentStateFound> Find(int dataSourceId, string userCode)
		{
			lock (_lock)
				return _data
					.Where(x => x.DataSourceId == dataSourceId && x.UserCode == userCode)
					.Select(x => x.State)
					.ToArray();
		}

		public IEnumerable<AgentStateFound> Find(int dataSourceId, IEnumerable<string> userCodes)
		{
			lock (_lock)
				return _data
					.Where(x => dataSourceId == x.DataSourceId && userCodes.Any(userCode => userCode == x.UserCode))
					.Select(x => x.State)
					.ToArray();
		}

		public IEnumerable<Guid> GetAllPersonIds()
		{
			return GetStates().Select(x => x.PersonId).ToArray();
		}

		public AgentState Get(Guid personId)
		{
			lock (_lock)
				return _data
					.Select(x => x.State)
					.FirstOrDefault(x => x.PersonId == personId);
		}

		public IEnumerable<AgentState> GetStatesNotInSnapshot(DateTime snapshotId, string sourceId)
		{
			lock (_lock)
				return _data
					.Select(x => x.State)
					.Where(s => s.SourceId == sourceId && (s.BatchId < snapshotId || s.BatchId == null))
					.GroupBy(x => x.PersonId, (guid, states) => states.First())
					.ToArray();
		}

		public IEnumerable<AgentState> Get(IEnumerable<Guid> personIds)
		{
			lock (_lock)
				return _data
					.Where(x => personIds.Contains(x.PersonId))
					.Select(x => x.State)
					.GroupBy(x => x.PersonId, (guid, states) => states.First())
					.ToArray();
		}

		public IEnumerable<AgentState> GetStates()
		{
			lock (_lock)
				return _data
					.Select(x => x.State)
					.GroupBy(x => x.PersonId, (guid, states) => states.First())
					.ToArray();
		}

	}
}