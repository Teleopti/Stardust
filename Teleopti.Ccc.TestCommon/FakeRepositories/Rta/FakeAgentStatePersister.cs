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

		public void Has(AgentStateFound model)
		{
			_data.Add(new data
			{
				State = model,
				PersonId = model.PersonId,
				DataSourceId = model.DataSourceId,
				UserCode = model.UserCode
			});
		}

		public AgentState ForPersonId(Guid personId)
		{
			return _data
				.Select(x => x.State)
				.FirstOrDefault(x => x.PersonId == personId);
		}



		public void Prepare(AgentStatePrepare model, DeadLockVictim deadLockVictim)
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

		public void InvalidateSchedules(Guid personId, DeadLockVictim deadLockVictim)
		{
			lock (_lock)
			{
				var existing = _data.Where(x => x.PersonId == personId).ToArray();
				existing.ForEach(x =>
				{
					x.State.Schedule = null;
					x.State.NextCheck = null;
				});
			}
		}

		public IEnumerable<ExternalLogon> FindAll()
		{
			lock (_lock)
				return _data
					.GroupBy(x => x.PersonId, (guid, states) => states.First())
					.Select(x => new ExternalLogon
					{
						DataSourceId = x.DataSourceId,
						UserCode = x.UserCode
					})
					.ToArray();
		}

		public IEnumerable<ExternalLogonForCheck> FindForCheck()
		{
			lock (_lock)
				return _data
					.GroupBy(x => x.PersonId, (guid, states) => states.First())
					.Select(x => new ExternalLogonForCheck
					{
						DataSourceId = x.DataSourceId,
						UserCode = x.UserCode,
						NextCheck = x.State.NextCheck
					})
					.ToArray();
		}

		public IEnumerable<ExternalLogon> FindForClosingSnapshot(DateTime snapshotId, string sourceId, string loggedOutState)
		{
			lock (_lock)
				return _data
					.Select(x => x.State)
					.Where(s => s.SourceId == sourceId && (s.BatchId < snapshotId || s.BatchId == null) && s.StateCode != loggedOutState)
					.GroupBy(x => x.PersonId, (guid, states) => states.First())
					.Select(x => new ExternalLogon
					{
						DataSourceId = x.DataSourceId,
						UserCode = x.UserCode
					})
					.ToArray();
		}

		public void Delete(Guid personId, DeadLockVictim deadLockVictim)
		{
			lock (_lock)
			{
				var existing = _data.Where(x => x.PersonId == personId).ToArray();
				existing.ForEach(x => _data.Remove(x));
			}
		}

		public void Update(AgentState model, bool updateSchedule)
		{
			lock (_lock)
				_data
					.Where(x => x.PersonId == model.PersonId)
					.ForEach(x =>
					{
						var dataSourceId = x.DataSourceId;
						var userCode = x.UserCode;
						var schedule = x.State.Schedule;

						x.State = JsonConvert.DeserializeObject<AgentStateFound>(JsonConvert.SerializeObject(model));
						x.State.DataSourceId = dataSourceId;
						x.State.UserCode = userCode;
						if (!updateSchedule)
							x.State.Schedule = schedule;
					});
		}

		public IEnumerable<AgentStateFound> Find(ExternalLogon externalLogon, DeadLockVictim deadLockVictim)
		{
			lock (_lock)
				return _data
					.Where(x => x.DataSourceId == externalLogon.DataSourceId && x.UserCode == externalLogon.UserCode)
					.Select(x => x.State)
					.ToArray();
		}

		public IEnumerable<AgentStateFound> Find(IEnumerable<ExternalLogon> externalLogons, DeadLockVictim deadLockVictim)
		{
			lock (_lock)
				return _data
					.Where(x => externalLogons.Any(y => y.UserCode == x.UserCode && y.DataSourceId == x.DataSourceId))
					.Select(x => x.State)
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