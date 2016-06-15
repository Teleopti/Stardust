using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAgentStateReadModelPersister : 
		IAgentStateReadModelReader, 
		IAgentStateReadModelPersister
	{
		private readonly INow _now;
		private readonly ConcurrentDictionary<Guid, AgentStateReadModel> _data = new ConcurrentDictionary<Guid, AgentStateReadModel>(); 

		public FakeAgentStateReadModelPersister(INow now)
		{
			_now = now;
		}
		
		public FakeAgentStateReadModelPersister Has(AgentStateReadModel model)
		{
			_data.AddOrUpdate(model.PersonId, model, (g, m) => model);
			return this;
		}

		public IEnumerable<AgentStateReadModel> Models => _data.Values.ToArray();

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

		public IEnumerable<AgentStateReadModel> LoadForSites(IEnumerable<Guid> siteIds)
		{
			return from s in siteIds
				from m in _data.Values
				where s == m.SiteId
				select m;
		}

		public IEnumerable<AgentStateReadModel> LoadForTeams(IEnumerable<Guid> teamIds)
		{
			return from t in teamIds
				from m in _data.Values
				where t == m.TeamId
				select m;
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForSites(IEnumerable<Guid> siteIds)
		{
			return from s in siteIds
				from m in _data.Values
				where s == m.SiteId
					  && m.AlarmStartTime <= _now.UtcDateTime()
				orderby m.AlarmStartTime
				select m;
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForTeams(IEnumerable<Guid> teamIds)
		{
			return from s in teamIds
				from m in _data.Values
				where s == m.TeamId
					  && m.AlarmStartTime <= _now.UtcDateTime()
				orderby m.AlarmStartTime
				select m;
		}
	}
}