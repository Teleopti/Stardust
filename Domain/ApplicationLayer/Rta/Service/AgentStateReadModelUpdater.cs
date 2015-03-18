namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStateReadModelUpdater : IAgentStateReadModelUpdater
	{
		private readonly IDatabaseWriter _databaseWriter;

		public AgentStateReadModelUpdater(IDatabaseWriter databaseWriter)
		{
			_databaseWriter = databaseWriter;
		}

		public void Update(StateInfo info, string tenant)
		{
			var state = info.MakeActualAgentState();
			_databaseWriter.PersistActualAgentReadModel(state, tenant);
		}
	}
}