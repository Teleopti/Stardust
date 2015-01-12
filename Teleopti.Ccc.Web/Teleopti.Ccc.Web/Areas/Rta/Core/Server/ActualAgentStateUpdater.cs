using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class ActualAgentStateUpdater : IActualAgentStateUpdater
	{
		private readonly IDatabaseWriter _databaseWriter;

		public ActualAgentStateUpdater(IDatabaseWriter databaseWriter)
		{
			_databaseWriter = databaseWriter;
		}

		public void Update(StateInfo info)
		{
			var state = info.MakeActualAgentState();
			_databaseWriter.PersistActualAgentState(state);
		}

	}
}