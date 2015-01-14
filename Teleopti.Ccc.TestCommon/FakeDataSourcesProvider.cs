using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeDataSourcesProvider : IAvailableDataSourcesProvider
	{
		private IEnumerable<IDataSource> _dataSources;

		public void SetAvailableDataSources(IEnumerable<IDataSource> dataSources)
		{
			_dataSources = dataSources;
		}

		public IEnumerable<IDataSource> AvailableDataSources()
		{
			return _dataSources;
		}

		public IEnumerable<IDataSource> UnavailableDataSources()
		{
			return null;
		}
	}
}