using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeAgentStateReadModelPersister : IAgentStateReadModelPersister
	{
		private IList<AgentStateReadModel> _data = new List<AgentStateReadModel>();

		public void PersistActualAgentReadModel(AgentStateReadModel model)
		{
			var existing = from m in _data
						   where m.PersonId == model.PersonId
						   select m;
			_data.Remove(existing.SingleOrDefault());
			_data.Add(model);
		}

		public void Delete(Guid personId)
		{
			_data = _data.Where(x => x.PersonId != personId).ToList();
		}

		public IEnumerable<AgentStateReadModel> Models { get { return _data; } } 
	}
}