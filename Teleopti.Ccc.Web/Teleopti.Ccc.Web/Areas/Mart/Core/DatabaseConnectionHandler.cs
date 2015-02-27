using System.Data.SqlClient;
using System.Threading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Mart.Core
{
	public interface IDatabaseConnectionHandler
	{
		SqlConnection MartConnection(string name, int latency);
	}

	public class DatabaseConnectionHandler : IDatabaseConnectionHandler
	{
		private readonly IApplicationData _applicationData;

		public DatabaseConnectionHandler(IApplicationData applicationData)
		{
			_applicationData = applicationData;
		}

		public SqlConnection MartConnection(string name, int latency)
		{
			if (latency > 0)
				Thread.Sleep(latency);
			return new SqlConnection(_applicationData.DataSource(name).Statistic.ConnectionString);
		}
	}
}