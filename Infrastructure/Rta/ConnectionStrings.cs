using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class ConnectionStrings : IConnectionStrings
	{
		private readonly ICurrentDataSource _dataSource;

		public ConnectionStrings(ICurrentDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		public string Application()
		{
			return _dataSource.Current().Application.ConnectionString;
		}

		public string Analytics()
		{
			return _dataSource.Current().Statistic.ConnectionString;
		}
	}
}