using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public sealed class FakeDataSource : IDataSource
	{
		public IUnitOfWorkFactory Application { get; set; }
		public IAnalyticsUnitOfWorkFactory Analytics { get; set; }
		public IReadModelUnitOfWorkFactory ReadModel { get; private set; }
		public string DataSourceName { get; set; }

		public FakeDataSource()
		{
		}

		public FakeDataSource(string name)
		{
			DataSourceName = name;
		}

		public void RemoveAnalytics()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
		}
	}
}