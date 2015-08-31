using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStateReadModelUpdater : IAgentStateReadModelUpdater
	{
		private readonly IDatabaseWriter _databaseWriter;
		private readonly IDataSourceScope _dataSourceScope;
		private readonly ICurrentApplicationData _applicationData;

		public AgentStateReadModelUpdater(
			IDatabaseWriter databaseWriter,
			IDataSourceScope dataSourceScope,
			ICurrentApplicationData applicationData)
		{
			_databaseWriter = databaseWriter;
			_dataSourceScope = dataSourceScope;
			_applicationData = applicationData;
		}

		public void Update(StateInfo info)
		{
			var state = info.MakeAgentStateReadModel();
			var dataSource = _applicationData.Current().Tenant(info.Person.Tenant);
			using (_dataSourceScope.OnThisThreadUse(dataSource))
				_databaseWriter.PersistActualAgentReadModel(state);
		}
	}
}