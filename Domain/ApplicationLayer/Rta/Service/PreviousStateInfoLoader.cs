using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class PreviousStateInfoLoader : IPreviousStateInfoLoader
	{
		private readonly IAgentStateReadModelPersister _persister;

		public PreviousStateInfoLoader(IAgentStateReadModelPersister persister)
		{
			_persister = persister;
		}

		public StoredStateInfo Load(Guid personId)
		{
			var loaded = _persister.GetCurrentActualAgentState(personId);
			return loaded == null ? null : new StoredStateInfo(personId, loaded);
		}
	}
}