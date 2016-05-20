using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAgentStateStorage : 
		IAgentStateReadModelReader, 
		IAgentStateReadModelPersister, 
		IAgentStatePersister
	{
		private readonly ConcurrentDictionary<Guid, AgentStateReadModel> _data = new ConcurrentDictionary<Guid, AgentStateReadModel>(); 

		public FakeAgentStateStorage()
		{
		}

		public FakeAgentStateStorage(IEnumerable<AgentStateReadModel> data)
		{
			data.ForEach(model =>_data.AddOrUpdate(model.PersonId, model, (g, m) => model));
		}

		public FakeAgentStateStorage Has(AgentStateReadModel model)
		{
			_data.AddOrUpdate(model.PersonId, model, (g, m) => model);
			return this;
		}

		public IEnumerable<AgentStateReadModel> Models { get { return _data.Values.ToArray(); } } 

		public IList<AgentStateReadModel> Load(IEnumerable<IPerson> persons)
		{
			throw new NotImplementedException();
		}

		public IList<AgentStateReadModel> Load(IEnumerable<Guid> personIds)
		{
			throw new NotImplementedException();
		}
		
		public IList<AgentStateReadModel> LoadForTeam(Guid teamId)
		{
			return _data.Values.Where(x => x.TeamId == teamId).ToArray();
		}

		public IEnumerable<AgentStateReadModel> LoadForSites(IEnumerable<Guid> siteIds, bool? inAlarmOnly, bool? alarmTimeDesc)
		{
			var sites = from s in siteIds
				from m in _data.Values
				where s == m.SiteId
				select m;
			sites = inAlarmOnly.HasValue ? sites.Where(x => x.IsAlarm == inAlarmOnly.Value).ToArray() : sites.ToArray();
			return sites;
		}

		public IEnumerable<AgentStateReadModel> LoadForTeams(IEnumerable<Guid> teamIds, bool? inAlarmOnly, bool? alarmTimeDesc)
		{
			return (from t in teamIds
					from m in _data.Values
					where t == m.TeamId
					select m).ToArray();
		}

		public void Persist(AgentStateReadModel model)
		{
			AgentStateReadModel removed;
			_data.TryRemove(model.PersonId, out removed);
			_data.AddOrUpdate(model.PersonId, model, (g, m) => model);

		}

		void IAgentStateReadModelPersister.Delete(Guid personId)
		{
			AgentStateReadModel model;
			_data.TryRemove(personId, out model);
		}





		
		public void Persist(AgentState model)
		{
			throw new NotImplementedException();
		}

		void IAgentStatePersister.Delete(Guid personId)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<AgentState> GetNotInSnapshot(DateTime batchId, string dataSourceId)
		{
			return (from s in _data.Values
					where s.OriginalDataSourceId == dataSourceId &&
						  (s.BatchId < batchId ||
						   s.BatchId == null)
					select new AgentState(s))
				.ToArray();
		}

		public AgentState Get(Guid personId)
		{
			return _data.Values
				.Select(x => new AgentState(x))
				.SingleOrDefault(x => x.PersonId == personId);
		}

		public IEnumerable<AgentState> GetAll()
		{
			return _data.Values
				.Select(x => new AgentState(x))
				.ToArray()
				;
		}





	}
}