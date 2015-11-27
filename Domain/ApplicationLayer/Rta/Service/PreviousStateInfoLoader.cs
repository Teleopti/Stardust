using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class PreviousStateInfoLoader : IPreviousStateInfoLoader
	{
		private readonly IAgentStateReadModelReader _reader;

		public PreviousStateInfoLoader(IAgentStateReadModelReader reader)
		{
			_reader = reader;
		}

		public StoredStateInfo Load(Guid personId)
		{
			var loaded = _reader.GetCurrentActualAgentState(personId);
			return loaded == null ? null : new StoredStateInfo(personId, loaded);
		}
	}
}