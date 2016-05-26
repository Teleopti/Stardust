using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAgentStateReadModelPersister : 
		IAgentStateReadModelReader, 
		IAgentStateReadModelPersister
	{
		private readonly ConcurrentDictionary<Guid, AgentStateReadModel> _data = new ConcurrentDictionary<Guid, AgentStateReadModel>(); 

		public FakeAgentStateReadModelPersister()
		{
		}

		public FakeAgentStateReadModelPersister(IEnumerable<AgentStateReadModel> data)
		{
			data.ForEach(model =>_data.AddOrUpdate(model.PersonId, model, (g, m) => model));
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

		public IEnumerable<AgentStateReadModel> LoadForSites(IEnumerable<Guid> siteIds, bool inAlarm)
		{
			var states = from s in siteIds
				from m in _data.Values
				where s == m.SiteId
				select m;
			if (inAlarm)
				states = states.Where(x => x.IsRuleAlarm)
					.OrderBy(x => x.AlarmStartTime);
			return states;
		}

		public IEnumerable<AgentStateReadModel> LoadForTeams(IEnumerable<Guid> teamIds, bool inAlarm)
		{
			var states = from t in teamIds
				from m in _data.Values
				where t == m.TeamId
				select m;
			if (inAlarm)
				states = states.Where(x => x.IsRuleAlarm)
					.OrderBy(x => x.AlarmStartTime);
			return states;
		}

	}
}