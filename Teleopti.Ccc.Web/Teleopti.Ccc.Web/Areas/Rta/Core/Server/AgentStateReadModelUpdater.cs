using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class AgentStateReadModelUpdater : IAgentStateReadModelUpdater
	{
		private readonly IDatabaseWriter _databaseWriter;

		public AgentStateReadModelUpdater(IDatabaseWriter databaseWriter)
		{
			_databaseWriter = databaseWriter;
		}

		public void Update(StateInfo info)
		{
			var state = info.MakeActualAgentState();
			_databaseWriter.PersistActualAgentReadModel(state);
		}
	}
}