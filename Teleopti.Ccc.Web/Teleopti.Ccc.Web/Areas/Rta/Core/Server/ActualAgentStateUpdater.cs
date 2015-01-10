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

			if (state.StateCode == null) state.StateCode = "";
			state.AlarmStart = ensureValidDatabaseDate(state.AlarmStart);
			state.StateStart = ensureValidDatabaseDate(state.StateStart);
			state.NextStart = ensureValidDatabaseDate(state.NextStart); 

			_databaseWriter.PersistActualAgentState(state);
		}

		private static DateTime ensureValidDatabaseDate(DateTime dateToVerify)
		{
			if (dateToVerify < System.Data.SqlTypes.SqlDateTime.MinValue.Value)
				return System.Data.SqlTypes.SqlDateTime.MinValue.Value;
			if (dateToVerify > System.Data.SqlTypes.SqlDateTime.MaxValue.Value)
				return System.Data.SqlTypes.SqlDateTime.MaxValue.Value;
			return dateToVerify;
		}
	}
}