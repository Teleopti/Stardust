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
			return new StoredStateInfo(personId, _reader.GetCurrentActualAgentState(personId));
		}
	}
}