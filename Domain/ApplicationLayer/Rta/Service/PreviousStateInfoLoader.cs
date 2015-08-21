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

		public PreviousStateInfo Load(Guid personId)
		{
			return new PreviousStateInfo(personId, _reader.GetCurrentActualAgentState(personId));
		}
	}
}