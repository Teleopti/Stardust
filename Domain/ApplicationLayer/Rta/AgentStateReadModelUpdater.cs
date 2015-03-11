namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
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