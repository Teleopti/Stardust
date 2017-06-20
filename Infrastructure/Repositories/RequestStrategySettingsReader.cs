using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class RequestStrategySettingsReader : IRequestStrategySettingsReader
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public RequestStrategySettingsReader(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public int GetIntSetting(string setting, int defaultValue)
		{
			if (setting == "UpdateResourceReadModelIntervalMinutes")
				return 60;

			 if (setting == "BulkRequestTimeoutMinutes")
				return 60;

			const string query = @"select value FROM RequestStrategySettings WHERE setting=:setting";

			var queryResult = _currentUnitOfWork.Session().CreateSQLQuery(query)
				.SetString("setting", setting)
				.UniqueResult();
			
			if (queryResult == null)
			{
				return defaultValue;
			}

			return (int)queryResult;
		}

	}
}
