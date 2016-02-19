using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public sealed class FakeDataSource : IDataSource
	{
		public IUnitOfWorkFactory Application { get; set; }
		public IUnitOfWorkFactory Analytics { get; set; }
		public IReadModelUnitOfWorkFactory ReadModel { get; private set; }
		public string DataSourceName { get; set; }

		public FakeDataSource()
		{
		}

		public FakeDataSource(string name)
		{
			DataSourceName = name;
		}

		public void ResetStatistic()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
		}
	}
}