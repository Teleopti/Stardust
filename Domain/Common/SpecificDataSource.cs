using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class SpecificDataSource : ICurrentDataSource
	{
		private readonly IDataSource _dataSource;

		public SpecificDataSource(IDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		public IDataSource Current()
		{
			return _dataSource;
		}

		public string CurrentName()
		{
			return _dataSource.DataSourceName;
		}
	}
}